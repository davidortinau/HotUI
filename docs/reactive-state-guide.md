# Comet Reactive State Guide

A practical guide to building reactive UIs with Comet's signal-based state system. Every concept includes a working code example.

---

## Table of Contents

1. [Quick Start](#1-quick-start)
2. [Core Concepts](#2-core-concepts)
3. [Binding Patterns](#3-binding-patterns)
4. [Computed Values](#4-computed-values)
5. [Side Effects](#5-side-effects)
6. [Lists and Collections](#6-lists-and-collections)
7. [Navigation State](#7-navigation-state)
8. [Component Pattern](#8-component-pattern)
9. [Hot Reload](#9-hot-reload)
10. [Best Practices](#10-best-practices)
11. [Migration from Old API](#11-migration-from-old-api)

---

## 1. Quick Start

Here's a complete Comet view with reactive state — a counter that updates when you tap a button:

```csharp
using Comet;
using Comet.Reactive;
using Microsoft.Maui.Graphics;
using static Comet.CometControls;

public class CounterView : View
{
    readonly Reactive<int> count = 0;

    [Body]
    View body() =>
        VStack(16,
            Text(() => $"Count: {count.Value}")
                .FontSize(48)
                .Color(Colors.DodgerBlue),
            Button("Increment", () => count.Value++)
        );
}
```

That's it. When `count.Value` changes, the `Text` control updates automatically. No `INotifyPropertyChanged`, no bindings to wire up, no view model.

### How to run it

Register your view in `MauiProgram.cs`:

```csharp
var builder = MauiApp.CreateBuilder();
builder.UseMauiApp<App>();
builder.UseCometHandlers();
```

Then in your `App`:

```csharp
public class App : View
{
    [Body]
    View body() => new CounterView();
}
```

---

## 2. Core Concepts

### Reactive\<T\> — Mutable State

`Reactive<T>` is the primary state primitive you'll use in Comet views. It holds a value and notifies the UI when that value changes.

```csharp
// Declare state as readonly fields with implicit conversion from T
readonly Reactive<int> count = 0;
readonly Reactive<string> name = "World";
readonly Reactive<bool> isOn = false;
readonly Reactive<double> progress = 0.5;
```

**Reading** — access `.Value` inside a lambda to create a reactive binding:

```csharp
Text(() => $"Hello, {name.Value}!")
```

**Writing** — assign to `.Value` to trigger a UI update:

```csharp
Button("Greet", () => name.Value = "Comet")
```

**Implicit conversion** — you can assign a plain value directly:

```csharp
readonly Reactive<int> count = 0;        // int → Reactive<int>
readonly Reactive<string> text = "hello"; // string → Reactive<string>
```

### Signal\<T\> — Lower-Level Primitive

`Signal<T>` lives in `Comet.Reactive` and is the internal foundation. `Reactive<T>` and `Signal<T>` have the same shape — both hold a value, track reads, and notify on writes. The difference:

| Feature | `Reactive<T>` | `Signal<T>` |
|---------|---------------|-------------|
| Namespace | `Comet` | `Comet.Reactive` |
| Sealed | No | Yes |
| Hot reload state transfer | Not automatic | Automatic (field-by-field) |
| Custom equality comparer | Default only | Constructor parameter |
| `DebugName` property | No | Yes |
| `Peek()` (read without tracking) | No | Yes |

**For app development, use `Reactive<T>`.** You'll only need `Signal<T>` if you need `Peek()`, custom equality, or debug diagnostics.

### Body Rebuilds

A method decorated with `[Body]` is the view's render function. It runs:

1. **On first display** — when the view appears on screen
2. **When a dependency changes** — when any `Reactive<T>` or `Signal<T>` read *directly in the body* has its value changed

```csharp
[Body]
View body()
{
    // Reading selectedIndex.Value HERE means: if selectedIndex changes,
    // the entire body() re-executes and the view tree is rebuilt.
    var idx = selectedIndex.Value;
    var page = pages[idx];

    return Grid(
        new object[] { 200, "*" },
        null,
        BuildSidebar().Cell(row: 0, column: 0),
        page.Cell(row: 0, column: 1)
    );
}
```

### Fine-Grained Updates (No Body Rebuild)

When you read `.Value` *inside a lambda* passed to a control, Comet tracks that read at the **control level**, not the body level. The control updates itself without re-executing body():

```csharp
[Body]
View body() =>
    VStack(
        // This Text updates when count changes — body() does NOT re-run
        Text(() => $"Count: {count.Value}"),

        // This Button's label is static — never updates
        Button("Add", () => count.Value++)
    );
```

This is a critical performance feature. Most of your UI should use lambda bindings for fine-grained updates. Reserve direct `.Value` reads in body() for when you need structural changes (swapping pages, showing/hiding sections).

---

## 3. Binding Patterns

### One-Way Display

Pass a `Func<T>` (a lambda) to a control's constructor. The lambda is re-evaluated whenever any `Reactive<T>` it reads changes:

```csharp
// Simple text display
Text(() => $"Count: {count.Value}")

// Conditional formatting
Text(() => count.Value >= 0 ? "Positive" : "Negative")
    .Color(count.Value >= 0 ? Colors.Green : Colors.Red)

// Computed display
Text(() => $"{firstName.Value} {lastName.Value}, age {(int)age.Value}")

// Progress bar driven by state
ProgressBar(() => Math.Min(1.0, count.Value / 20.0))
```

### Two-Way Binding (TextField)

Text fields need to both **read** the current value and **write** user input back:

```csharp
readonly Reactive<string> name = "";

// Read via lambda, write via OnTextChanged callback
TextField(() => name.Value, () => "Type here...")
    .OnTextChanged(v => name.Value = v ?? "")
```

Two `TextField` controls can bind to the same `Reactive<string>` — typing in either updates both:

```csharp
TextField(() => sharedText.Value, () => "Field A...")
    .OnTextChanged(v => sharedText.Value = v ?? ""),
TextField(() => sharedText.Value, () => "Field B...")
    .OnTextChanged(v => sharedText.Value = v ?? ""),
Text(() => $"Live: \"{sharedText.Value}\"")
```

### Slider with Callback

```csharp
readonly Reactive<double> sliderValue = 50;

Slider(() => sliderValue.Value, () => 0.0, () => 100.0)
    .OnValueChanged(v => sliderValue.Value = v),
Text(() => $"Value: {sliderValue.Value:F1}")
```

### Toggle with Callback

```csharp
readonly Reactive<bool> isOn = false;

HStack(12,
    Toggle(() => isOn.Value)
        .OnToggled(v => isOn.Value = v),
    Text(() => isOn.Value ? "ON ✅" : "OFF ❌")
)
```

### CheckBox

```csharp
readonly Reactive<bool> checked = false;

HStack(12,
    CheckBox(() => checked.Value)
        .OnCheckedChanged(v => checked.Value = v),
    Text(() => checked.Value ? "Checked ✓" : "Unchecked")
)
```

### Static Values (No Reactivity)

When a value never changes, pass it directly — no lambda needed:

```csharp
Text("This never changes")           // static string
Text("Username:").FontSize(12)        // static styling
Slider(() => 0.5, () => 0.0, () => 1.0) // static slider
```

### ⚠️ Common Mistake: Inline .Value vs Lambda .Value

This is the most common source of confusion. Where you read `.Value` determines **what gets rebuilt**:

```csharp
// ❌ BAD — reads .Value directly in body, causes full rebuild on every change
[Body]
View body() =>
    VStack(
        Text($"Count: {count.Value}")  // No lambda! This is evaluated once.
    );

// ✅ GOOD — reads .Value inside a lambda, only the Text control updates
[Body]
View body() =>
    VStack(
        Text(() => $"Count: {count.Value}")  // Lambda! Re-evaluated per change.
    );
```

**Rule of thumb:** If a control displays changing data, wrap the expression in `() =>`.

---

## 4. Computed Values

`Computed<T>` derives a value from one or more signals. It caches the result and only recalculates when a dependency changes.

```csharp
using Comet.Reactive;

public class ProfileView : View
{
    readonly Reactive<string> firstName = "Jane";
    readonly Reactive<string> lastName = "Doe";
    readonly Reactive<double> age = 30;

    readonly Computed<string> summary;
    readonly Computed<string> greeting;

    public ProfileView()
    {
        // Depends on firstName, lastName, and age
        summary = new Computed<string>(() =>
            $"{firstName.Value} {lastName.Value}, age {(int)age.Value}");

        // Depends on firstName and age — conditional logic works fine
        greeting = new Computed<string>(() =>
        {
            var name = firstName.Value;
            var ageVal = (int)age.Value;
            if (ageVal < 18) return $"Hey {name}! 🎮";
            if (ageVal < 65) return $"Hello, {name}. 👋";
            return $"Good day, {name}. 🎩";
        });
    }

    [Body]
    View body() =>
        VStack(16,
            TextField(() => firstName.Value, () => "First name...")
                .OnTextChanged(v => firstName.Value = v ?? ""),
            TextField(() => lastName.Value, () => "Last name...")
                .OnTextChanged(v => lastName.Value = v ?? ""),
            Slider(() => age.Value, () => 0.0, () => 100.0)
                .OnValueChanged(v => age.Value = v),
            Text(() => summary.Value).FontSize(18),
            Text(() => greeting.Value).Color(Colors.MediumSeaGreen)
        );
}
```

### When to Use Computed vs a Lambda in Text()

| Approach | Use when |
|----------|----------|
| `Text(() => expr)` | The expression is simple and only used once |
| `Computed<T>` | The derived value is used by multiple controls, or the calculation is expensive |

```csharp
// Fine — simple, used once
Text(() => $"{firstName.Value} {lastName.Value}")

// Better — reused in multiple places
readonly Computed<string> fullName;
// in constructor: fullName = new Computed<string>(() => $"{firstName.Value} {lastName.Value}");
Text(() => fullName.Value)  // in body
Text(() => $"Welcome, {fullName.Value}!")  // elsewhere
```

`Computed<T>` also implements `IDisposable` — dispose it when you're done to unsubscribe from dependencies.

---

## 5. Side Effects

`Effect` runs an action whenever its tracked dependencies change. Use it for logging, API calls, analytics, or any non-UI reaction to state changes.

```csharp
using Comet.Reactive;

public class SearchView : View
{
    readonly Reactive<string> query = "";
    readonly Reactive<string> results = "Type to search...";
    Effect? _searchEffect;

    public SearchView()
    {
        // Runs immediately, then re-runs whenever query.Value changes
        _searchEffect = new Effect(() =>
        {
            var q = query.Value;  // tracked dependency
            if (string.IsNullOrWhiteSpace(q))
                results.Value = "Type to search...";
            else
                results.Value = $"Searching for: {q}";
        });
    }

    [Body]
    View body() =>
        VStack(16,
            TextField(() => query.Value, () => "Search...")
                .OnTextChanged(v => query.Value = v ?? ""),
            Text(() => results.Value)
        );
}
```

### Effect Lifecycle

- By default, `Effect` runs immediately on construction (`runImmediately: true`).
- Pass `runImmediately: false` to defer the first execution.
- Effects are batched by the `ReactiveScheduler` — rapid signal changes coalesce into a single Effect re-run.
- Dispose the `Effect` to stop it: `_searchEffect?.Dispose();`

### ⚠️ Avoid Cycles

An Effect that writes to a signal consumed by another Effect (which writes to a signal consumed by the first) creates a cycle. The scheduler detects this and breaks the loop after 100 iterations, but your UI may show stale data. In debug builds, it throws an `InvalidOperationException`.

---

## 6. Lists and Collections

`SignalList<T>` is a reactive list that triggers UI updates when items are added, removed, or replaced.

```csharp
using Comet.Reactive;

public class TodoView : View
{
    readonly SignalList<string> items = new();
    readonly Reactive<string> newItem = "";
    int _nextId;

    [Body]
    View body()
    {
        // Build item views from the list
        var itemViews = new View[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            var index = i;
            var text = items[index];
            itemViews[i] = HStack(8,
                Text($"• {text}").FontSize(14),
                new Spacer(),
                Button("✕", () =>
                {
                    if (index < items.Count)
                        items.RemoveAt(index);
                })
                .Color(Colors.Crimson)
            );
        }

        return VStack(16,
            HStack(8,
                TextField(() => newItem.Value, () => "New item...")
                    .OnTextChanged(v => newItem.Value = v ?? ""),
                Button("Add", () =>
                {
                    var text = string.IsNullOrWhiteSpace(newItem.Value)
                        ? $"Item {++_nextId}"
                        : newItem.Value;
                    items.Add(text);
                    newItem.Value = "";
                })
            ),
            items.Count == 0
                ? (View)Text("No items yet").Color(Colors.Grey)
                : VStack(4, itemViews),
            Button("Clear All", () => items.Clear())
                .Color(Colors.Crimson)
        );
    }
}
```

### SignalList API

| Method | Description |
|--------|-------------|
| `Add(T item)` | Appends an item |
| `Insert(int index, T item)` | Inserts at position |
| `Remove(T item)` | Removes first match, returns `bool` |
| `RemoveAt(int index)` | Removes at position |
| `Clear()` | Removes all items |
| `Batch(Action<List<T>>)` | Mutate the inner list directly, then triggers a single Reset notification |
| `this[int index]` | Get or set by index (setter triggers Replace notification) |
| `Count` | Reactive — reads are tracked |

### Batch Mutations

For adding many items at once, use `Batch` to avoid per-item notifications:

```csharp
items.Batch(list =>
{
    list.AddRange(newData);
    list.Sort();
});
// Single UI update after the batch completes
```

> **Note:** `SignalList<T>` reads (Count, indexer, enumeration) are automatically tracked by `ReactiveScope`. Any mutation triggers a body rebuild for views that read the list.

---

## 7. Navigation State

A common pattern is using a `Reactive<int>` to drive page selection, where you **want** a full body rebuild to swap the entire content area:

```csharp
public class SidebarLayout : View
{
    readonly Reactive<int> selectedIndex = 0;

    static readonly List<(string Title, Func<View> Create)> pages = new()
    {
        ("Home",     () => new HomePage()),
        ("Settings", () => new SettingsPage()),
        ("Profile",  () => new ProfilePage()),
    };

    [Body]
    View body()
    {
        // Reading selectedIndex.Value HERE causes body rebuild on change
        var idx = selectedIndex.Value;
        var detail = pages[idx].Create();

        return Grid(
            new object[] { 200, "*" },
            null,
            BuildSidebar().Cell(row: 0, column: 0),
            NavigationView(detail)
                .Title(pages[idx].Title)
                .Cell(row: 0, column: 1)
        );
    }

    View BuildSidebar()
    {
        var items = new List<View>();
        for (int i = 0; i < pages.Count; i++)
        {
            var index = i;
            var isSelected = selectedIndex.Value == index;
            items.Add(
                Text(pages[i].Title)
                    .FontSize(14)
                    .Color(isSelected ? Colors.Blue : Colors.Black)
                    .Padding(new Thickness(16, 10))
                    .Background(isSelected ? Colors.Blue.WithAlpha(0.1f) : Colors.Transparent)
                    .OnTap((v) => selectedIndex.Value = index)
            );
        }
        return ScrollView(VStack(0, items.ToArray()));
    }
}
```

### When You Want Body Rebuild vs When You Don't

| Scenario | Read `.Value` in | Result |
|----------|-------------------|--------|
| **Swap pages** (navigation) | body() directly | Full rebuild — new page view created |
| **Update a label** | Lambda `() => x.Value` | Fine-grained — only that control updates |
| **Conditional show/hide** | body() directly | Full rebuild — structural change |
| **Animate a value** | Lambda | Fine-grained — no flicker |

---

## 8. Component Pattern

`Component<TState>` offers a React-like pattern with `SetState()` for batched mutations.

```csharp
public class CounterState
{
    public int Count { get; set; }
    public string Message { get; set; } = "Ready";
}

public class CounterComponent : Component<CounterState>
{
    public override View Render()
    {
        return VStack(16,
            Text($"Count: {State.Count}")
                .FontSize(32),
            Text(State.Message)
                .Color(Colors.Grey),
            Button("Increment", () => SetState(s =>
            {
                s.Count++;
                s.Message = $"Incremented to {s.Count}";
            }))
        );
    }
}
```

### How Component Differs from View/[Body]

| Feature | `View` + `[Body]` | `Component<TState>` |
|---------|--------------------|-----------------------|
| State declaration | `Reactive<T>` fields | Plain C# class with properties |
| Triggering updates | Automatic (write to `.Value`) | Explicit (`SetState(...)`) |
| Render method | `[Body] View body()` | `override View Render()` |
| Granularity | Fine-grained per-control | Full re-render on `SetState` |
| Lifecycle hooks | `OnLoaded` | `OnMounted`, `OnWillUnmount` |

### Component with Props

`Component<TState, TProps>` accepts parent-supplied data:

```csharp
public class CardProps
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
}

public class CardState
{
    public bool IsExpanded { get; set; }
}

public class ExpandableCard : Component<CardState, CardProps>
{
    public override View Render()
    {
        return VStack(8,
            Text(Props.Title).FontSize(18).FontWeight(FontWeight.Bold),
            State.IsExpanded
                ? Text(Props.Subtitle).Color(Colors.Grey)
                : null,
            Button(State.IsExpanded ? "Collapse" : "Expand",
                () => SetState(s => s.IsExpanded = !s.IsExpanded))
        );
    }
}

// Usage:
new ExpandableCard { Props = new CardProps { Title = "Details", Subtitle = "More info here..." } }
```

---

## 9. Hot Reload

Comet supports .NET Hot Reload — edit code, save, and see changes without restarting the app.

### How State Survives Hot Reload

When hot reload replaces a view type, Comet's `TransferHotReloadStateTo` method copies `Signal<T>` fields from the old instance to the new one by matching field names and types via reflection. This means:

- **`Signal<T>` fields keep their values** across hot reload edits
- The field name and type must match between old and new code
- Adding or removing signal fields is fine — new fields get their initial value, removed fields are dropped

### What to Watch Out For

1. **`Reactive<T>` fields are NOT automatically transferred** — only `Signal<T>` is. If you're using `Reactive<T>` (recommended), hot reload will reset those fields to their initial values. The view rebuilds correctly, but accumulated state is lost.

2. **Component state** — `Component<TState>` has its own `TransferStateFrom` that copies the `TState` object by reference, so component state survives hot reload.

3. **Structural changes** — renaming a field or changing its type resets that field. The rest of the view's state is preserved.

4. **Constructor logic** — constructors re-run on hot reload. If you initialize `Computed<T>` or `Effect` in the constructor, they'll be re-created (which is usually fine since they re-discover dependencies automatically).

---

## 10. Best Practices

### Declare Signals as `readonly` Fields

```csharp
// ✅ Good — readonly prevents accidental reassignment
readonly Reactive<int> count = 0;

// ❌ Bad — could accidentally write: count = new Reactive<int>(5)
Reactive<int> count = 0;
```

### Use Func Constructors for Display, Callbacks for Writes

```csharp
// Display: lambda (Func constructor)
Text(() => $"Value: {x.Value}")

// Write: callback
Button("Go", () => x.Value++)
.OnTextChanged(v => x.Value = v)
.OnValueChanged(v => x.Value = v)
```

### Keep body() Fast

The body method runs on the UI thread. Avoid heavy computation:

```csharp
// ❌ Don't do expensive work in body
[Body]
View body()
{
    var data = LoadFromDatabase(); // blocks UI thread!
    return Text(data);
}

// ✅ Load async, store in state
readonly Reactive<string> data = "Loading...";

public MyView()
{
    Task.Run(async () =>
    {
        var result = await LoadFromDatabase();
        data.Value = result; // triggers UI update from background
    });
}

[Body]
View body() => Text(() => data.Value);
```

### Coalescing: Rapid Writes Are Batched

Multiple signal writes within a single synchronous handler result in **one** UI update:

```csharp
Button("Add 100", () =>
{
    // All 100 writes happen synchronously
    // Only ONE body rebuild occurs (after this handler returns)
    for (int i = 0; i < 100; i++)
        count.Value++;
})
```

The `ReactiveScheduler` posts a single flush to the UI thread's dispatcher. Even from background threads, writes are coalesced.

### Capture Loop Variables

When building views in a loop, capture the loop variable:

```csharp
for (int i = 0; i < items.Count; i++)
{
    var index = i; // ✅ Capture — otherwise all closures see the final value of i
    Button($"Item {index}", () => selectedIndex.Value = index);
}
```

### Don't Read .Value Inline for Rapidly-Changing Properties

For controls like `Slider` or `TextField` that fire many updates per second, always use the lambda pattern:

```csharp
// ✅ Fine-grained: only the Text updates on each slider move
Slider(() => val.Value, () => 0.0, () => 100.0)
    .OnValueChanged(v => val.Value = v),
Text(() => $"Value: {val.Value:F1}")

// ❌ Reading val.Value directly in body — rebuilds entire view tree on every drag
var current = val.Value;
Text($"Value: {current:F1}")
```

---

## 11. Migration from Old API

If you're migrating from an earlier version of Comet that used `State<T>` and `Binding<T>`:

| Old API | New API | Notes |
|---------|---------|-------|
| `State<T>` | `Reactive<T>` | Same usage: `readonly Reactive<int> x = 0;` |
| `Binding<T>` | Lambdas / `PropertySubscription<T>` | You don't interact with `PropertySubscription` directly — it's internal infrastructure. Use `() => x.Value` lambdas. |
| `StateManager` | Gone | No replacement needed — `ReactiveScope` handles tracking automatically. |
| `@"x.Value"` binding strings | `() => x.Value` | Lambda expressions replaced string-based bindings. |

### Before (old API):

```csharp
readonly State<int> count = 0;

[Body]
View body() => new VStack {
    new Text(() => $"Count: {count.Value}"),
    new Button("Add", () => count.Value++)
};
```

### After (new API):

```csharp
readonly Reactive<int> count = 0;

[Body]
View body() =>
    VStack(16,
        Text(() => $"Count: {count.Value}"),
        Button("Add", () => count.Value++)
    );
```

The code is nearly identical. The main changes:

1. `State<T>` → `Reactive<T>`
2. `new VStack { ... }` → `VStack(spacing, ...)` (static factory methods from `CometControls`)
3. `new Text(...)` → `Text(...)` (static factory methods via `using static Comet.CometControls`)

---

## Quick Reference

```csharp
using Comet;
using Comet.Reactive;
using static Comet.CometControls;

// State primitives
readonly Reactive<int> count = 0;              // mutable state
readonly Signal<int> precise = new(0);         // lower-level, with Peek() and DebugName
readonly SignalList<string> items = new();      // reactive collection

// Derived state
readonly Computed<string> label;               // auto-recalculates
// In constructor:
// label = new Computed<string>(() => $"Count is {count.Value}");

// Side effects
// In constructor:
// var effect = new Effect(() => Console.WriteLine($"Count changed: {count.Value}"));

// Read (one-way display)
Text(() => $"Count: {count.Value}")

// Write
Button("Add", () => count.Value++)

// Two-way
TextField(() => name.Value, () => "Placeholder...")
    .OnTextChanged(v => name.Value = v ?? "")

// Static (no reactivity)
Text("Hello, World!")
```
