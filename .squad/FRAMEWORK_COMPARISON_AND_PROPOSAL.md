# The Best of Both Worlds: Comet + MauiReactor Technical Comparison & Proposal

## Executive Summary

This document proposes a new framework — working title **"Orbit"** — that merges the best architectural decisions from both **Comet** and **MauiReactor** into a single, optimal .NET MAUI MVU framework. The core thesis:

> **Use Comet's low-level MAUI integration (IView, handlers, no BindableObject) as the foundation, but adopt MauiReactor's developer-facing API (method-based DSL, Component model, typed state/props, theme system) as the surface.**

This is fundamentally **bringing MauiReactor's API design to Comet's architecture**, not the reverse. Comet has the better engine; MauiReactor has the better steering wheel.

---

## Table of Contents

1. [Framework Architecture Comparison](#1-framework-architecture-comparison)
2. [Side-by-Side Code Comparison](#2-side-by-side-code-comparison)
3. [What Each Framework Does Better](#3-what-each-framework-does-better)
4. [The Proposal: Converged Architecture](#4-the-proposal-converged-architecture)
5. [Technical Requirements](#5-technical-requirements)
6. [Control System Design](#6-control-system-design)
7. [State Management Design](#7-state-management-design)
8. [Rendering & Reconciliation Design](#8-rendering--reconciliation-design)
9. [Styling & Theming Design](#9-styling--theming-design)
10. [3rd-Party Control Interop](#10-3rd-party-control-interop)
11. [Navigation Design](#11-navigation-design)
12. [Hot Reload Design](#12-hot-reload-design)
13. [Migration Path](#13-migration-path)
14. [Risk Analysis](#14-risk-analysis)

---

## 1. Framework Architecture Comparison

### Architectural Layers

```
┌─────────────────────────────────────────────────────────────────────┐
│                     DEVELOPER API SURFACE                           │
│  MauiReactor: Method DSL          Comet: Class constructors         │
│  Button("text")                   new Button("text")                │
│  .OnClicked(handler)              .OnTap(_ => handler())            │
│  .FontSize(18)                    .FontSize(18)                     │
├─────────────────────────────────────────────────────────────────────┤
│                     COMPONENT MODEL                                 │
│  MauiReactor: Component<S,P>      Comet: View with Body             │
│  Render() → VisualNode            Body() → View                     │
│  SetState(s => s.X++)             state.Value = newVal              │
│  Explicit invalidation            Automatic dep tracking            │
├─────────────────────────────────────────────────────────────────────┤
│                     VIRTUAL TREE / RECONCILIATION                   │
│  MauiReactor: VisualNode tree     Comet: View tree                  │
│  Index-based diff                 Type-based diff with lookahead    │
│  MergeWith() reuses native ctrl   Diff() reuses handlers            │
├─────────────────────────────────────────────────────────────────────┤
│                     NATIVE CONTROL BRIDGE                           │
│  MauiReactor: VisualNode<T>       Comet: ViewHandler<View,Platform> │
│  T = M.M.Controls.Button          Implements IView directly         │
│  Creates BindableObject instances  No BindableObject at all          │
│  Uses SetValue(BindableProperty)  Uses Handler property mappers     │
├─────────────────────────────────────────────────────────────────────┤
│                     MAUI PLATFORM LAYER                             │
│  MauiReactor: Microsoft.Maui.     Comet: Microsoft.Maui.            │
│  Controls (BindableObject tree)   Handlers + IView interfaces       │
│  Full MVVM control instances      Direct to platform handlers       │
└─────────────────────────────────────────────────────────────────────┘
```

### The Critical Difference: Which MAUI Layer?

**MauiReactor** targets `Microsoft.Maui.Controls` — the **MVVM layer**:
```
VisualNode<T> where T : BindableObject
  → OnMount(): _nativeControl = new Microsoft.Maui.Controls.Button()
  → OnUpdate(): NativeControl.SetValue(Button.TextProperty, "Hello")
```
Every MauiReactor control creates a full `BindableObject` instance with its entire MVVM binding infrastructure, property change notifications, triggers, behaviors, and style resolution — even though MauiReactor never uses any of it.

**Comet** targets `Microsoft.Maui` — the **handler/interface layer**:
```
View : IView, ITextButton, ILabel (implements interfaces directly)
  → Handler receives IView
  → Handler maps properties to platform views
  → No BindableObject, no BindableProperty, no binding infrastructure
```
Comet's View IS the IView. There is no intermediary. The handler receives Comet's View directly and reads properties from the environment dictionary.

### What This Means for Performance

| Aspect | MauiReactor | Comet |
|--------|-------------|-------|
| **Object creation per control** | 1 VisualNode + 1 BindableObject (MAUI Control) | 1 View (implements IView) |
| **Property storage** | BindableProperty system (type checking, coercion, callbacks) | Simple dictionary lookup |
| **Property notification overhead** | Full INotifyPropertyChanged chain | Direct handler.UpdateValue() |
| **Memory per control** | VisualNode fields + BindableObject fields + BindableProperty storage | View fields + environment dictionary |
| **Unused MVVM infrastructure** | Styles, Triggers, Behaviors, Visual State Manager all allocated | None — doesn't exist |
| **Event system** | MAUI's event infrastructure on BindableObject | Direct delegate/action |

**Estimated overhead per control**: MauiReactor creates ~2x the objects with ~3-5x the property infrastructure that goes completely unused in an MVU context.

---

## 2. Side-by-Side Code Comparison

### Simple Counter Component

**MauiReactor:**
```csharp
class CounterState { public int Count { get; set; } }

class CounterPage : Component<CounterState>
{
    public override VisualNode Render() =>
        ContentPage(
            VStack(
                Label($"Count: {State.Count}")
                    .FontSize(24)
                    .HCenter(),
                Button("Increment", () => SetState(s => s.Count++))
                    .HCenter()
            )
            .VCenter()
            .Spacing(16)
        );
}
```

**Comet:**
```csharp
class CounterPage : View
{
    readonly State<int> count = 0;

    [Body]
    View body() =>
        new VStack(spacing: 16)
        {
            new Text(() => $"Count: {count.Value}")
                .FontSize(24)
                .HorizontalTextAlignment(TextAlignment.Center),
            new Button("Increment", () => count.Value++)
        }
        .HorizontalLayoutAlignment(LayoutAlignment.Center);
}
```

### Form with Validation

**MauiReactor:**
```csharp
class FormState
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsValid => !string.IsNullOrEmpty(Name) && Email.Contains('@');
}

class FormPage : Component<FormState>
{
    public override VisualNode Render() =>
        ContentPage(
            VStack(
                Entry()
                    .Placeholder("Name")
                    .Text(State.Name)
                    .OnTextChanged(t => SetState(s => s.Name = t)),
                Entry()
                    .Placeholder("Email")
                    .Keyboard(Keyboard.Email)
                    .Text(State.Email)
                    .OnTextChanged(t => SetState(s => s.Email = t)),
                Button("Submit")
                    .IsEnabled(State.IsValid)
                    .OnClicked(Submit)
            )
            .Spacing(12)
            .Padding(20)
        );

    async void Submit() { /* ... */ }
}
```

**Comet:**
```csharp
class FormPage : View
{
    readonly State<string> name = "";
    readonly State<string> email = "";

    [Body]
    View body() =>
        new VStack(spacing: 12)
        {
            new TextField(name, "Name"),
            new TextField(email, "Email"),
            new Button("Submit", Submit)
                .Enabled(!string.IsNullOrEmpty(name.Value) && email.Value.Contains('@'))
        }
        .Padding(20);

    async void Submit() { /* ... */ }
}
```

### Key API Differences

| Pattern | MauiReactor | Comet |
|---------|-------------|-------|
| **Create control** | `Button()` (method) | `new Button()` (constructor) |
| **Set text** | `.Text("hello")` | constructor param or `.Text("hello")` |
| **Event handler** | `.OnClicked(() => ...)` | constructor param `Action` or `.OnTap()` |
| **State declaration** | `class MyState { ... }` + `Component<MyState>` | `readonly State<T> field = default;` |
| **State mutation** | `SetState(s => s.Prop = val)` | `field.Value = val` |
| **Computed values** | Inline in Render: `$"{State.Count}"` | Lambda: `() => $"{count.Value}"` |
| **Child nesting** | Method params: `VStack(child1, child2)` | Collection init: `new VStack { child1, child2 }` |
| **Centering** | `.HCenter()` | `.HorizontalLayoutAlignment(LayoutAlignment.Center)` |
| **Component w/ props** | `ComponentWithProps<P>`, `Props.X` | Constructor params on View subclass |
| **Lifecycle** | `OnMounted()`, `OnWillUnmount()` | `OnLoaded()`, `OnUnloaded()` |
| **Navigation** | `Shell.GoToAsync<T>()` | No built-in typed navigation |
| **Theme** | `ButtonStyles.Default = ...` + `.ThemeKey("primary")` | Environment cascading |

---

## 3. What Each Framework Does Better

### MauiReactor Advantages ✅

1. **Method-based DSL** — `Button("text")` is cleaner than `new Button("text")`. No `new` keyword noise. Controls feel like function calls, which aligns with the functional MVU mental model.

2. **Typed state classes** — `Component<CounterState>` with `SetState(s => s.Count++)` is more structured than individual `State<T>` fields. State is a cohesive object, not scattered fields.

3. **Explicit state mutation** — `SetState()` makes state transitions visible and batched. You know exactly when and why the component re-renders.

4. **Props system** — `ComponentWithProps<P>` provides clean parent→child data flow with typed prop objects. Comet uses constructor parameters which don't support hot-reload state preservation.

5. **Theme system** — `ButtonStyles.Default`, `ButtonStyles.Themes["primary"]`, `.ThemeKey()` is elegant. Per-control-type defaults plus named variants. Comet's environment cascading is powerful but less ergonomic.

6. **Generated factory methods** — `Component.partial.cs` generates `Button()`, `Label()`, etc. as instance methods on Component, eliminating `new` and enabling clean IntelliSense.

7. **Key-aware reconciliation** — `VisualNode.Key` property exists (though index-based by default). Comet has no key concept.

8. **Shell navigation** — `Shell.GoToAsync<T>(props => props.Id = 42)` with typed props is excellent. Comet has basic NavigationView with push/pop.

9. **Comprehensive control coverage** — 148 scaffolded types from WidgetList.txt. Every MAUI control wrapped. Comet generates ~20 from interfaces.

10. **Animation system** — Built-in `RxAnimation` on properties with easing, interpolation. Comet has basic MAUI animations.

### Comet Advantages ✅

1. **Direct IView implementation** — View IS IView. No intermediary BindableObject. Zero MVVM overhead. This is architecturally superior for MVU.

2. **Handler-level integration** — Speaks directly to MAUI's handler layer. No unnecessary Controls layer in between. Leaner, faster, fewer allocations.

3. **Automatic dependency tracking** — `INotifyPropertyRead` captures which state was read during Body evaluation. No manual `SetState()` needed for simple cases. Truly reactive.

4. **Binding<T> reactive system** — `Binding<T>` wraps `Func<T>` and automatically re-evaluates when dependencies change. Can update individual properties without full re-render.

5. **Granular property updates** — StateManager can update specific View properties (via `ViewUpdateProperties`) without rebuilding the entire subtree. MauiReactor always re-renders the full component.

6. **Environment cascading** — Three-level lookup (local → context → global) with typed key constants. Properties naturally flow down the tree. More powerful than MauiReactor's style system for dynamic theming.

7. **No BindableObject allocation** — Each MauiReactor control creates a full `Microsoft.Maui.Controls.*` instance. Comet creates only a View object with a dictionary.

8. **Source generator from interfaces** — Generates from `ITextButton`, `ILabel`, etc. (MAUI handler interfaces), not from `Microsoft.Maui.Controls.Button` (MVVM classes). This targets the right abstraction level.

9. **Hot reload with state transfer** — `IHotReloadableView.TransferState()` with smart diff. MauiReactor uses type-name matching to copy state, which is simpler but less precise.

10. **View tree is the interface tree** — A `Comet.Button` is simultaneously the view node AND the `ITextButton` that the handler reads from. In MauiReactor, there's always an extra hop through the native control.

---

## 4. The Proposal: Converged Architecture

### Direction: MauiReactor API → Comet Engine

The proposal is to build a framework that:
- Uses **Comet's handler-level architecture** (IView implementation, no BindableObject)
- Uses **MauiReactor's API surface** (method DSL, Component<S,P>, Render(), factory methods)
- Uses **MauiReactor's state model** (typed state classes, SetState, Props)
- Uses **MauiReactor's theme system** (ControlStyles.Default, .ThemeKey())
- Uses **Comet's environment cascading** for property propagation internally
- Uses **Comet's granular update system** where possible (single-property updates)
- Uses **MauiReactor's source generator** (adapted to generate from IView interfaces instead of Controls)
- Provides **MauiReactor-compatible 3rd party interop** (wrapping arbitrary MAUI controls)

### Why This Direction (Not the Reverse)

Bringing Comet concepts to MauiReactor would mean:
- Rewriting MauiReactor's VisualNode<T> to not create BindableObject instances — this breaks the entire scaffolding system
- Rewriting 148 generated wrappers to target IView interfaces instead of Controls
- Fundamentally changing the property storage from `SetValue(BindableProperty)` to a dictionary/environment system
- This is essentially rewriting MauiReactor from scratch

Bringing MauiReactor concepts to Comet means:
- Adding a Component<S,P> base class alongside existing View — additive, not destructive
- Adding factory methods via source generator — additive
- Adding a theme system on top of existing environment — additive  
- Adapting the source generator to produce more controls — expanding existing capability
- The handler architecture stays the same

**Comet is the better foundation to build on.**

### Target Developer Experience

```csharp
// The dream: MauiReactor syntax + Comet performance

class CoffeeListState
{
    public List<Coffee> Items { get; set; } = [];
    public bool IsLoading { get; set; }
    public string SearchText { get; set; } = "";
}

class CoffeeListPage : Component<CoffeeListState>
{
    public override View Render() =>
        ContentPage(
            VStack(
                SearchBar()
                    .Text(State.SearchText)
                    .OnTextChanged(t => SetState(s => s.SearchText = t)),

                State.IsLoading
                    ? ActivityIndicator().IsRunning(true).HCenter()
                    : CollectionView(
                        State.Items
                            .Where(c => c.Name.Contains(State.SearchText))
                            .Select(c => CoffeeCard(c))
                      )
            )
            .Spacing(8)
        );

    View CoffeeCard(Coffee coffee) =>
        Border(
            HStack(
                Image(coffee.ImageUrl)
                    .Frame(60, 60)
                    .CornerRadius(8),
                VStack(
                    Label(coffee.Name).FontSize(16).Bold(),
                    Label(coffee.Origin).FontSize(12).TextColor(Colors.Gray)
                )
                .Spacing(4)
            )
            .Padding(12)
            .Spacing(12)
        )
        .CornerRadius(12)
        .Background(Colors.White)
        .Shadow(2, Colors.Black.WithAlpha(0.1f))
        .OnTapped(() => Shell.GoToAsync<CoffeeDetailPage>(
            p => p.CoffeeId = coffee.Id));

    protected override async void OnMounted()
    {
        SetState(s => s.IsLoading = true);
        var items = await Services.GetService<ICoffeeService>()!.GetAllAsync();
        SetState(s => { s.Items = items; s.IsLoading = false; });
    }
}
```

**Key observations about the target syntax:**
- `Render()` returns `View` (Comet's), not `VisualNode` (MauiReactor's)
- `Label("text")`, `Button("text")`, `Border(content)` — method DSL, no `new`
- `State.SearchText`, `SetState(s => s.X = y)` — MauiReactor state pattern
- `.Bold()`, `.Shadow()`, `.OnTapped()` — fluent extensions
- `Shell.GoToAsync<T>(props)` — typed navigation
- `Services.GetService<T>()` — DI access
- Under the hood: no BindableObject. View implements IView directly.

---

## 5. Technical Requirements

### R1: Core View Base Class

**Requirement**: Create a `View` base class that implements `IView` directly (Comet pattern) but supports the Component render model (MauiReactor pattern).

```csharp
// The converged View hierarchy
public abstract class View : IView, IDisposable, ISafeAreaView, IGestureView
{
    // Environment-based property storage (from Comet)
    internal EnvironmentData Context { get; }
    internal EnvironmentData LocalContext { get; }
    
    // Render tree (from MauiReactor)
    internal IReadOnlyList<View> Children { get; }
    internal View? Parent { get; set; }
    
    // Handler bridge (from Comet)
    internal IElementHandler? ViewHandler { get; set; }
    
    // IView implementation reads from environment
    Size IView.DesiredSize => GetEnvironment<Size>(Keys.DesiredSize);
    double IView.Width => GetEnvironment<double>(Keys.Width, -1);
    // ... etc
}

public abstract class Component : View
{
    public abstract View Render();
    
    protected override IEnumerable<View> RenderChildren()
        => new[] { Render() };
}

public abstract class Component<S> : Component where S : class, new()
{
    public S State { get; private set; }
    
    protected void SetState(Action<S> action)
    {
        action(State);
        Invalidate();
    }
}

public abstract class Component<S, P> : Component<S> 
    where S : class, new() 
    where P : class, new()
{
    public P Props { get; internal set; }
}
```

### R2: Method-Based Control DSL

**Requirement**: Controls are created via static methods, not constructors. Source generator produces both the View implementation and the factory methods.

```csharp
// Generated: View class implementing ITextButton directly
public partial class Button : View, ITextButton
{
    // ITextButton implementation reads from environment
    string ITextButton.Text => GetEnvironment<string>(Keys.Button.Text) ?? "";
    Font IText.Font => ResolveFont();
    
    // Fluent setters store in environment
    // (generated as extension methods)
}

// Generated: Factory methods on Component base
public partial class Component
{
    public static Button Button() => new();
    public static Button Button(string text) => new Button().Text(text);
    public static Button Button(string text, Action clicked) => new Button().Text(text).OnClicked(clicked);
    
    public static Label Label() => new();
    public static Label Label(string text) => new Label().Text(text);
    
    public static VStack VStack(params View[] children) => new VStack(children);
    public static HStack HStack(params View[] children) => new HStack(children);
    
    public static Grid Grid(string rows, string columns, params View[] children) 
        => new Grid(rows, columns, children);
}
```

### R3: Source Generator Targeting IView Interfaces

**Requirement**: Adapt Comet's source generator approach (generating from `ITextButton`, `ILabel`, etc.) but produce MauiReactor-style output (factory methods, fluent extensions, style classes).

**Input** (attribute):
```csharp
[assembly: Generate(typeof(ITextButton), 
    ClassName = "Button",
    ConstructorParams = new[] { "Text", "Clicked" },
    Extensions = true,
    Styles = true)]
```

**Output** (generated):
```csharp
// 1. View implementation
public partial class Button : View, ITextButton, IButton
{
    string ITextButton.Text => GetEnvironment<string>(Keys.Text) ?? "";
    Color ITextStyle.TextColor => GetEnvironment<Color>(Keys.TextColor);
    Font IText.Font => BuildFont();
    Action? IButton.Clicked => GetEnvironment<Action>(Keys.Clicked);
    // ... all interface members from environment
}

// 2. Extension methods  
public static class ButtonExtensions
{
    public static T Text<T>(this T view, string text) where T : Button
        => view.SetEnvironment(Keys.Text, text);
    public static T OnClicked<T>(this T view, Action handler) where T : Button
        => view.SetEnvironment(Keys.Clicked, handler);
    public static T FontSize<T>(this T view, double size) where T : Button
        => view.SetEnvironment(Keys.FontSize, size);
    public static T Bold<T>(this T view) where T : Button
        => view.SetEnvironment(Keys.FontWeight, FontWeight.Bold);
    // ... all settable properties
}

// 3. Style support
public static class ButtonStyles
{
    public static Action<Button>? Default { get; set; }
    public static Dictionary<string, Action<Button>> Themes { get; } = new();
}
```

### R4: Handler Registration

**Requirement**: Register MAUI handlers for the new View types, mapping them to the same platform handlers MAUI uses internally.

```csharp
public static MauiAppBuilder UseOrbit<TApp>(this MauiAppBuilder builder)
    where TApp : Component, new()
{
    builder.ConfigureMauiHandlers(handlers =>
    {
        // Orbit's Button (ITextButton) → MAUI's ButtonHandler
        handlers.AddHandler<Button, ButtonHandler>();
        handlers.AddHandler<Label, LabelHandler>();
        handlers.AddHandler<TextField, EntryHandler>();
        handlers.AddHandler<Toggle, SwitchHandler>();
        // ... all controls
        
        // Composite view handler for Component
        handlers.AddHandler<View, OrbitViewHandler>();
    });
    
    return builder;
}
```

### R5: Typed State and Props

**Requirement**: MauiReactor-style typed state classes with SetState pattern.

```csharp
// State is a plain class — no base class needed
class CounterState
{
    public int Count { get; set; }
    public string Label { get; set; } = "Click me";
}

// Props for parent→child communication
class CoffeeCardProps
{
    public Coffee Coffee { get; set; }
    public Action<Coffee> OnSelected { get; set; }
}

class CoffeeCard : Component<EmptyState, CoffeeCardProps>
{
    public override View Render() =>
        Border(
            Label(Props.Coffee.Name)
        )
        .OnTapped(() => Props.OnSelected?.Invoke(Props.Coffee));
}

// Usage from parent:
new CoffeeCard { Props = { Coffee = item, OnSelected = HandleSelect } }
// Or with factory:
CoffeeCard(props => { props.Coffee = item; props.OnSelected = HandleSelect; })
```

### R6: Theme System

**Requirement**: MauiReactor's theme system built on Comet's environment cascading.

```csharp
public abstract class Theme
{
    public static AppTheme CurrentAppTheme { get; set; }
    public static bool IsDarkTheme => CurrentAppTheme == AppTheme.Dark;
    
    protected abstract void OnApply();
}

class AppTheme : Theme
{
    // Theme-aware colors
    public static Color Primary => IsDarkTheme ? Color.FromHex("#BB86FC") : Color.FromHex("#6200EE");
    public static Color Surface => IsDarkTheme ? Color.FromHex("#121212") : Colors.White;
    public static Color OnSurface => IsDarkTheme ? Colors.White : Colors.Black;
    
    protected override void OnApply()
    {
        // Default styles for all instances of a control type
        LabelStyles.Default = label => label
            .TextColor(OnSurface)
            .FontFamily("Inter");
        
        ButtonStyles.Default = button => button
            .BackgroundColor(Primary)
            .TextColor(Colors.White)
            .CornerRadius(8)
            .FontSize(16);
        
        // Named variants
        ButtonStyles.Themes["outline"] = button => button
            .BackgroundColor(Colors.Transparent)
            .BorderColor(Primary)
            .TextColor(Primary);
        
        ButtonStyles.Themes["text"] = button => button
            .BackgroundColor(Colors.Transparent)
            .TextColor(Primary);
    }
}

// Usage:
Button("Save").ThemeKey("primary")   // applies ButtonStyles.Themes["primary"]
Button("Cancel").ThemeKey("outline") // applies ButtonStyles.Themes["outline"]
```

### R7: 3rd-Party MAUI Control Interop

**Requirement**: Wrap any `Microsoft.Maui.Controls` view (including 3rd-party like Syncfusion) inside the Orbit tree.

Two strategies:

**Strategy A: NativeHost wrapper** (simple, always works)
```csharp
// Wrap any MAUI control
public class NativeHost : View
{
    private readonly Func<BindableObject> _factory;
    
    public NativeHost(Func<BindableObject> factory) => _factory = factory;
    
    // Creates a MAUI control and embeds it via a platform adapter
    internal override void OnMount() { /* create and embed */ }
}

// Usage:
NativeHost(() => new SfRadialGauge
{
    Axes = new RadialAxisCollection
    {
        new RadialAxis { Minimum = 0, Maximum = 100, ... }
    }
})
.Frame(200, 200)
```

**Strategy B: Scaffold generator for 3rd-party controls** (better DX, opt-in)
```csharp
// Developer runs a CLI tool:
// orbit scaffold --assembly Syncfusion.Maui.Gauges --types SfRadialGauge

// Generates:
public partial class SfRadialGauge : NativeHostView<Syncfusion.Maui.Gauges.SfRadialGauge>
{
    // Generated fluent setters that delegate to native control
}
```

### R8: Navigation

**Requirement**: MauiReactor's typed Shell navigation with Orbit components.

```csharp
// Shell definition
class App : Component
{
    public override View Render() =>
        Shell(
            TabBar(
                Tab(
                    ShellContent<HomePage>()
                        .Title("Home")
                        .Icon("home.png")
                ),
                Tab(
                    ShellContent<SettingsPage>()
                        .Title("Settings")
                        .Icon("settings.png")
                )
            )
        );
}

// Navigation with typed props
await Shell.GoToAsync<CoffeeDetailPage>(p => p.CoffeeId = 42);

// Stack navigation
await Navigation.PushAsync<EditPage>(p => p.Item = selectedItem);
```

### R9: Hot Reload

**Requirement**: Support both C# Hot Reload (MAUI built-in) and the Reloadify3000 approach.

```csharp
// Component merge during hot reload
protected override void MergeWith(View newNode)
{
    if (newNode.GetType().FullName == GetType().FullName)
    {
        // Transfer state to new component instance (MauiReactor pattern)
        if (newNode is IComponentWithState newComponent)
            newComponent.State = this.State;
        
        // Transfer handler (Comet pattern — no re-creation)
        ((View)newNode).ViewHandler = this.ViewHandler;
        this.ViewHandler = null;
        
        // Recursively merge children
        base.MergeWith(newNode);
    }
    else
    {
        Unmount();
    }
}
```

### R10: Reconciliation Algorithm

**Requirement**: Improved diff algorithm combining strengths of both.

```csharp
// Key-aware reconciliation (MauiReactor's Key concept + Comet's type checking)
internal void MergeChildrenFrom(IReadOnlyList<View> oldChildren)
{
    var newChildren = Children;
    
    // Phase 1: Key-based matching (if keys present)
    var keyedOld = oldChildren.Where(c => c.Key != null).ToDictionary(c => c.Key);
    var keyedNew = newChildren.Where(c => c.Key != null).ToList();
    foreach (var newChild in keyedNew)
    {
        if (keyedOld.TryGetValue(newChild.Key, out var oldChild))
        {
            oldChild.MergeWith(newChild); // Reuse by key
        }
    }
    
    // Phase 2: Index-based matching for non-keyed children (MauiReactor default)
    // Phase 3: Type-based lookahead for insertions/deletions (Comet's approach)
}
```

---

## 6. Control System Design

### Control Categories

**Tier 1: Core Controls** (generated from IView interfaces — Comet approach)
These implement MAUI interfaces directly and use MAUI's built-in handlers.

| Orbit Control | MAUI Interface | MAUI Handler |
|---------------|----------------|--------------|
| `Label` | `ILabel` | `LabelHandler` |
| `Button` | `ITextButton` | `ButtonHandler` |
| `Image` | `IImage` | `ImageHandler` |
| `TextField` | `IEntry` | `EntryHandler` |
| `TextEditor` | `IEditor` | `EditorHandler` |
| `Toggle` | `ISwitch` | `SwitchHandler` |
| `Slider` | `ISlider` | `SliderHandler` |
| `Stepper` | `IStepper` | `StepperHandler` |
| `Picker` | `IPicker` | `PickerHandler` |
| `DatePicker` | `IDatePicker` | `DatePickerHandler` |
| `TimePicker` | `ITimePicker` | `TimePickerHandler` |
| `ProgressBar` | `IProgress` | `ProgressBarHandler` |
| `ActivityIndicator` | `IActivityIndicator` | `ActivityIndicatorHandler` |
| `CheckBox` | `ICheckBox` | `CheckBoxHandler` |
| `SearchBar` | `ISearchBar` | `SearchBarHandler` |

**Tier 2: Layout Controls** (hand-written — need custom child management)

| Orbit Control | Behavior |
|---------------|----------|
| `VStack` | Vertical stack, `IStackLayout` |
| `HStack` | Horizontal stack, `IStackLayout` |
| `ZStack` | Overlay stack |
| `Grid` | Row/column grid, `IGridLayout` |
| `ScrollView` | Scrollable container |
| `Border` | Decorated container with corner radius |
| `ContentView` | Single-child wrapper |

**Tier 3: Navigation Controls** (hand-written — complex lifecycle)

| Orbit Control | Behavior |
|---------------|----------|
| `ContentPage` | Single-page container |
| `NavigationPage` | Stack-based navigation |
| `Shell` | App shell with tabs/flyout |
| `TabBar` / `Tab` | Tab navigation |
| `FlyoutItem` | Flyout menu item |

**Tier 4: Collection Controls** (hand-written — virtualization)

| Orbit Control | Behavior |
|---------------|----------|
| `CollectionView` | Virtualized list/grid |
| `ListView` | Simple list with templates |
| `CarouselView` | Horizontal swipe paging |

**Tier 5: Interop Controls** (bridge to MAUI Controls layer)

| Orbit Control | Behavior |
|---------------|----------|
| `NativeHost<T>` | Wrap any `Microsoft.Maui.Controls.View` |
| `NativeHost` | Wrap via factory function |

### Generated Control Architecture

```
[Generate(typeof(ITextButton))]
                 │
                 ▼
    ┌─────────────────────────┐
    │   Source Generator       │
    │   reads ITextButton      │
    │   members via Roslyn     │
    └────────┬────────────────┘
             │
    ┌────────▼────────────────┐
    │   Button : View,         │
    │           ITextButton    │
    │                          │
    │   // IView props from    │
    │   // environment dict    │
    │                          │
    │   string ITextButton.Text│
    │     => GetEnv("Text")    │
    └────────┬────────────────┘
             │
    ┌────────▼────────────────┐
    │   ButtonExtensions       │
    │   .Text(string)          │
    │   .OnClicked(Action)     │
    │   .FontSize(double)      │
    │   .Bold()                │
    └────────┬────────────────┘
             │
    ┌────────▼────────────────┐
    │   Component.Button()     │
    │   Component.Button(text) │
    │   Component.Button(      │
    │     text, clicked)       │
    └──────────────────────────┘
```

---

## 7. State Management Design

### Hybrid Approach

The converged framework supports BOTH patterns — MauiReactor's explicit SetState for components, and Comet's automatic tracking for simple reactive bindings.

#### Primary: Component State (MauiReactor pattern)

```csharp
class PageState
{
    public string Title { get; set; } = "Untitled";
    public List<Item> Items { get; set; } = [];
    public bool IsLoading { get; set; }
}

class MyPage : Component<PageState>
{
    // SetState mutates + invalidates (explicit, predictable)
    void LoadItems() => SetState(async s =>
    {
        s.IsLoading = true;
        s.Items = await _service.FetchAsync();
        s.IsLoading = false;
    });
    
    public override View Render() =>
        VStack(
            State.IsLoading
                ? ActivityIndicator()
                : Label($"{State.Items.Count} items")
        );
}
```

#### Secondary: Reactive Bindings (Comet pattern, opt-in)

For simple cases where automatic dependency tracking is convenient:

```csharp
class SimplePage : Component
{
    // Reactive<T> = automatic tracking (Comet-inspired)
    readonly Reactive<int> count = 0;
    
    public override View Render() =>
        VStack(
            // Lambda form: auto-tracks count.Value access
            Label(() => $"Count: {count.Value}"),
            Button("++", () => count.Value++)
        );
}
```

The `Reactive<T>` type would use Comet's `INotifyPropertyRead` pattern internally but only for simple, localized state. Complex pages should use `Component<S>` with explicit `SetState`.

### State Comparison

| Aspect | MauiReactor | Comet | Orbit (Converged) |
|--------|-------------|-------|--------------------|
| Declaration | `class S { props }` | `State<T> field = val` | Both: `Component<S>` + `Reactive<T>` |
| Mutation | `SetState(s => ...)` | `field.Value = x` | `SetState()` primary, `.Value =` secondary |
| Trigger | Explicit `Invalidate()` | Auto via PropertyChanged | `SetState` explicit; `Reactive<T>` auto |
| Granularity | Full component re-render | Per-property update possible | Full re-render for Component; per-property for Reactive |
| Batching | Single SetState call | `StateManager.BeginBatch()` | `SetState` naturally batches |
| Hot Reload | State class copied by type name | State fields tracked individually | State class copy (MauiReactor) |

---

## 8. Rendering & Reconciliation Design

### Render Cycle

```
Developer calls SetState(s => s.Count++)
            │
            ▼
    ┌───────────────┐
    │ Dispatch to    │
    │ UI thread      │
    └───────┬───────┘
            │
            ▼
    ┌───────────────┐
    │ Invalidate()   │
    │ _invalidated   │
    │ = true         │
    └───────┬───────┘
            │
            ▼
    ┌───────────────────┐
    │ Layout cycle       │
    │                    │
    │ 1. Render() called │
    │    → new View tree │
    │                    │
    │ 2. MergeChildren   │
    │    old vs new      │
    │                    │
    │ 3. For each child: │
    │    MergeWith()     │
    │    → reuse handler │
    │    → transfer state│
    │                    │
    │ 4. OnUpdate()      │
    │    → apply props   │
    │    to IView        │
    │                    │
    │ 5. Handler reads   │
    │    IView props     │
    │    → updates native│
    └───────────────────┘
```

### Key Design Decision: No BindableObject in the Path

```
CURRENT MAUIREACTOR:
  Component.Render()  →  VisualNode<T>  →  new Microsoft.Maui.Controls.Button()  →  ButtonHandler  →  UIButton
                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                                            Creates full BindableObject with unused
                                            MVVM infrastructure (Styles, Triggers,
                                            Behaviors, BindableProperty storage)

PROPOSED ORBIT:
  Component.Render()  →  Button : ITextButton  →  ButtonHandler  →  UIButton
                          ^^^^^^^^^^^^^^^^^^^^
                          Implements ITextButton directly.
                          Handler reads properties from View.
                          No intermediate BindableObject.
```

This eliminates an entire layer of allocation and indirection per control.

---

## 9. Styling & Theming Design

### Three-Tier Approach

**Level 1: Control-Type Defaults** (MauiReactor pattern)
```csharp
// Applied automatically to every Button instance
ButtonStyles.Default = button => button
    .CornerRadius(8)
    .FontFamily("Inter")
    .Padding(12, 8);
```

**Level 2: Named Themes** (MauiReactor pattern)
```csharp
ButtonStyles.Themes["primary"] = button => button
    .BackgroundColor(Theme.Primary)
    .TextColor(Colors.White);

ButtonStyles.Themes["danger"] = button => button
    .BackgroundColor(Colors.Red)
    .TextColor(Colors.White);

// Usage:
Button("Delete").ThemeKey("danger")
```

**Level 3: Environment Cascading** (Comet pattern, for inherited properties)
```csharp
// Set on a parent — all descendants inherit
VStack(
    Label("Title"),
    Label("Subtitle"),
    Button("Action")
)
.FontFamily("Inter")      // Cascades to all text within
.TextColor(Colors.Gray)   // Cascades to all text within
```

### Theme Switching

```csharp
class AppTheme : Theme
{
    public static Color Primary => IsDarkTheme ? Colors.Purple : Colors.Blue;
    public static Color Surface => IsDarkTheme ? Color.FromHex("#1E1E1E") : Colors.White;
    
    protected override void OnApply()
    {
        // Re-apply all control defaults using current theme colors
        LabelStyles.Default = l => l.TextColor(IsDarkTheme ? Colors.White : Colors.Black);
        ButtonStyles.Default = b => b.BackgroundColor(Primary).TextColor(Colors.White);
        
        // Force full re-render
        InvalidateAll();
    }
}

// Toggle:
AppTheme.ToggleTheme(); // Calls OnApply() → updates all styles → re-renders
```

---

## 10. 3rd-Party Control Interop

### The Challenge

Syncfusion, Telerik, DevExpress, UXDivers, and other 3rd-party vendors ship controls as `Microsoft.Maui.Controls.View` subclasses. They use BindableProperty, XAML, Styles — the full MVVM stack. Our framework must coexist.

### Solution: NativeHost Bridge

```csharp
// Simple factory wrapper
public class NativeHost : View
{
    Func<Microsoft.Maui.Controls.View> _factory;
    Microsoft.Maui.Controls.View? _nativeView;
    
    public NativeHost(Func<Microsoft.Maui.Controls.View> factory)
        => _factory = factory;
    
    internal override void OnMount(IElementHandler parentHandler)
    {
        _nativeView = _factory();
        // Create a handler for the MAUI control and add to parent
        var handler = _nativeView.ToHandler(MauiContext);
        parentHandler.AddChild(handler);
    }
    
    internal override void OnUnmount()
    {
        _nativeView?.Handler?.DisconnectHandler();
        _nativeView = null;
    }
}

// Usage in Orbit component:
public override View Render() =>
    VStack(
        Label("Pressure Gauge"),
        NativeHost(() => new SfRadialGauge
        {
            Axes = { new RadialAxis { Minimum = 0, Maximum = 100 } }
        })
        .Frame(200, 200),
        Label("Current: " + State.Pressure)
    );
```

### Advanced: Typed Native Wrappers

For frequently-used 3rd-party controls, generate typed wrappers:

```csharp
// User defines a thin wrapper:
[NativeControl(typeof(Syncfusion.Maui.Gauges.SfRadialGauge))]
public partial class RadialGauge : NativeHost<SfRadialGauge>
{
    // Source generator creates fluent setters for the native control's properties
}

// Usage becomes fluent:
RadialGauge()
    .Minimum(0)
    .Maximum(100)
    .Value(State.Pressure)
    .Frame(200, 200)
```

### Interop Comparison

| Aspect | MauiReactor | Comet | Orbit |
|--------|-------------|-------|-------|
| **Mechanism** | Native scaffolding (same system as built-in controls) | `MauiViewHost(control)` | `NativeHost(() => control)` + optional scaffolding |
| **Property updates** | Same as any other control (SetValue) | Manual — caller manages | Factory re-invoked on state change OR typed wrapper |
| **Overhead** | None extra (all controls use BindableObject anyway) | Extra MauiViewHost container | Bridge adapter per native control |
| **3rd party DX** | Seamless — same as built-in controls | Awkward — must use MAUI APIs inside Comet tree | Clean factory pattern + optional fluent wrappers |

---

## 11. Navigation Design

### Shell-Based (Primary)

```csharp
class App : Component
{
    public override View Render() =>
        Shell(
            FlyoutItem(
                Tab("Home", "home.png",
                    ShellContent<HomePage>()),
                Tab("Settings", "settings.png",
                    ShellContent<SettingsPage>())
            ).FlyoutDisplayOptions(FlyoutDisplayOptions.AsMultipleItems)
        )
        .FlyoutBehavior(FlyoutBehavior.Flyout);
}

// Navigate:
await Shell.GoToAsync<DetailPage>(p => p.ItemId = 42);
await Shell.GoToAsync("//home");  // Route-based
await Shell.GoBackAsync();
```

### Stack-Based (Secondary)

```csharp
// From within a component:
await Navigation.PushAsync<EditPage>(p => p.Item = selected);
await Navigation.PopAsync();
await Navigation.PushModalAsync<LoginPage>();
```

### Comparison

| Feature | MauiReactor | Comet | Orbit |
|---------|-------------|-------|-------|
| Shell | Full Shell wrapper | Basic TabView + NavigationView | Full Shell (MauiReactor-style) |
| Typed nav | `GoToAsync<T>(props)` | `Navigate<T>(props)` | `GoToAsync<T>(props)` |
| Back nav | `GoBackAsync()` | Manual pop | `GoBackAsync()` |
| Query params | Via props object | Not supported | Via props object |
| Deep linking | Shell routes | Not supported | Shell routes |

---

## 12. Hot Reload Design

### C# Hot Reload (Built-in .NET)

Both frameworks support this via `[assembly: MetadataUpdateHandler]`. The handler:
1. Detects type replacements
2. Finds all mounted components of the replaced type
3. Creates new instances with new code
4. Transfers state from old → new
5. Re-renders

### Reloadify3000 (External, for VS Code)

Provides immediate feedback without debugger. Works by:
1. File watcher detects save
2. Recompiles changed types
3. Sends new assembly to running app
4. App replaces component types and re-renders

### State Preservation Strategy

```csharp
// During hot reload, state transfer happens in MergeWith:
protected override void MergeWith(View newNode)
{
    // Match by full type name (survives assembly reload)
    if (newNode.GetType().FullName == GetType().FullName && _isMounted)
    {
        // 1. Transfer state object
        if (newNode is IComponentWithState newComp && this is IComponentWithState oldComp)
        {
            if (newComp.StateType == oldComp.StateType)
                newComp.State = oldComp.State;           // Same type: direct assign
            else
                CopyProperties(oldComp.State, newComp.State); // Different assembly: property copy
        }
        
        // 2. Transfer handler (avoid native control recreation — Comet's key insight)
        ((View)newNode).ViewHandler = this.ViewHandler;
        this.ViewHandler = null;
        
        // 3. Merge children recursively
        base.MergeWith(newNode);
    }
}
```

---

## 13. Migration Path

### From MauiReactor

MauiReactor developers would find the Orbit API nearly identical:

| MauiReactor | Orbit | Change Required |
|-------------|-------|-----------------|
| `Component<S>` | `Component<S>` | None |
| `Render()` returns `VisualNode` | `Render()` returns `View` | Return type change |
| `SetState(s => ...)` | `SetState(s => ...)` | None |
| `Button("text").OnClicked(...)` | `Button("text").OnClicked(...)` | None |
| `Label("text").FontSize(18)` | `Label("text").FontSize(18)` | None |
| `VStack(child1, child2)` | `VStack(child1, child2)` | None |
| `.ThemeKey("primary")` | `.ThemeKey("primary")` | None |
| `Shell.GoToAsync<T>()` | `Shell.GoToAsync<T>()` | None |
| `NativeControl` access | `NativeHost(() => ...)` for 3rd party | Pattern change for interop |

**Estimated effort**: ~80% of code is identical. Main changes are namespace + return types.

### From Comet

Comet developers would need to adopt the Component model:

| Comet | Orbit | Change Required |
|-------|-------|-----------------|
| `class X : View` with `[Body]` | `class X : Component<S>` with `Render()` | Class hierarchy change |
| `readonly State<int> x = 0` | State class or `Reactive<int>` | State model change |
| `new Button("text")` | `Button("text")` | Remove `new` keyword |
| `new VStack { child1, child2 }` | `VStack(child1, child2)` | Init → params |
| `.OnTap(_ => ...)` | `.OnTapped(() => ...)` | Method name change |
| Environment cascading | Environment cascading (same!) | None |
| `MauiViewHost(ctrl)` | `NativeHost(() => ctrl)` | Wrapper name change |

**Estimated effort**: ~60% rewrite due to fundamental model change (View→Component, State<T>→state class).

### From MAUI XAML/MVVM

The framework would be a complete paradigm shift for MAUI XAML developers, similar to the existing MauiReactor migration path. MauiReactor has excellent documentation for this transition.

---

## 14. Risk Analysis

### High Risk

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Handler compatibility** — MAUI handlers expect specific IView behavior that Comet's environment-based property storage might not satisfy | Subtle rendering bugs across platforms | Extensive platform testing; start with Comet's proven handler integration |
| **Performance regression** — Environment dictionary lookups vs direct property access | Slower property reads | Cache hot-path properties; benchmark against both existing frameworks |
| **3rd-party interop edge cases** — NativeHost bridge may not handle all scenarios (animations, gestures crossing boundaries) | Broken 3rd-party controls | Keep NativeHost implementation close to Comet's MauiViewHost which is proven |

### Medium Risk

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Source generator complexity** — Generating from IView interfaces is less straightforward than from Controls | Slow generator development | Start with Comet's existing generator; extend incrementally |
| **Incomplete IView coverage** — Not all MAUI controls have clean IView interfaces | Missing controls | Fall back to NativeHost<T> scaffolding for controls without IView |
| **Community adoption** — Yet another .NET MAUI framework | Fragmented ecosystem | Position as evolution, not competitor; migration guides |

### Low Risk

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Hot reload regression** — New architecture might break existing hot reload | Development velocity | Both frameworks have working hot reload; combine proven approaches |
| **State transfer complexity** — Merging two state models | Confusing API | Document clear guidance: Component<S> for pages, Reactive<T> for local state |

---

## Appendix A: File-by-File Architecture Map

### Comet Core (Current)
```
src/Comet/
├── Controls/
│   ├── View.cs                 (1008 lines) — IView impl, Body, handlers
│   ├── ContainerView.cs        — Children management
│   ├── ContentView.cs          — Single-child container
│   ├── ListView.cs             — Virtual list
│   ├── ControlsGenerator.cs    — [CometGenerate] attributes (20 controls)
│   └── ... (61 total files)
├── Handlers/
│   └── View/CometViewHandler.*.cs — Platform handlers
├── State.cs                    (105 lines) — State<T>
├── StateManager.cs             (590 lines) — Dependency tracking
├── BindingObject.cs            (198 lines) — INotifyPropertyRead
├── Binding.cs                  — Reactive bindings
├── EnvironmentData.cs          (272 lines) — Property storage
├── EnvironmentAware.cs         (328 lines) — Cascading lookups
├── Helpers/
│   └── DatabindingExtensions.cs (250 lines) — Diff algorithm
└── Comet.SourceGenerator/      — Roslyn generator
```

### MauiReactor Core (Current)
```
src/MauiReactor/
├── VisualNode.cs               (1065 lines) — Base node, reconciliation
├── Component.cs                (571 lines) — Component<S,P>, SetState
├── Component.partial.cs        — Generated factory methods (200+ controls)
├── ReactorApplication.cs       — App entry, builder
├── Theme.cs                    — Theme system
├── ResourceManager.cs          — MAUI resource bridge
├── Button.cs                   (generated) — Wrapper for M.M.C.Button
├── Label.cs                    (generated) — Wrapper for M.M.C.Label
├── ... (183 total .cs files, 148 from WidgetList)
├── Shell.partial.cs            — Navigation extensions
└── MauiReactor.ScaffoldGenerator/ — Source generator
```

### Proposed Orbit Core
```
src/Orbit/
├── Core/
│   ├── View.cs                 — IView impl (from Comet)
│   ├── Component.cs            — Component<S,P> (from MauiReactor)
│   ├── Component.Generated.cs  — Factory methods (generated)
│   └── VisualTree.cs           — Tree reconciliation (merged)
├── Controls/
│   ├── Generated/              — From IView interfaces (Comet approach)
│   │   ├── Button.cs           
│   │   ├── Label.cs            
│   │   └── ...
│   ├── Layout/                 — Hand-written
│   │   ├── VStack.cs           
│   │   ├── HStack.cs           
│   │   └── Grid.cs             
│   ├── Navigation/             — From MauiReactor
│   │   ├── Shell.cs            
│   │   └── NavigationPage.cs   
│   └── Collections/            — Hand-written
│       ├── CollectionView.cs   
│       └── ListView.cs         
├── State/
│   ├── Reactive.cs             — Reactive<T> (from Comet's State<T>)
│   ├── StateTracking.cs        — INotifyPropertyRead (from Comet)
│   └── Binding.cs              — Func<T> reactive bindings (from Comet)
├── Environment/
│   ├── EnvironmentData.cs      — Property storage (from Comet)
│   ├── EnvironmentKeys.cs      — Key constants (from Comet)
│   └── EnvironmentCascading.cs — Lookup chain (from Comet)
├── Theming/
│   ├── Theme.cs                — Theme base (from MauiReactor)
│   ├── ControlStyles.cs        — Per-type defaults (from MauiReactor)
│   └── ThemeKey.cs             — Named variants (from MauiReactor)
├── Interop/
│   ├── NativeHost.cs           — MAUI control bridge
│   └── NativeHost{T}.cs        — Typed wrapper
├── Handlers/
│   ├── OrbitViewHandler.cs     — Component handler (from Comet)
│   └── Platform/               — Platform-specific (from Comet)
├── Navigation/
│   ├── ShellExtensions.cs      — GoToAsync<T> (from MauiReactor)
│   └── NavigationExtensions.cs — Push/Pop (from MauiReactor)
├── HotReload/
│   ├── HotReloadHandler.cs     — MetadataUpdateHandler
│   └── TypeLoader.cs           — Assembly reload (from MauiReactor)
└── Orbit.SourceGenerator/
    ├── ControlGenerator.cs     — From IView interfaces (Comet approach)
    ├── FactoryGenerator.cs     — Component.Button() etc (MauiReactor approach)
    ├── ExtensionGenerator.cs   — .FontSize() etc (merged)
    └── StylesGenerator.cs      — ButtonStyles etc (MauiReactor approach)
```

---

## Appendix B: Decision Matrix

| Decision | Choose Comet | Choose MauiReactor | Rationale |
|----------|-------------|-------------------|-----------|
| Base class hierarchy | ✅ View : IView | | Direct handler integration, no BindableObject |
| Control creation API | | ✅ Method DSL | Cleaner, more functional feel |
| State model | | ✅ Component<S> + SetState | Structured, predictable, batched |
| Reactive bindings | ✅ Binding<T> + auto-tracking | | Useful for simple cases, opt-in |
| Property storage | ✅ Environment dictionary | | Cascading, flexible, no BindableProperty |
| Theming | | ✅ ControlStyles + ThemeKey | Elegant, per-type, named variants |
| Environment cascade | ✅ Three-level lookup | | More powerful than style-only |
| Source generator input | ✅ IView interfaces | | Targets right abstraction level |
| Source generator output | | ✅ Factory + Extensions + Styles | Better developer experience |
| Reconciliation | Both | Both | Key + index + type matching |
| Hot reload state | | ✅ Type-name matching + copy | Simple, proven |
| Handler reuse | ✅ Transfer handler on merge | | Avoids native control recreation |
| Navigation | | ✅ Shell + typed GoToAsync | Full-featured, typed |
| 3rd-party interop | ✅ NativeHost bridge | | Clean separation; MauiReactor scaffolding as optional |
| Lifecycle | | ✅ OnMounted/OnWillUnmount | Explicit, React-like |
| DI access | | ✅ Component.Services | Convenient static access |
| Animation | | ✅ RxAnimation | Per-property animation support |

**Final tally: Comet engine (8) + MauiReactor API (11) + Both (1)**

This confirms the thesis: **MauiReactor's API on Comet's engine.**
