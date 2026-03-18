# Comet State Management — The Definitive Guide

> **Audience:** Contributors, maintainers, and advanced users of the Comet MVU framework.
> **Source of truth:** All code references are verified against `src/Comet/` source files.
> **Last updated:** 2025-01 (based on .NET 10 / MAUI 10 era)

---

## Table of Contents

1. [Mental Model: How Comet State Actually Works](#1-mental-model-how-comet-state-actually-works)
2. [Core Types](#2-core-types)
3. [How State Is Discovered](#3-how-state-is-discovered)
4. [Build-Time Tracking vs Binding-Time Tracking](#4-build-time-tracking-vs-binding-time-tracking)
5. [Change Routing: Global Reloads vs Targeted Property Updates](#5-change-routing-global-reloads-vs-targeted-property-updates)
6. [Binding Edge Cases and Pitfalls](#6-binding-edge-cases-and-pitfalls)
7. [State Batching](#7-state-batching)
8. [Components](#8-components)
9. [Environment as State Input](#9-environment-as-state-input)
10. [Hot Reload State Transfer](#10-hot-reload-state-transfer)
11. [Lifecycle, Cleanup, and Performance](#11-lifecycle-cleanup-and-performance)
12. [Threading & Concurrency](#12-threading--concurrency)
13. [Appendix: Source Generation](#13-appendix-source-generation)

---

## 1. Mental Model: How Comet State Actually Works

Comet's state management is built around a simple but powerful idea: **record which observable properties are read while building UI, then use that read-set to route later writes**.

This gives the framework two critical capabilities:

1. **Targeted property updates** when a binding maps cleanly to a single control property (e.g., `Text` binding to `State<string>.Value`)
2. **Full view rebuilds** when a state read affects view structure, composition, or multi-property expressions

The key insight: Comet is **neither** a plain MVVM binding system **nor** a naive "always rerender on any change" MVU engine. It's a **hybrid dependency-tracking system** that classifies state usage and chooses the cheapest safe update path.

### The Three Core Differentiators

1. **Implicit read tracking.** When a view's `Body` lambda executes, every `State<T>.Value` access is intercepted by `StateManager`, building a dependency graph on-the-fly. No manual `Subscribe()` calls, no explicit annotations.

2. **Two-tier change routing.** Changes to state properties that feed into a lambda `Binding<T>` trigger a targeted `ViewPropertyChanged` (handler update only). Changes to properties read during Body evaluation trigger a full `Reload` (rebuild + diff). This eliminates unnecessary work.

3. **Binding stabilization.** After a `Func<T>` binding re-evaluates and the set of read properties hasn't changed, the binding marks itself `_bindingStable` and skips property tracking on subsequent evaluations. This is a fast path that avoids `StartProperty`/`EndProperty` overhead entirely. **Critical limitation:** once stable, the binding does NOT continue tracking dependency changes. If the Func's logic would cause it to read different properties, those new dependencies are never registered until the binding is recreated.

### Terminology Reference

| Term | Definition |
|------|------------|
| **Global property** | A state property read during `Body` evaluation. Changes trigger a full view `Reload()`. "Global" means the entire view body depends on it, not that all views in the app are affected. |
| **Bound property** | A state property read inside a `Func<T>` binding. Changes trigger targeted `ViewPropertyChanged(propertyName, value)` on the control displaying the binding. |
| **Monitoring view** | The view whose `Body` created a binding (tracked by `Binding.BoundFromView`). |
| **Target view** | The control that displays a binding's value (tracked by `Binding.View`). |
| **Build-time tracking** | Dependency capture during `Body.Invoke()` via `StateBuilder`. |
| **Binding-time tracking** | Dependency capture during `Func<T>` evaluation via `StartProperty()`/`EndProperty()`. |

---

## 2. Core Types

### 2.1 INotifyPropertyRead

Standard .NET has `INotifyPropertyChanged` for write notifications. Comet extends this with **read** notifications, enabling automatic dependency tracking:

```csharp
// BindingObject.cs
public interface INotifyPropertyRead : INotifyPropertyChanged
{
	event PropertyChangedEventHandler PropertyRead;
}
```

Both `PropertyRead` and `PropertyChanged` fire through `BindingObject`'s protected helpers, which route through `StateManager` before raising the CLR events:

```csharp
// BindingObject.cs
protected virtual void CallPropertyRead(string propertyName)
{
	StateManager.OnPropertyRead(this, propertyName);
	if (PropertyRead != null)
		PropertyRead.Invoke(this, GetCachedArgs(propertyName));
}

protected virtual void CallPropertyChanged<T>(string propertyName, T value)
{
	StateManager.OnPropertyChanged(this, propertyName, value);
	if (PropertyChanged != null)
		PropertyChanged.Invoke(this, GetCachedArgs(propertyName));
}
```

The `PropertyChangedEventArgs` are cached per property name to avoid repeated allocations (`_argsCache` dictionary).

**Important:** `CallPropertyRead` happens during **get** operations, `CallPropertyChanged` happens during **set** operations. The read tracking is what enables Comet to know which properties a view or binding depends on.

### 2.2 BindingObject

The base class for all observable objects in Comet. Provides a property bag (`dictionary`), typed get/set helpers, and fires both read and changed notifications.

```csharp
// BindingObject.cs
public class BindingObject : INotifyPropertyRead, IAutoImplemented
{
	internal protected Dictionary<string, object> dictionary = new Dictionary<string, object>();

	protected T GetProperty<T>(T defaultValue = default, [CallerMemberName] string propertyName = "")
	{
		CallPropertyRead(propertyName);  // Track reads for dependency graph
		if (dictionary.TryGetValue(propertyName, out var val))
			return (T)val;
		return defaultValue;
	}

	protected bool SetProperty<T>(T value, [CallerMemberName] string propertyName = "")
	{
		if (dictionary.TryGetValue(propertyName, out object val))
		{
			if (val is T typedVal && EqualityComparer<T>.Default.Equals(typedVal, value))
				return false;   // no-op if equal
		}
		dictionary[propertyName] = value;
		CallPropertyChanged<T>(propertyName, value);
		return true;
	}

	internal virtual (bool hasValue, object value) GetValueInternal(string propertyName)
	{
		if (string.IsNullOrWhiteSpace(propertyName))
			return (false, null);
		var hasValue = dictionary.TryGetValue(propertyName, out var val);
		return (hasValue, val);
	}
}
```

**`IAutoImplemented` marker interface:** When `StateManager` detects this interface, it skips subscribing to `PropertyChanged`/`PropertyRead` events. Why? Because `BindingObject` subclasses already call `StateManager.OnPropertyRead`/`OnPropertyChanged` directly in their methods. Subscribing to the events would cause **double-dispatch** — the same property change would be processed twice.

External `INotifyPropertyRead` implementors that aren't `IAutoImplemented` do get event subscriptions in `StateManager.StartMonitoringCore()`.

### 2.3 State\<T\>

The primary reactive container for typed mutable state. Most common way to declare state in a Comet view.

```csharp
// State.cs
public class State<T> : BindingObject
{
	T _value;
	bool _hasValue;
	static readonly string ValuePropertyName = "Value";

	public State(T value)
	{
		_value = value;
		_hasValue = true;
		dictionary[ValuePropertyName] = value;  // Dual storage: field + dictionary
	}

	public State() { }

	public T Value
	{
		get
		{
			CallPropertyRead(ValuePropertyName);
			return _hasValue ? _value : default;
		}
		set
		{
			// Fast typed equality check — no dictionary lookup, no boxing
			if (_hasValue && EqualityComparer<T>.Default.Equals(_value, value))
				return;
			_value = value;
			_hasValue = true;
			CallPropertyChanged(ValuePropertyName, value);
			ValueChanged?.Invoke(value);
		}
	}

	// Override to return typed value without dictionary lookup (perf optimization)
	internal override (bool hasValue, object value) GetValueInternal(string propertyName)
	{
		if (propertyName == ValuePropertyName)
			return (_hasValue, _value);
		return base.GetValueInternal(propertyName);
	}

	// Implicit conversions for seamless API integration
	public static implicit operator T(State<T> state) => state.Value;
	public static implicit operator Action<T>(State<T> state) => value => state.Value = value;
	public static implicit operator State<T>(T value) => new State<T>(value);

	public Action<T> ValueChanged { get; set; }  // Direct callback, bypasses INPC
}
```

**Key design points:**

| Feature | Rationale |
|---------|-----------|
| **Dual storage** | `_value` field for fast typed access, `dictionary[ValuePropertyName]` for `BindingObject` compatibility |
| **`_hasValue` flag** | Distinguishes uninitialized state (`new State<string>()`) from initialized-to-default (`new State<string>(null)`) |
| **`GetValueInternal` fast path** | Avoids dictionary lookup for the common "Value" property access |
| **Typed equality** | `EqualityComparer<T>.Default` — no boxing for value types |
| **`ValueChanged` callback** | For imperative subscribers who don't need full INPC |
| **Implicit operators** | `readonly State<int> count = 0;` works seamlessly |

**Usage example:**

```csharp
public class Counter : View
{
	readonly State<int> count = 0;  // implicit conversion from int

	[Body]
	View body() => new VStack
	{
		new Text(() => $"Count: {count.Value}"),
		new Button("Increment", () => count.Value++),
	};
}
```

### 2.4 Binding\<T\>

The bridge between state and view properties. A `Binding<T>` can wrap either a **raw value** (`IsValue = true`) or a **lambda `Func<T>`** (`IsFunc = true`).

```csharp
// Binding.cs (base class)
public class Binding
{
	public object Value { get; protected set; }
	public bool IsValue { get; internal set; }
	public bool IsFunc { get; internal set; }

	WeakReference _view;
	internal View View { get; set; }  // target view (weak)

	WeakReference _boundFromView;
	internal View BoundFromView { get; set; }  // monitoring view (weak)

	public IReadOnlyList<(INotifyPropertyRead BindingObject, string PropertyName)> BoundProperties { get; protected set; }
	internal bool IsDirty { get; set; }  // Used during batching

	public virtual void BindingValueChanged(INotifyPropertyRead bindingObject, string propertyName, object value)
	{
		Value = value;
		View?.ViewPropertyChanged(propertyName, value);
	}
}

// Binding.cs (generic class)
public class Binding<T> : Binding
{
	Func<T> Get { get; set; }
	Action<T> _set;
	bool _bindingStable;

	public Action<T> Set
	{
		get => _set ?? (_set = (v) => CurrentValue = v);
		internal set => _set = value;
	}

	public T CurrentValue { get => Value == null ? default : (T)Value; private set => Value = value; }
}
```

**Critical distinction: `BoundFromView` vs `View`**

- **`BoundFromView`** (monitoring view): The view whose `Body` lambda created the binding. This is the view that "owns" the state field(s) being read.
- **`View`** (target view): The control that displays the binding's computed value (e.g., a `Text` control).

**Why this matters:** Bindings are registered in the **monitoring view's** `BindingState`, NOT the target view's. This prevents **double-dispatch**: without this separation, both the monitoring view and the target view would end up in `NotifyToViewMappings`, causing `OnPropertyChanged` to dispatch to both. The monitoring view would cascade to the target, AND the target would process bindings directly — double work. By registering in the monitoring view's state, `OnPropertyChanged` dispatches once to the monitoring view, which finds the binding and calls `EvaluateAndNotify` → `ViewPropertyChanged` on the target view. Single dispatch path, no redundancy.

**Example scenario:**

```csharp
public class ParentView : View
{
	readonly State<string> name = "Alice";

	[Body]
	View body() => new Text(() => $"Hello, {name.Value}!");
}
```

- **Monitoring view (BoundFromView):** `ParentView` (owns the `name` field)
- **Target view (View):** The generated `Text` control instance
- **Binding registration:** Happens in `ParentView.GetState().ViewUpdateProperties`, keyed by `(name, "Value")`

When `name.Value` changes → `StateManager.OnPropertyChanged` dispatches to `ParentView` → `ParentView.BindingPropertyChanged` looks up the binding → binding calls `EvaluateAndNotify` → `Text.ViewPropertyChanged("Text", "Hello, Alice!")`.

### 2.5 BindingState

Each `View` has a `BindingState` that tracks:
1. **Global properties** — state reads that affect the entire view body (trigger `Reload()`)
2. **View update properties** — targeted bindings that map to specific control properties (trigger `ViewPropertyChanged`)
3. **Changed properties** — used for hot reload state transfer

```csharp
// BindingObject.cs
public class BindingState
{
	public IEnumerable<KeyValuePair<string, object>> ChangedProperties => changeDictionary;
	Dictionary<string, object> changeDictionary = new Dictionary<string, object>();

	// Properties that trigger a full view reload (read during Body evaluation)
	public HashSet<(INotifyPropertyRead BindingObject, string PropertyName)> GlobalProperties { get; set; }
		= new HashSet<(INotifyPropertyRead BindingObject, string PropertyName)>();

	// Property → Bindings that need targeted updates
	public Dictionary<(INotifyPropertyRead BindingObject, string PropertyName), 
		HashSet<(string PropertyName, Binding Binding)>> ViewUpdateProperties
		= new Dictionary<(INotifyPropertyRead BindingObject, string PropertyName), 
			HashSet<(string PropertyName, Binding Binding)>>();

	public void AddGlobalProperty((INotifyPropertyRead BindingObject, string PropertyName) property)
	{
		GlobalProperties.Add(property);
	}

	public void AddViewProperty((INotifyPropertyRead BindingObject, string PropertyName) property, 
		string propertyName, Binding binding)
	{
		if (!ViewUpdateProperties.TryGetValue(property, out var actions))
			ViewUpdateProperties[property] = actions = new HashSet<(string PropertyName, Binding Binding)>();
		actions.Add((propertyName, binding));
	}

	public bool UpdateValue<T>(View view, (INotifyPropertyRead BindingObject, string PropertyName) property, 
		string fullProperty, T value, out bool bindingsHandled)
	{
		changeDictionary[fullProperty] = value;
		// Walk parent chain to propagate changed property dict up the tree
		if (view.Parent != null)
			UpdatePropertyChangeProperty(view, fullProperty, value);

		bindingsHandled = false;

		// Check for targeted bindings
		if (ViewUpdateProperties.TryGetValue((property.BindingObject, property.PropertyName), out var bindings))
		{
			bindingsHandled = true;
			var count = bindings.Count;
			var bindingsArray = System.Buffers.ArrayPool<(string PropertyName, Binding Binding)>.Shared.Rent(count);
			bindings.CopyTo(bindingsArray);
			try
			{
				for (var i = 0; i < count; i++)
				{
					var binding = bindingsArray[i];
					binding.Binding.BindingValueChanged(property.BindingObject, binding.PropertyName, value);
				}
			}
			finally
			{
				System.Buffers.ArrayPool<(string PropertyName, Binding Binding)>.Shared.Return(bindingsArray, true);
			}
		}

		// Check if this is a global property (requires full reload)
		if (GlobalProperties.Contains(property))
		{
			return false;  // Signal: full reload needed
		}
		return true;  // Signal: no reload needed
	}
}
```

**Three-way routing logic:**

1. **If `ViewUpdateProperties` contains the property:** Call all registered bindings' `BindingValueChanged` methods (targeted updates), set `bindingsHandled = true`.
2. **If `GlobalProperties` contains the property:** Return `false` to signal a full `Reload()` is needed.
3. **Neither:** Return `true` with `bindingsHandled = false`, signaling a fallback `ViewPropertyChanged` should fire.

This return value is critical for the caller (`View.BindingPropertyChanged`) to decide what to do next.

---

## 3. How State Is Discovered

Comet uses two mechanisms to discover stateful fields on a view or binding object: **`State<T>` type detection** and **`[State]` attribute annotation**. Both trigger runtime inspection via `StateManager.CheckForStateAttributes`.

### 3.1 CheckForStateAttributes

Called during view construction (`StateManager.ConstructingView`) and when monitoring a new binding object (`StateManager.StartMonitoringCore`).

```csharp
// StateManager.cs (simplified)
static IEnumerable<INotifyPropertyRead> CheckForStateAttributes(object obj, View view)
{
	var type = obj.GetType();
	var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
		.Where(x => (x.FieldType.Assembly == CometAssembly && x.FieldType.Name == "State`1") 
		         || Attribute.IsDefined(x, typeof(StateAttribute)))
		.ToList();

	if (fields.Any())
	{
		foreach (var field in fields)
		{
			// ENFORCE READONLY
			if (!field.IsInitOnly)
			{
				throw new ReadonlyRequiresException(field.DeclaringType?.FullName, field.Name);
			}

			var fieldValue = field.GetValue(obj);
			var child = fieldValue as INotifyPropertyRead;
			if (child != null)
			{
				RegisterChild(view, child, field.Name);
				yield return child;
			}
		}
	}
}
```

**Key behaviors:**

1. **Reflection-based discovery:** Finds all fields of type `State<T>` or annotated with `[State]`.
2. **Readonly enforcement:** Throws `ReadonlyRequiresException` if a state field is not `readonly`. This is critical because reassigning a state field would break the dependency graph — the old `State<T>` instance is registered in `NotifyToViewMappings`, but the new instance is not.
3. **Child registration:** Calls `RegisterChild(view, child, fieldName)` to set up `ChildPropertyNamesMapping` and subscribe to property change notifications.

**Example triggering readonly enforcement:**

```csharp
public class BadView : View
{
	State<int> count = 0;  // NOT readonly — will throw at construction time

	[Body]
	View body() => new Text(() => $"{count.Value}");
}
// Throws: ReadonlyRequiresException("BadView", "count")
```

### 3.2 RegisterChild and Nested Property Names

When a `BindingObject` contains a nested `INotifyPropertyRead` field (e.g., a view contains a `State<T>` or a custom state class with `[State]` fields), `RegisterChild` establishes the mapping:

```csharp
// StateManager.cs
public static void RegisterChild(View view, INotifyPropertyRead value, string fieldName)
{
	bool needsMonitoring;
	_rwLock.EnterWriteLock();
	try
	{
		ChildPropertyNamesMapping.GetOrCreateForKey(value)[view?.Id ?? ""] = fieldName;
		needsMonitoring = !MonitoredObjects.Contains(value);
		if (needsMonitoring)
			MonitoredObjects.Add(value);
	}
	finally
	{
		_rwLock.ExitWriteLock();
	}
	if (needsMonitoring)
		StartMonitoringCore(value);
}
```

**`ChildPropertyNamesMapping` purpose:** When a nested object's property changes, `StateManager.OnPropertyChanged` needs to know the "full path" to report to the view. For example, if `MyView` has a field `readonly MyStateClass state;` and `MyStateClass` has a `State<string> name;`, then a change to `name.Value` should be reported to `MyView` as `"state.Name.Value"`, not just `"Value"`.

The `ResolvePropertyName` helper builds these paths:

```csharp
// StateManager.cs
static string ResolvePropertyName(string parentProperty, string propertyName)
{
	if (string.IsNullOrWhiteSpace(parentProperty))
		return propertyName;
	var cacheKey = (parentProperty, propertyName);
	if (!_propertyNameCache.TryGetValue(cacheKey, out var prop))
	{
		prop = string.Concat(parentProperty, ".", propertyName);
		_propertyNameCache[cacheKey] = prop;
	}
	return prop;
}
```

This allows the view to receive property change notifications like `"state.name.Value"` which can then be used for targeted binding updates if needed.

### 3.3 [Body] Attribute Runtime Discovery

Views that use the `[Body]` attribute (instead of setting `Body` directly in the constructor) rely on runtime reflection to wire up the method. This happens in `View.CheckForBody()`:

```csharp
// View.cs (simplified)
void CheckForBody()
{
	if (didCheckForBody)
		return;
	if (usedEnvironmentData.Any())
		PopulateFromEnvironment();
	StateManager.CheckBody(this);
	didCheckForBody = true;
	if (Body != null)
		return;
	var bodyMethod = this.GetBody();  // Reflection helper
	if (bodyMethod != null)
		Body = bodyMethod;
}
```

`GetBody()` is an extension method that scans for methods marked `[Body]` using `GetDeepMethodInfo(typeof(BodyAttribute))` and creates a `Func<View>` delegate via `Delegate.CreateDelegate`. This is handled **entirely at runtime** — no source generator is involved with `[Body]`. The `AutoNotifyGenerator` only processes `[AutoNotify]` fields (see §13).

---

## 4. Build-Time Tracking vs Binding-Time Tracking

Comet captures dependencies at **two distinct points** in the rendering lifecycle, each with different routing implications.

### 4.1 Build-Time Tracking (Global Properties)

When a view's `Body` lambda executes, any `State<T>.Value` reads are captured as **global properties** — the entire view body depends on them, so changes trigger a full rebuild.

```
┌────────────────────────────────────────────────────────────────┐
│  View.GetRenderView()                                          │
│    ├─ new StateBuilder(view)  ←── pushes view onto stack       │
│    │    └─ StateManager.StartBuilding(view)                    │
│    ├─ Body.Invoke()                                            │
│    │    └─ any State<T>.Value access fires:                    │
│    │         CallPropertyRead("Value")                         │
│    │           └─ StateManager.OnPropertyRead(obj, prop)       │
│    │                └─ appends to _currentReadProperties       │
│    ├─ props = StateManager.EndProperty()                       │
│    │    └─ returns accumulated reads, clears list              │
│    ├─ State.AddGlobalProperties(props)                         │
│    └─ StateBuilder.Dispose()                                   │
│         └─ StateManager.EndBuilding(view)                      │
└────────────────────────────────────────────────────────────────┘
```

**Example:**

```csharp
public class ConditionalView : View
{
	readonly State<bool> showDetails = false;

	[Body]
	View body() => showDetails.Value  // ← Read happens here
		? new Text("Details here")
		: new Text("Summary");
}
```

`showDetails` becomes a **global property** because it affects view structure. When `showDetails.Value` changes, Comet calls `View.Reload()`, which:
1. Calls `Body.Invoke()` to get the new view tree
2. Diffs the new tree against the old
3. Updates handlers in-place where possible
4. Disposes removed views

### 4.2 Binding-Time Tracking (Targeted Updates)

When a `Binding<T>` wraps a `Func<T>`, it tracks reads during the **function's first evaluation**. These become **bound properties** — changes trigger targeted `ViewPropertyChanged` on the control displaying the value.

```csharp
// Binding.cs
protected void ProcessGetFunc()
{
	StateManager.StartProperty();
	var result = Get == null ? default : Get.Invoke();
	var props = StateManager.EndProperty();
	IsFunc = true;
	CurrentValue = result;
	BoundProperties = props;
	BoundFromView = StateManager.CurrentView;
}
```

`StartProperty()` sets `_isTrackingProperties = true` and flushes any accumulated reads to the current view's global properties. `EndProperty()` returns the reads captured during the Func evaluation, deduplicating them for the multi-property case.

**Example:**

```csharp
public class GreetingView : View
{
	readonly State<string> firstName = "Alice";
	readonly State<string> lastName = "Smith";

	[Body]
	View body() => new Text(() => $"{firstName.Value} {lastName.Value}");
	// ↑ Lambda binding — reads tracked separately from body
}
```

The `Text` constructor receives a `Binding<string>` (implicit conversion from `Func<string>`). During `ProcessGetFunc()`:
- `firstName.Value` read → captured
- `lastName.Value` read → captured
- `BoundProperties = [(firstName, "Value"), (lastName, "Value")]`

When `firstName.Value` changes → `StateManager.OnPropertyChanged` → `GreetingView.BindingPropertyChanged` → looks up binding in `ViewUpdateProperties[(firstName, "Value")]` → calls `binding.EvaluateAndNotify()` → `Text.ViewPropertyChanged("Text", "Alice Johnson")`.

**No full rebuild.** The diff algorithm never runs. The `Text` control's handler updates in-place.

### 4.3 Comparison Table

| Aspect | Build-Time (Global) | Binding-Time (Targeted) |
|--------|---------------------|-------------------------|
| **When** | During `Body.Invoke()` | During `Func<T>` evaluation in binding constructor |
| **Scope** | `StateBuilder` using block | `StartProperty()`/`EndProperty()` pair |
| **Registered in** | `BindingState.GlobalProperties` | `BindingState.ViewUpdateProperties` |
| **Change triggers** | `View.Reload()` — full rebuild + diff | `View.ViewPropertyChanged(property, value)` — handler update only |
| **Threading** | Always on main thread (Body eval) | Can happen off main thread (Func creation), but changes are dispatched to main thread |
| **Example** | `return showFlag ? viewA : viewB;` | `new Text(() => name.Value)` |

---

## 5. Change Routing: Global Reloads vs Targeted Property Updates

When a `BindingObject` property changes, the notification flows through `StateManager.OnPropertyChanged`, which dispatches to all views monitoring that object. Each view's `BindingPropertyChanged` method then consults `BindingState.UpdateValue` to determine the appropriate response.

### 5.1 The Dispatch Flow

```
Property Change
    ↓
BindingObject.CallPropertyChanged<T>(propertyName, value)
    ↓
StateManager.OnPropertyChanged(sender, propertyName, value)
    ├─ Look up views in NotifyToViewMappings[sender]
    ├─ Resolve nested property name via ChildPropertyNamesMapping
    ├─ For each view: view.BindingPropertyChanged(sender, propertyName, fullProperty, value)
    │     ↓
    │  BindingState.UpdateValue(view, property, fullProperty, value, out bindingsHandled)
    │     ├─ Check ViewUpdateProperties → call binding.BindingValueChanged() → targeted update
    │     ├─ Check GlobalProperties → return false → full reload
    │     └─ Neither → return true, bindingsHandled=false → fallback ViewPropertyChanged
    │
    └─ If batching: defer reload to EndBatch(), else execute immediately
```

### 5.2 UpdateValue Three-Way Routing

```csharp
// BindingObject.cs (actual implementation)
public bool UpdateValue<T>(View view, (INotifyPropertyRead BindingObject, string PropertyName) property, 
	string fullProperty, T value, out bool bindingsHandled)
{
	changeDictionary[fullProperty] = value;
	if (view.Parent != null)
		UpdatePropertyChangeProperty(view, fullProperty, value);

	bindingsHandled = false;

	// Route 1: Targeted bindings
	if (ViewUpdateProperties.TryGetValue((property.BindingObject, property.PropertyName), out var bindings))
	{
		bindingsHandled = true;
		var count = bindings.Count;
		var bindingsArray = System.Buffers.ArrayPool<(string PropertyName, Binding Binding)>.Shared.Rent(count);
		bindings.CopyTo(bindingsArray);
		try
		{
			for (var i = 0; i < count; i++)
			{
				var binding = bindingsArray[i];
				binding.Binding.BindingValueChanged(property.BindingObject, binding.PropertyName, value);
			}
		}
		finally
		{
			System.Buffers.ArrayPool<(string PropertyName, Binding Binding)>.Shared.Return(bindingsArray, true);
		}
	}

	// Route 2: Global property → needs reload
	if (GlobalProperties.Contains(property))
	{
		return false;
	}

	return true;
}
```

**Return value meanings:**

- **`false`** (global property): Caller should trigger `Reload()`.
- **`true` + `bindingsHandled = true`**: Targeted updates handled, nothing more to do.
- **`true` + `bindingsHandled = false`**: No bindings registered, fall through to `ViewPropertyChanged`.

### 5.3 View.BindingPropertyChanged

The view method that receives the notification and decides what to do:

```csharp
// View.cs
internal void BindingPropertyChanged<T>(INotifyPropertyRead bindingObject, string property, 
	string fullProperty, T value)
{
	try
	{
		if (!State.UpdateValue(this, (bindingObject, property), fullProperty, value, out bool bindingsHandled))
		{
			// Global property changed → full reload
			if (StateManager.IsBatching)
				StateManager.AddViewNeedingReload(this);
			else
				Reload(false);
		}
		else if (!StateManager.IsBatching && !bindingsHandled)
		{
			// No bindings, not global → fallback to property update
			ViewPropertyChanged(property, value);
		}
	}
	catch (Exception ex)
	{
		Logger.Error(ex);
	}
}
```

**Batching note:** If `StateManager.IsBatching` is true, the view is added to `_viewsNeedingReload` and the reload is deferred until `EndBatch()`. This prevents redundant rebuilds when multiple state properties change in quick succession.

### 5.4 OnPropertyChanged Multi-View Dispatch

When a single `INotifyPropertyRead` object is monitored by multiple views (e.g., a shared state class), `StateManager.OnPropertyChanged` must notify all of them. It uses `ArrayPool` to avoid allocations:

```csharp
// StateManager.cs (multi-view path)
HashSet<View> views;
Dictionary<string, string> mappings;

_rwLock.EnterReadLock();
try
{
	if (!NotifyToViewMappings.TryGetValue(notify, out views))
		return;
	if (views.Count == 0)
		return;
	ChildPropertyNamesMapping.TryGetValue(notify, out mappings);
}
finally
{
	_rwLock.ExitReadLock();
}

// Multi-view path: use ArrayPool
View[] viewsCopy = null;
int viewCount;

_rwLock.EnterReadLock();
try
{
	viewCount = views.Count;
	viewsCopy = System.Buffers.ArrayPool<View>.Shared.Rent(viewCount);
	views.CopyTo(viewsCopy);
}
finally
{
	_rwLock.ExitReadLock();
}

List<View> disposedViews = null;
try
{
	for (int i = 0; i < viewCount; i++)
	{
		var view = viewsCopy[i];
		if (view == null || view.IsDisposed)
		{
			disposedViews ??= new List<View>();
			disposedViews.Add(view);
			continue;
		}
		string parentproperty = null;
		if (mappings != null && mappings.Count > 0 && !mappings.TryGetValue(view.Id, out parentproperty))
		{
			parentproperty = mappings.First().Value;
		}
		var prop = ResolvePropertyName(parentproperty, propertyName);
		view.BindingPropertyChanged(notify, propertyName, prop, value);
	}
}
finally
{
	if (viewsCopy != null)
		System.Buffers.ArrayPool<View>.Shared.Return(viewsCopy, true);
}

// Clean up disposed views from the mapping
if (disposedViews?.Count > 0)
{
	_rwLock.EnterWriteLock();
	try
	{
		if (NotifyToViewMappings.TryGetValue(notify, out var viewsForCleanup))
		{
			foreach (var view in disposedViews)
				viewsForCleanup.Remove(view);
		}
	}
	finally
	{
		_rwLock.ExitWriteLock();
	}
}
```

**Fast path for single view:** If `views.Count == 1`, the code extracts the single view inside the read lock and processes it directly without ArrayPool overhead.

**Important:** The `ViewPropertyChanged` call happens **directly**, with no guaranteed `RunOnMainThread` wrapper. This means binding updates can occur on any thread that calls `SetProperty`. However, most view properties are set on the main thread by convention, and handler updates are safe to defer. If you're setting state from a background thread, wrap it in `ThreadHelper.RunOnMainThread(() => state.Value = newValue)`.

---

## 6. Binding Edge Cases and Pitfalls

### 6.1 Direct Value Bindings vs Lambda Bindings

The `Binding<T>(T value)` implicit conversion has complex fallback logic in `BindToProperty` that developers often misunderstand.

#### Case 1: Single State\<T\> Read → Optimized Direct Binding

```csharp
readonly State<string> name = "Alice";

new Text(name.Value);  // Implicit: Binding<string> = name.Value
```

When `StateManager.EndProperty()` returns `[(name, "Value")]` and the single property is a `State<T>` with a matching type, the `Binding<T>(T value)` operator delegates to the `State<T>` implicit conversion:

```csharp
// Binding.cs
public static implicit operator Binding<T>(T value)
{
	var props = StateManager.EndProperty();
	// ...
	else if (props?.Count == 1 && props[0].BindingObject is State<T> state)
	{
		return state;  // Calls State<T>.operator Binding<T>
	}
	// ...
}
```

The `State<T>.operator Binding<T>` creates a true lambda binding with a setter:

```csharp
// State.cs
public static implicit operator Binding<T>(State<T> state)
{
	StateManager.StartProperty();
	var result = state.Value;
	var props = StateManager.EndProperty();

	var binding = new Binding<T>(
		getValue: () => state.Value,
		setValue: (v) => { state.Value = v; })
	{
		CurrentValue = result,
		BoundProperties = props,
		IsFunc = true,
	};
	return binding;
}
```

**Result:** Two-way binding, fully reactive, zero allocations per change.

#### Case 2: Formatted String / Multiple State Reads → Global Fallback

```csharp
readonly State<string> firstName = "Alice";
readonly State<string> lastName = "Smith";

new Text($"{firstName.Value} {lastName.Value}");  // PITFALL!
```

`EndProperty()` returns `[(firstName, "Value"), (lastName, "Value")]`. The `Binding<string>(string value)` operator sees `props.Count > 1` and falls back to **global property** registration:

```csharp
// Binding.cs
public static implicit operator Binding<T>(T value)
{
	var props = StateManager.EndProperty();
	if (props?.Count > 1)
	{
		StateManager.CurrentView.GetState().AddGlobalProperties(props);
	}
	// ...
	return new Binding<T>()
	{
		IsValue = true,
		CurrentValue = value,
		BoundProperties = props,
		BoundFromView = StateManager.CurrentView
	};
}
```

Then in `BindToProperty` (the `IsValue` branch), the code detects multiple properties and logs a warning:

```csharp
// Binding.cs, BindToProperty method
if (IsValue)
{
	bool isGlobal = BoundProperties?.Count > 1;
	// ...
	if (BoundProperties?.Count == 1)
	{
		var stateValue = prop.BindingObject.GetPropertyValue(prop.PropertyName).Cast<T>();
		if (EqualityComparer<T>.Default.Equals(stateValue, CurrentValue))
		{
			// 1-to-1 binding established
			Set = (v) => { prop.BindingObject.SetPropertyValue(prop.PropertyName, v); CurrentValue = v; };
			StateManager.UpdateBinding(this, view);
			view.GetState().AddViewProperty(prop, property, this);
			Debug.WriteLine($"Databinding: {property} to {prop}");
		}
		else
		{
			var errorMessage = $"Warning: {property} is using formated Text. For performance reasons, please switch to a Lambda. i.e new Text(()=> \"Hello\")";
			if (Debugger.IsAttached)
			{
				Logger.Fatal(errorMessage);
			}
			Debug.WriteLine(errorMessage);
			isGlobal = true;
		}
	}
	else
	{
		var errorMessage = $"Warning: {property} is using Multiple state Variables. For performance reasons, please switch to a Lambda.";
		isGlobal = true;
		Debug.WriteLine(errorMessage);
	}

	if (isGlobal)
	{
		StateManager.UpdateBinding(this, BoundFromView);
		BoundFromView.GetState().AddGlobalProperties(BoundProperties);
	}
	// ...
}
```

**Result:** Any change to `firstName` or `lastName` triggers a full `Reload()` of the parent view, which rebuilds the `Text` control from scratch. Much slower than a targeted update.

**Fix:** Use a lambda:

```csharp
new Text(() => $"{firstName.Value} {lastName.Value}");
```

Now the `Func<string>` tracks both properties, and the binding calls `EvaluateAndNotify()` on each change, which re-evaluates the lambda and updates the `Text` control's handler property. No full rebuild.

#### Case 3: Formatted String with Single State → Also Global

```csharp
readonly State<int> count = 0;

new Text($"Count: {count.Value}");  // PITFALL!
```

`EndProperty()` returns `[(count, "Value")]`, but the **value** is the string `"Count: 0"`, not the integer `0`. When `BindToProperty` checks `EqualityComparer<string>.Default.Equals(stateValue, CurrentValue)`:

- `stateValue` = `0` (from `count.Value`)
- `CurrentValue` = `"Count: 0"` (the interpolated string)

They don't match, so the code logs "Warning: ... using formatted Text" and marks it global.

**Fix:** Use a lambda:

```csharp
new Text(() => $"Count: {count.Value}");
```

### 6.2 Reading State Outside Build Context

Reading `State<T>.Value` outside a `StateBuilder` block or `StartProperty()`/`EndProperty()` pair does **nothing**:

```csharp
public class BrokenView : View
{
	readonly State<int> count = 0;

	public BrokenView()
	{
		// Constructor runs BEFORE StateBuilder
		var initial = count.Value;  // ← NOT TRACKED
		Console.WriteLine($"Initial count: {initial}");
	}

	[Body]
	View body() => new Text(() => $"{count.Value}");  // ← TRACKED
}
```

The constructor read happens before `StateManager.ConstructingView(this)` is called, so `OnPropertyRead` sees `IsBuilding = false` and ignores it. This is usually harmless (constructor logic rarely needs reactivity), but can be confusing when debugging.

### 6.3 Non-Readonly State Fields

```csharp
public class BadView : View
{
	State<int> count = 0;  // Missing 'readonly'

	[Body]
	View body()
	{
		if (someCondition)
			count = new State<int>(100);  // BREAKS DEPENDENCY GRAPH
		return new Text(() => $"{count.Value}");
	}
}
```

When `CheckForStateAttributes` runs in the constructor, it registers the **original** `State<int>` instance in `NotifyToViewMappings`. If you reassign `count` to a new instance, the old instance is still registered but no longer reachable from the view. Changes to the new instance are never dispatched.

**Enforcement:** `CheckForStateAttributes` throws `ReadonlyRequiresException` if a `State<T>` field is not `readonly`.

### 6.4 Binding Stabilization Limitation

After a `Binding<T>` marks itself `_bindingStable = true`, it **stops tracking dependency changes**. If the Func's logic later causes it to read different properties, those new dependencies are **never registered** until the binding is recreated.

```csharp
readonly State<bool> showFirstName = true;
readonly State<string> firstName = "Alice";
readonly State<string> lastName = "Smith";

[Body]
View body() => new Text(() => showFirstName.Value ? firstName.Value : lastName.Value);
```

**Initial evaluation:**
- Reads: `showFirstName.Value`, `firstName.Value`
- `BoundProperties = [(showFirstName, "Value"), (firstName, "Value")]`
- After re-evaluation with same properties → `_bindingStable = true`

**When `showFirstName.Value` changes to `false`:**
- Binding re-evaluates the Func
- But `_bindingStable = true`, so `EvaluateAndNotify` **skips** `StartProperty()`/`EndProperty()`
- The Func now reads `lastName.Value`, but this is **not tracked**
- Changes to `lastName.Value` do **not** trigger the binding until it destabilizes

**When does it destabilize?** Only if the binding is recreated (e.g., the parent view rebuilds and calls `BindToProperty` again).

**Workaround:** Ensure all possible reads happen in every evaluation:

```csharp
new Text(() => {
	var flag = showFirstName.Value;
	var first = firstName.Value;  // Always read
	var last = lastName.Value;     // Always read
	return flag ? first : last;
});
```

Now `BoundProperties` always contains all three, and stabilization doesn't miss any.

**Why not always re-track?** Performance. Re-running `StartProperty()`/`EndProperty()` on every change would double the cost of property change notifications. Stabilization is a critical fast path for the 99% case where a binding's dependencies are static.

---

## 7. State Batching

Comet provides two batching mechanisms: **StateManager-level** (global) and **view-level** (per-view).

### 7.1 StateManager Batching

Used when multiple state mutations should trigger a single render pass. Implemented via `BeginBatch()`/`EndBatch()`.

```csharp
// StateManager.cs
static int _batchDepth;
static readonly List<Binding> _dirtyBindings = new List<Binding>();
static readonly HashSet<View> _viewsNeedingReload = new HashSet<View>();

public static bool IsBatching => _batchDepth > 0;

public static void BeginBatch()
{
	_batchDepth++;
}

public static void EndBatch()
{
	if (--_batchDepth <= 0)
	{
		_batchDepth = 0;
		FlushBatch();
	}
}

static void FlushBatch()
{
	// Flush dirty bindings first
	if (_dirtyBindings.Count > 0)
	{
		for (int i = 0; i < _dirtyBindings.Count; i++)
			_dirtyBindings[i].Flush();
		_dirtyBindings.Clear();
	}

	// Then reload views that had global property changes
	if (_viewsNeedingReload.Count > 0)
	{
		foreach (var v in _viewsNeedingReload)
		{
			if (!v.IsDisposed)
				v.Reload();
		}
		_viewsNeedingReload.Clear();
	}
}
```

**Usage example:**

```csharp
readonly State<string> firstName = "Alice";
readonly State<string> lastName = "Smith";
readonly State<int> age = 30;

void UpdatePerson(Person p)
{
	StateManager.BeginBatch();
	try
	{
		firstName.Value = p.FirstName;
		lastName.Value = p.LastName;
		age.Value = p.Age;
	}
	finally
	{
		StateManager.EndBatch();
	}
}
```

Without batching, the view would reload three times (if these are global properties) or update three times (if they're bound properties). With batching, the reload or update happens once after all three changes.

**Nesting:** `BeginBatch()` can be called multiple times; only the outermost `EndBatch()` flushes.

### 7.2 Binding Batching

During `StateManager` batching, `Binding<T>.BindingValueChanged` defers Func re-evaluation:

```csharp
// Binding.cs
public override void BindingValueChanged<TVal>(INotifyPropertyRead bindingObject, string propertyName, TVal value)
{
	if (IsFunc && StateManager.IsBatching)
	{
		if (!IsDirty)
		{
			IsDirty = true;
			StateManager.AddDirtyBinding(this);
		}
		return;
	}
	EvaluateAndNotify(bindingObject, propertyName, value);
}
```

When the batch ends, `FlushBatch()` calls `Flush()` on each dirty binding, which re-evaluates the Func once and updates the view.

### 7.3 View-Level Batching

Each `View` has its own `BatchBegin()`/`BatchCommit()` methods for batching **property changes on the view itself** (not state changes). This is used when setting multiple environment or context properties at once.

```csharp
// View.cs
private bool _isBatching;
private readonly List<(string property, object value)> _batchedChanges = new List<(string, object)>();

public void BatchBegin()
{
	_isBatching = true;
}

public void BatchCommit()
{
	_isBatching = false;
	if (_batchedChanges.Count == 0)
		return;

	var changes = _batchedChanges.ToList();
	_batchedChanges.Clear();

	foreach (var (property, value) in changes)
	{
		try
		{
			this.SetPropertyValue(property, value);
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Error setting batched property:{property} : {value} on :{this}");
			Debug.WriteLine(ex);
		}
	}

	// Single handler update and layout invalidation
	foreach (var (property, _) in changes)
	{
		var newPropName = GetHandlerPropertyName(property);
		ViewHandler?.UpdateValue(newPropName);
	}

	InvalidateMeasurement();
}
```

**When to use:** When programmatically setting multiple view properties (e.g., `view.SetEnvironment("Color", Colors.Red)`, `view.SetEnvironment("FontSize", 18)`) and you want a single handler update instead of two.

**Note:** This is distinct from `StateManager` batching. View-level batching defers `SetPropertyValue` and handler updates. StateManager batching defers binding re-evaluations and view reloads.

---

## 8. Components

Comet provides three `Component` base classes for MauiReactor-style patterns: `Component`, `Component<TState>`, and `Component<TState, TProps>`. All extend `View`, so they participate in the existing lifecycle (handlers, hot reload, diffing).

### 8.1 Component (Stateless)

```csharp
// Component.cs
public abstract class Component : View, IComponentWithState
{
	bool _mounted;

	protected Component()
	{
		Body = () => Render();
	}

	public abstract View Render();

	protected virtual void OnMounted() { }
	protected virtual void OnWillUnmount() { }

	protected override void OnLoaded()
	{
		base.OnLoaded();
		if (!_mounted)
		{
			_mounted = true;
			OnMounted();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && _mounted)
		{
			_mounted = false;
			OnWillUnmount();
		}
		base.Dispose(disposing);
	}

	public virtual object GetStateObject() => null;
	public virtual void TransferStateFrom(IComponentWithState source) { }
}
```

**Key points:**

- **`Body = () => Render()`**: Wires the abstract `Render()` method into the existing `View` rendering pipeline.
- **Lifecycle hooks**: `OnMounted()` fires once when the component gets a handler, `OnWillUnmount()` fires during disposal.
- **No state management**: This base class has no state. If you need state, use `Component<TState>`.

### 8.2 Component\<TState\> (Mutable State)

```csharp
// Component.cs
public abstract class Component<TState> : Component, IComponentWithState
	where TState : class, new()
{
	TState _state;

	public new TState State => _state ??= new TState();

	protected void SetState(Action<TState> mutator)
	{
		if (mutator == null)
			throw new ArgumentNullException(nameof(mutator));

		var state = State;

		StateManager.BeginBatch();
		try
		{
			mutator(state);
		}
		finally
		{
			StateManager.EndBatch();
		}

		// Trigger re-render on the main thread
		ThreadHelper.RunOnMainThread(() => Reload());
	}

	internal void MergeStateFrom(Component<TState> oldComponent)
	{
		if (oldComponent?._state != null)
		{
			_state = oldComponent._state;
		}
	}

	public override object GetStateObject() => _state;

	public override void TransferStateFrom(IComponentWithState source)
	{
		if (source is Component<TState> typed && typed._state != null)
		{
			_state = typed._state;
		}
	}
}
```

**Key points:**

- **`TState` is a plain class**: Does NOT extend `BindingObject`. It's just a POCO with properties.
- **`SetState(Action<TState>)`**: Mutates the state object in-place, then calls `Reload()`. This is a **coarse-grained** update model — every `SetState` call triggers a full rebuild of the component's subtree.
- **No fine-grained reactive tracking**: Unlike `State<T>.Value` changes, which can trigger targeted binding updates, `Component<TState>` always rebuilds. This is by design — it's a simpler programming model for component-local state.
- **Batching**: `SetState` wraps mutations in `StateManager.BeginBatch()`/`EndBatch()` so multiple property changes trigger one reload.
- **State transfer**: `TransferStateFrom` is called during hot reload to carry over the old component's state to the new instance.

**Usage example:**

```csharp
public class CounterComponent : Component<CounterState>
{
	public override View Render() => new VStack
	{
		new Text(() => $"Count: {State.Count}"),
		new Button("Increment", () => SetState(s => s.Count++)),
	};
}

public class CounterState
{
	public int Count { get; set; }
}
```

### 8.3 Component\<TState, TProps\> (With Props)

```csharp
// Component.cs
public abstract class Component<TState, TProps> : Component<TState>, IComponentWithState
	where TState : class, new()
	where TProps : class, new()
{
	TProps _props;

	public TProps Props
	{
		get => _props ??= new TProps();
		set
		{
			_props = value ?? new TProps();
			ThreadHelper.RunOnMainThread(() => Reload());
		}
	}

	internal void UpdatePropsFromDiff(TProps newProps)
	{
		if (newProps == null)
			newProps = new TProps();
		_props = newProps;
	}

	protected virtual bool ShouldUpdate(TProps oldProps, TProps newProps)
	{
		return true;  // Default: always re-render
	}

	public override void TransferStateFrom(IComponentWithState source)
	{
		base.TransferStateFrom(source);
		if (source is Component<TState, TProps> typed && typed._props != null)
		{
			_props = typed._props;
		}
	}
}
```

**Key points:**

- **Props are parent-supplied**: The parent component sets `childComponent.Props = new MyProps { ... }`, which triggers a reload.
- **`UpdatePropsFromDiff` is internal**: Called by the diff algorithm when reconciling same-type components. Sets props without triggering an immediate reload (the diff cycle handles rendering).
- **`ShouldUpdate` is defined but NOT wired into a live call path**: In the examined source, this method exists as an extension point, but no code calls it. It's a placeholder for future optimization where components could skip re-renders if props haven't meaningfully changed.
- **State + Props separation**: State is internal and mutable (via `SetState`), Props are external and read-only (by convention).

**Important distinction from View + State\<T\>:**

| Aspect | View + State\<T\> | Component\<TState\> |
|--------|-------------------|---------------------|
| **State type** | `State<T>` (extends `BindingObject`) | Plain POCO class |
| **Change detection** | `INotifyPropertyRead` + dependency tracking | None — `SetState` always rebuilds |
| **Update granularity** | Targeted (binding) or global (body read) | Always global (full component rebuild) |
| **Batching** | Automatic via `StateManager` | Manual via `SetState` wrapper |
| **Complexity** | Higher — developer must understand tracking | Lower — just mutate and call `SetState` |

**When to use which:**

- Use **`State<T>` in a View**: When you need fine-grained reactivity and control over which changes trigger rebuilds vs binding updates.
- Use **`Component<TState>`**: When you want a simpler, React-like programming model where every state change rebuilds the component.

---

## 9. Environment as State Input

Comet's environment system is tightly integrated with state management. `EnvironmentData` (the backing store for environment properties) extends `BindingObject`, making environment properties **reactive**.

### 9.1 Environment Structure

```csharp
// ContextualObject.cs (simplified)
public class ContextualObject
{
	EnvironmentData currentContext = new EnvironmentData();
	EnvironmentData parent;

	protected internal static EnvironmentData Environment = new EnvironmentData();
}

// EnvironmentData.cs (simplified)
public class EnvironmentData : BindingObject
{
	Dictionary<string, object> data = new Dictionary<string, object>();

	public T GetValue<T>(string key, bool cascades = true)
	{
		if (data.TryGetValue(key, out var value))
			return (T)value;
		if (cascades && Parent != null)
			return Parent.GetValue<T>(key, cascades);
		return default;
	}

	public void SetValue(string key, object value, bool cascades)
	{
		data[key] = value;
		CallPropertyChanged(key, value);
	}
}
```

### 9.2 Environment Property Reads as Global Properties

When a view's `Body` reads an environment property via `GetEnvironment<T>(key)`:

```csharp
public T GetEnvironment<T>(string property, bool cascades = true)
{
	var value = currentContext.GetValue<T>(property, cascades);
	if (value == null && cascades && parent != null)
		value = parent.GetValue<T>(property, cascades);
	return value;
}
```

The `EnvironmentData.GetValue` call does **not** fire `CallPropertyRead`, so it's not automatically tracked. However, views that want to react to environment changes call `StateManager.ListenToEnvironment(view)`:

```csharp
// StateManager.cs
internal static void ListenToEnvironment(View view)
{
	_rwLock.EnterWriteLock();
	try
	{
		NotifyToViewMappings.GetOrCreateForKey(View.Environment).Add(view);
	}
	finally
	{
		_rwLock.ExitWriteLock();
	}
}
```

This registers the view in the global `View.Environment`'s notification set. Any change to a global environment property via `SetGlobalEnvironment` triggers `ViewPropertyChanged` on all registered views.

### 9.3 SetGlobalEnvironment Fan-Out

```csharp
// View.cs
public static void SetGlobalEnvironment(string key, object value)
{
	Environment.SetValue(key, value, true);
	ThreadHelper.RunOnMainThread(() => {
		List<View> views;
		lock (ActiveViewsLock)
			views = ActiveViews.OfType<View>().ToList();
		views.ForEach(x => x.ViewPropertyChanged(key, value));
	});
}
```

**Critical behavior:** This calls `ViewPropertyChanged` on **every active view** in the app, whether or not they actually read that environment key. This is inefficient but simple. Views that don't use the key ignore the call (their handler properties don't have a mapping for it).

### 9.4 PopulateFromEnvironment

When a view's `Body` uses an `[Environment]` attribute to declare a dependency on an environment key, the value is populated during `CheckForBody()` or `ResetView()`:

```csharp
// View.cs
void SetEnvironmentFields()
{
	var fields = this.GetFieldsWithAttribute(typeof(EnvironmentAttribute));
	if (!fields.Any())
		return;
	foreach (var f in fields)
	{
		var attribute = f.GetCustomAttributes(true).OfType<EnvironmentAttribute>().FirstOrDefault();
		var key = attribute.Key ?? f.Name;
		usedEnvironmentData.Add((f.Name, key));
		State.AddGlobalProperty((View.Environment, key));
	}
}
```

The `usedEnvironmentData` set tracks which environment keys this view depends on. `State.AddGlobalProperty` immediately registers each key as a global dependency — meaning changes to these environment keys trigger a full view reload. When `PopulateFromEnvironment` runs, it re-reads these keys and updates the fields.

**Fallback order in `PopulateFromEnvironment`:**

1. **Context lookup**: `GetEnvironment<T>(key)` (checks local context, then parent chain, then global)
2. **DI container**: If the key is a type name, tries `MauiContext?.Services.GetService<T>()`
3. **Hot reload replaced view**: If a replaced view exists, tries `replacedView.GetEnvironment<T>(key)`
4. **Uppercase-key retry**: If the key starts with a lowercase letter, retries with the first char uppercased

This allows environment properties to be populated from DI, which is a common pattern for dependency injection into views.

---

## 10. Hot Reload State Transfer

When a view type is hot-reloaded, Comet transfers the **old view's state** to the **new view instance** using the `BindingState.ChangedProperties` dictionary.

### 10.1 Hot Reload Flow

```
User edits code → IDE sends type replacement → MauiHotReloadHelper.RegisterReplacedView(className, newType)
    ↓
MauiHotReloadHelper.TriggerReload() fires IHotReloadableView.Reload() on all active views
    ↓
View.Reload(isHotReload: true)
    ↓
ResetView(isHotReload: true)
    ├─ GetRenderView() creates new view tree using updated type
    ├─ Diff(oldView, newView, isHotReload: true)
    │    └─ DatabindingExtensions.Diff checks MauiHotReloadHelper.IsReplacedView(oldType, newType)
    │         └─ If same logical type → TransferHotReloadStateTo(newView)
    └─ oldView.Dispose()
```

### 10.2 State Transfer

State transfer is split across two methods. `SetHotReloadReplacement` (View.cs:330-344) handles the **structural** transfer — handler, navigation, parent, environment:

```csharp
internal void SetHotReloadReplacement(View replacement, bool transferState = true)
{
	replacement.viewThatWasReplaced = this;
	replacement.ViewHandler = ViewHandler;
	replacement.Navigation = Navigation;
	replacement.Parent = this;
	replacement.ReloadHandler = ReloadHandler;
	replacement.PopulateFromEnvironment();
	if (transferState)
		TransferHotReloadStateTo(replacement);
	replacedView = replacement;
}
```

Then `TransferHotReloadStateTo` handles **data** transfer via the `BindingState.ChangedProperties` dictionary:

```csharp
// View.cs — actual source
internal void TransferHotReloadStateTo(View newView)
{
	if (newView == null)
		return;
	TransferHotReloadStateToCore(newView);
}

protected virtual void TransferHotReloadStateToCore(View newView)
{
	var oldState = this.GetState();
	if (oldState == null)
		return;
	var changes = oldState.ChangedProperties;
	foreach (var change in changes)
	{
		newView.SetDeepPropertyValue(change.Key, change.Value);
	}
}
```

**Key points:**

- **`TransferHotReloadStateTo` is `internal`**, not `protected virtual`. The extensibility point is `TransferHotReloadStateToCore`.
- **`State.ChangedProperties`** contains all property values set during the view's lifetime, populated by `BindingState.UpdateValue`.
- **Uses `SetDeepPropertyValue`** (not `SetPropertyValue`) to handle nested property paths like `"myState.Value"`.
- **Handler transfer and environment repopulation** happen in `SetHotReloadReplacement`, NOT in `TransferHotReloadStateTo`.
- **Component state transfer** for `Component<TState>` overrides `TransferHotReloadStateToCore` to copy the `TState` object reference.

### 10.3 IComponentWithState

```csharp
// Public interface (src/Comet/IComponentWithState.cs)
public interface IComponentWithState
{
	object GetStateObject();
	void TransferStateFrom(IComponentWithState source);
}
```

Used by the diff algorithm to detect components and transfer their state during reconciliation (both hot reload and normal parent rebuilds).

---

## 11. Lifecycle, Cleanup, and Performance

### 11.1 View Lifecycle Events

```csharp
// View.cs
public event EventHandler Loaded;
public event EventHandler Unloaded;
public event EventHandler<HandlerChangingEventArgs> HandlerChanging;
public event EventHandler HandlerChanged;

protected virtual void OnHandlerChanging(IElementHandler oldHandler, IElementHandler newHandler)
{
	HandlerChanging?.Invoke(this, new HandlerChangingEventArgs(oldHandler, newHandler));
}

protected virtual void OnLoaded()
{
	Loaded?.Invoke(this, EventArgs.Empty);
}

protected virtual void OnUnloaded()
{
	Unloaded?.Invoke(this, EventArgs.Empty);
}
```

**When they fire:**

- **`Loaded`**: When a view's `ViewHandler` transitions from `null` to non-null (the view becomes part of the live visual tree).
- **`Unloaded`**: When a view's `ViewHandler` transitions from non-null to `null` (the view is removed from the tree).
- **`HandlerChanging`** / **`HandlerChanged`**: Fired around any handler change (including load/unload).

### 11.2 Disposal Flow

```csharp
// View.cs — actual source
protected virtual void Dispose(bool disposing)
{
	if (!disposing)
		return;

	lock (ActiveViewsLock)
		ActiveViews.Remove(this);

	// Clean up gestures
	var gestures = Gestures;
	if (gestures?.Any() ?? false)
		foreach (var g in gestures)
			ViewHandler?.Invoke(Gesture.RemoveGestureProperty, g);

	MauiHotReloadHelper.UnRegister(this);

	try
	{
		var vh = ViewHandler;
		ViewHandler = null;
		(vh as IDisposable)?.Dispose();
		replacedView?.Dispose();
		replacedView = null;
		builtView?.Dispose();
		builtView = null;
		body = null;
		Context(false)?.Clear();
		StateManager.Disposing(this);     // ← Critical: unregisters from all tracking
		VisualStateManager.ClearVisualStateGroups(this);
		State?.Clear();
	}
	finally
	{
		State = null;
	}
}
```

> **Note:** Reentrancy is guarded by a separate `OnDispose(bool)` wrapper that sets `disposedValue = true` before calling `Dispose(bool)`. The public `Dispose()` calls `OnDispose(true)`.

`StateManager.Disposing` is the critical cleanup method:

```csharp
// StateManager.cs
public static void Disposing(View view)
{
	List<INotifyPropertyRead> toStopMonitoring = null;
	_rwLock.EnterWriteLock();
	try
	{
		if (ViewObjectMappings.TryGetValue(view.Id, out var mappings))
		{
			foreach (var obj in mappings)
			{
				if (NotifyToViewMappings.TryGetValue(obj, out var views))
				{
					views.Remove(view);
					if (views.Count == 0)
					{
						NotifyToViewMappings.Remove(obj);
						if (MonitoredObjects.Remove(obj))
						{
							toStopMonitoring ??= new List<INotifyPropertyRead>();
							toStopMonitoring.Add(obj);
						}
						ChildPropertyNamesMapping.Remove(obj);
					}
				}
			}
			ViewObjectMappings.Remove(view.Id);
		}
	}
	finally
	{
		_rwLock.ExitWriteLock();
	}

	// Unsubscribe events outside lock
	if (toStopMonitoring != null)
	{
		foreach (var obj in toStopMonitoring)
		{
			if (!(obj is IAutoImplemented))
			{
				obj.PropertyChanged -= Obj_PropertyChanged;
				obj.PropertyRead -= Obj_PropertyRead;
			}
		}
	}
}
```

**Cleanup steps:**

1. Remove view from `NotifyToViewMappings` for each of its monitored objects.
2. If a monitored object has zero views remaining, remove it from `NotifyToViewMappings`, `MonitoredObjects`, and `ChildPropertyNamesMapping`.
3. Unsubscribe from `PropertyChanged`/`PropertyRead` events (outside the lock to avoid reentrancy).

### 11.3 Weak References

`Binding` uses weak references for both `View` and `BoundFromView`:

```csharp
// Binding.cs
WeakReference _view;
internal View View
{
	get => _view?.Target as View;
	set => _view = new WeakReference(value);
}

WeakReference _boundFromView;
internal View BoundFromView
{
	get => _boundFromView?.Target as View;
	set => _boundFromView = new WeakReference(value);
}
```

**Why:** Bindings can outlive views (they're stored in `BindingState.ViewUpdateProperties` which is held by the monitoring view). If the target view is disposed but the binding remains, the weak reference allows the view to be garbage collected. The binding's `BindingValueChanged` call will see `View == null` and no-op.

**Parent reference** is also weak:

```csharp
// View.cs
WeakReference parent;
public View Parent
{
	get => parent?.Target as View;
	set => parent = new WeakReference(value);
}
```

This prevents circular strong references (parent holds child's handler, child holds parent reference).

### 11.4 ArrayPool Usage

`OnPropertyChanged` uses `ArrayPool` when dispatching to multiple views:

```csharp
// StateManager.cs
View[] viewsCopy = null;
int viewCount;

_rwLock.EnterReadLock();
try
{
	viewCount = views.Count;
	viewsCopy = System.Buffers.ArrayPool<View>.Shared.Rent(viewCount);
	views.CopyTo(viewsCopy);
}
finally
{
	_rwLock.ExitReadLock();
}

// ... process views ...

finally
{
	if (viewsCopy != null)
		System.Buffers.ArrayPool<View>.Shared.Return(viewsCopy, true);
}
```

**Why:** Copying the `HashSet<View>` to an array allows the dispatch loop to run outside the lock. Using `ArrayPool` avoids allocating a new `View[]` on every property change. The `true` parameter to `Return` clears the array to prevent holding stale view references.

`BindingState.UpdateValue` uses the same pattern for the bindings array.

### 11.5 Lock Strategy

`StateManager` uses a single `ReaderWriterLockSlim` for all shared state:

- **Read locks:** Querying `NotifyToViewMappings`, `ViewObjectMappings`, `ChildPropertyNamesMapping`
- **Write locks:** Modifying these dictionaries, adding/removing monitored objects

**Critical rule:** Never call back into user code while holding the lock. This is why `OnPropertyChanged` copies the view set to an array (under read lock), releases the lock, then calls `view.BindingPropertyChanged()`.

**Fast path optimization:** Single-view dispatch extracts the view inside the read lock without using ArrayPool:

```csharp
// StateManager.cs
View singleView = null;

_rwLock.EnterReadLock();
try
{
	if (!NotifyToViewMappings.TryGetValue(notify, out views))
		return;
	if (views.Count == 0)
		return;
	if (views.Count == 1)
	{
		using var enumerator = views.GetEnumerator();
		singleView = enumerator.MoveNext() ? enumerator.Current : null;
	}
}
finally
{
	_rwLock.ExitReadLock();
}

if (singleView != null)
{
	// Process single view without ArrayPool
}
```

---

## 12. Threading & Concurrency

### 12.1 Thread-Static Tracking State

The property tracking state is thread-local to support concurrent view builds on different threads:

```csharp
// StateManager.cs
[ThreadStatic] static WeakStack<View> _viewStack;
[ThreadStatic] static List<(INotifyPropertyRead bindingObject, string property)> _currentReadProperties;
[ThreadStatic] static bool _isTrackingProperties;
```

**Why thread-static:**

- Multiple views can build simultaneously (e.g., in a list virtualization scenario).
- Each thread needs its own tracking context to avoid cross-contamination.
- The `[ThreadStatic]` attribute ensures each thread gets a separate instance of these fields (initialized to null on first access).

### 12.2 Main Thread Requirements

**Handler updates** are not automatically dispatched to the main thread. `View.ViewPropertyChanged` calls `ViewHandler?.UpdateValue(propertyName)` directly:

```csharp
// View.cs
public virtual void ViewPropertyChanged(string property, object value)
{
	// ... set property value ...
	var newPropName = GetHandlerPropertyName(property);
	ViewHandler?.UpdateValue(newPropName);  // ← NOT wrapped in RunOnMainThread
	// ...
}
```

**Implication:** If you set state from a background thread, the handler update can occur on that thread. Most handlers are thread-safe for property updates, but some platform controls require main thread access. If you see "main thread assertion" crashes, wrap your state changes:

```csharp
ThreadHelper.RunOnMainThread(() => {
	myState.Value = newValue;
});
```

**`SetGlobalEnvironment` IS dispatched to main thread:**

```csharp
// View.cs
public static void SetGlobalEnvironment(string key, object value)
{
	Environment.SetValue(key, value, true);
	ThreadHelper.RunOnMainThread(() => {
		// ... fan out to all views ...
	});
}
```

**Component `SetState` IS dispatched to main thread:**

```csharp
// Component.cs
protected void SetState(Action<TState> mutator)
{
	// ... mutate state ...
	ThreadHelper.RunOnMainThread(() => Reload());
}
```

### 12.3 Lock-Free Reads During Build

Property reads during `Body` evaluation do NOT take any locks. `OnPropertyRead` just appends to a thread-local list:

```csharp
// StateManager.cs
public static void OnPropertyRead(object sender, string propertyName)
{
	if (!IsBuilding)
		return;
	var currentReadProperties = GetCurrentReadProperties();
	currentReadProperties.Add((sender as INotifyPropertyRead, propertyName));
}
```

This is safe because `_currentReadProperties` is thread-static. No other thread can see or modify it.

**Writes** (`OnPropertyChanged`) take a **read lock** to query `NotifyToViewMappings`, then release the lock before dispatching.

---

## 13. Appendix: Source Generation

Comet uses two source generators: `AutoNotifyGenerator` (for `[AutoNotify]` fields) and `CometViewSourceGenerator` (for generating View wrapper classes from MAUI interfaces).

### 13.1 AutoNotifyGenerator

Processes **fields** marked with `[AutoNotify]` and generates property wrappers that participate in the Comet state system. This allows plain classes (that don't inherit `BindingObject`) to work with Comet's dependency tracking.

**Example input:**

```csharp
public partial class TodoItem
{
	[AutoNotify]
	string _title;

	[AutoNotify]
	bool _isCompleted;
}
```

**Generated output:**

```csharp
public partial class TodoItem : INotifyPropertyRead, IAutoImplemented
{
	public event PropertyChangedEventHandler PropertyChanged;
	public event PropertyChangedEventHandler PropertyRead;

	public string Title
	{
		get
		{
			StateManager.OnPropertyRead(this, nameof(Title));
			this.PropertyRead?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
			return this._title;
		}
		set
		{
			this._title = value;
			StateManager.OnPropertyChanged(this, nameof(Title), value);
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
		}
	}
	// ... similar for IsCompleted
}
```

The generated class implements `INotifyPropertyRead` and `IAutoImplemented`. The `IAutoImplemented` marker is significant — `StateManager` skips event subscription for these types (since the property accessors call `StateManager` directly).

> **Important:** `[Body]` is handled entirely at **runtime**, NOT by any source generator. `View.CheckForBody()` uses reflection to find `[Body]`-annotated methods and `Delegate.CreateDelegate` to wrap them as `Func<View>`. See §3.3.

### 13.2 CometViewSourceGenerator

Generates View wrapper classes from MAUI interface assemblies. For example, given `IText` interface, it generates:

```csharp
[CometGenerate(typeof(IText))]
public partial class Text : View
{
	public Binding<string> Text { get; set; }
	public Binding<Microsoft.Maui.Graphics.Color> TextColor { get; set; }
	// ... more properties ...
}
```

These generated properties are then consumed by the handler system. The generator reads `[assembly: CometGenerate(...)]` attributes in `Controls/ControlsGenerator.cs` and emits partial classes.

**Not covered in depth here** because it's a build-time concern, not a runtime state management concern. See `src/Comet.SourceGenerator/` for implementation details.

---

## Summary

Comet's state management is a **hybrid dependency-tracking system** that:

1. **Captures reads** during Body evaluation (global properties) and Func evaluation (bound properties).
2. **Routes writes** to either targeted handler updates (bindings) or full view rebuilds (globals).
3. **Optimizes with stabilization**, skipping re-tracking once a binding's dependencies stop changing.
4. **Batches changes** to avoid redundant work when multiple properties change in quick succession.
5. **Integrates environment properties** as reactive inputs.
6. **Transfers state during hot reload** to preserve the developer experience.
7. **Cleans up with weak references, locks, and ArrayPool** to prevent leaks and minimize allocations.

The key mental model: **Comet knows what each view depends on because it watches what the view reads, then uses that information to decide how to react to writes.**

For contributors: the most critical code paths are `StateManager.OnPropertyRead`, `StateManager.OnPropertyChanged`, `BindingState.UpdateValue`, and `Binding<T>.EvaluateAndNotify`. Understanding these four methods unlocks the entire system.

For advanced users: the most common pitfall is **formatted string bindings without lambdas**. Always use `new Text(() => $"Hello {name.Value}")` instead of `new Text($"Hello {name.Value}")` to get targeted updates instead of full rebuilds.

---

**End of document.**
