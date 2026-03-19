# Comet Framework Knowledge

Comet is an MVU framework on .NET MAUI. Views are C# functions, state changes auto-trigger re-rendering. No XAML.

## Build Order (Required)

```bash
dotnet build src/Comet.SourceGenerator/Comet.SourceGenerator.csproj -c Release
dotnet build src/Comet/Comet.csproj -c Release
dotnet build tests/Comet.Tests/Comet.Tests.csproj -c Release
dotnet test tests/Comet.Tests/Comet.Tests.csproj --no-build -c Release
```

## Key Patterns

### View with [Body]

```csharp
using Comet;
using Microsoft.Maui.Graphics;
using static Comet.CometControls;

public class MyView : View
{
	[Body]
	View body() => VStack(
		Text("Hello").FontSize(24).Color(Colors.Blue),
		Button("Tap", () => Console.WriteLine("tapped"))
	);
}
```

### Reactive State

```csharp
readonly Reactive<int> count = 0;

[Body]
View body() => VStack(
	Text(() => $"Count: {count.Value}"),
	Button("+", () => count.Value++)
);
```

`Reactive<T>` and `Signal<T>` are the reactive primitives. Read `.Value` inside a lambda for automatic tracking.

### Component with SetState

```csharp
public class MyState { public int Count { get; set; } }

public class MyComponent : Component<MyState>
{
	public override View Render() => VStack(
		Text($"Count: {State.Count}"),
		Button("+", () => SetState(s => s.Count++))
	);
}
```

### Two-Way Binding

```csharp
// Signal<T> passed to TextField creates automatic two-way binding
readonly Signal<string> name = new("");
TextField(name, "Enter name")

// In Component<TState>, use OnTextChanged callback
TextField(State.Name, "Name").OnTextChanged(t => SetState(s => s.Name = t))
```

### App Entry

```csharp
builder.UseCometApp<MyApp>();

public class MyApp : CometApp
{
	[Body]
	View body() => new MainPage();
}
```

## Mistakes to Avoid

1. `Text($"{count.Value}")` -- NOT reactive. Use `Text(() => $"{count.Value}")`
2. Missing `OnTextChanged` in `Component<TState>` two-way binding
3. Building Comet.csproj before Comet.SourceGenerator.csproj
4. `new VStack(child1, child2)` -- use `new VStack { child1, child2 }` or `VStack(child1, child2)`
5. Missing `using static Comet.CometControls` for factory methods
6. Missing `using Comet.Reactive` for `Signal<T>`, `Computed<T>`, `SignalList<T>`
7. Implicit usings are DISABLED -- all `using` statements must be explicit
8. Mixing `[Body]` (View pattern) with `Render()` (Component pattern) on same class

## Control Callbacks

| Control | Callback |
|---------|----------|
| `TextField` | `.OnTextChanged(Action<string>)` |
| `Toggle` | `.OnToggled(Action<bool>)` |
| `Slider` | `.OnValueChanged(Action<double>)` |
| `Stepper` | `.OnValueChanged(Action<double>)` |
| `CheckBox` | `.OnCheckedChanged(Action<bool>)` |
| `Picker` | `.OnSelectedIndexChanged(Action<int>)` |

## Styling

```csharp
view.Color(Colors.Red)                    // Foreground
view.Background(Colors.Blue)              // Background
view.FontSize(18).FontWeight(FontWeight.Bold)
view.Padding(new Thickness(16))
view.Frame(width: 200, height: 100)
view.ClipShape(new RoundedRectangle(16))
```

## Navigation

```csharp
// Wrap content in NavigationView
NavigationView(new HomePage())

// Push
NavigationView.Navigate(this, new DetailPage());

// Pop
NavigationView.Pop(this);

// ListView auto-navigate
new ListView<T>(items) { ViewFor = x => Text(x.Name) }
	.OnSelectedNavigate(x => new DetailPage(x))
```

## Code Style

- Tabs for indentation
- Allman brace style
- `var` preferred when type is apparent
- No `this.` qualifier

## Reference

Full API reference: [AGENTS.md](../AGENTS.md)
Detailed docs: `docs/controls.md`, `docs/reactive-state-guide.md`, `docs/layout.md`, `docs/navigation.md`

---

# Copilot Coding Agent -- Squad Instructions

You are working on a project that uses **Squad**, an AI team framework. When picking up issues autonomously, follow these guidelines.

## Team Context

Before starting work on any issue:

1. Read `.squad/team.md` for the team roster, member roles, and your capability profile.
2. Read `.squad/routing.md` for work routing rules.
3. If the issue has a `squad:{member}` label, read that member's charter at `.squad/agents/{member}/charter.md` to understand their domain expertise and coding style -- work in their voice.

## Capability Self-Check

Before starting work, check your capability profile in `.squad/team.md` under the **Coding Agent -> Capabilities** section.

- **Good fit** -- proceed autonomously.
- **Needs review** -- proceed, but note in the PR description that a squad member should review.
- **Not suitable** -- do NOT start work. Instead, comment on the issue:
  ```
  This issue doesn't match my capability profile (reason: {why}). Suggesting reassignment to a squad member.
  ```

## Branch Naming

Use the squad branch convention:
```
squad/{issue-number}-{kebab-case-slug}
```
Example: `squad/42-fix-login-validation`

## PR Guidelines

When opening a PR:
- Reference the issue: `Closes #{issue-number}`
- If the issue had a `squad:{member}` label, mention the member: `Working as {member} ({role})`
- If this is a needs-review task, add to the PR description: `This task was flagged as "needs review" -- please have a squad member review before merging.`
- Follow any project conventions in `.squad/decisions.md`

## Decisions

If you make a decision that affects other team members, write it to:
```
.squad/decisions/inbox/copilot-{brief-slug}.md
```
The Scribe will merge it into the shared decisions file.
