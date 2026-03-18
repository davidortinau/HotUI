# Comet ☄️

[![dev-build](https://github.com/dotnet/Comet/actions/workflows/dev.yml/badge.svg)](https://github.com/dotnet/Comet/actions/workflows/dev.yml)  [![Clancey.Comet on fuget.org](https://www.fuget.org/packages/Clancey.Comet/badge.svg)](https://www.fuget.org/packages/Clancey.Comet)

Comet is an MVU framework for [.NET MAUI](https://learn.microsoft.com/dotnet/maui/what-is-maui). Write your entire UI in C# with a reactive state system that tracks what you read and updates only what changed. No XAML, no view models, no binding markup.

```csharp
public class MyApp : View
{
    [Body]
    View body() => new Text("Hello, Comet!");
}
```

## Reactive State

One primitive: `Reactive<T>`. Declare it, read `.Value` in a lambda, write `.Value` anywhere. The UI updates automatically.

```csharp
public class CounterView : View
{
    readonly Reactive<int> count = 0;

    [Body]
    View body() => new VStack {
        new Text(() => $"Count: {count.Value}"),
        new Button("Increment", () => count.Value++)
    };
}
```

When the button increments `count.Value`, only the `Text` control updates — `body()` does not re-execute. Comet tracks the read inside the lambda and performs a fine-grained update at the control level.

### Two-Way Binding

Bind a `Reactive<T>` to input controls. Typing updates the signal; changing the signal updates the control.

```csharp
public class GreetingView : View
{
    readonly Reactive<string> name = "World";

    [Body]
    View body() => new VStack {
        new Text(() => $"Hello, {name.Value}!"),
        new TextField(() => name.Value, () => "Enter name")
            .OnTextChanged(v => name.Value = v ?? "")
    };
}
```

## Before / After: XAML+MVVM vs Comet

A text field bound to a greeting label.

**XAML + MVVM** — 3 files, ~30 lines:

```xml
<!-- GreetingPage.xaml -->
<VerticalStackLayout>
    <Label Text="{Binding Greeting}" />
    <Entry Text="{Binding Name, Mode=TwoWay}" />
</VerticalStackLayout>
```
```csharp
// GreetingViewModel.cs
public partial class GreetingViewModel : ObservableObject
{
    [ObservableProperty] string name = "World";
    public string Greeting => $"Hello, {Name}!";

    partial void OnNameChanged(string value) =>
        OnPropertyChanged(nameof(Greeting));
}
```
```csharp
// GreetingPage.xaml.cs
public partial class GreetingPage : ContentPage
{
    public GreetingPage() {
        InitializeComponent();
        BindingContext = new GreetingViewModel();
    }
}
```

**Comet** — 1 file, 10 lines:

```csharp
public class GreetingView : View
{
    readonly Reactive<string> name = "World";

    [Body]
    View body() => new VStack {
        new Text(() => $"Hello, {name.Value}!"),
        new TextField(() => name.Value, () => "Enter name")
            .OnTextChanged(v => name.Value = v ?? "")
    };
}
```

Same result. The binding, change notification, and UI update are all handled by `Reactive<T>`.

## Getting Started

Comet requires .NET 10 with the MAUI workload.

```bash
dotnet workload install maui
```

Add the NuGet package to your project:

```bash
dotnet add package Clancey.Comet
```

Register Comet handlers in `MauiProgram.cs`:

```csharp
var builder = MauiApp.CreateBuilder();
builder.UseMauiApp<MyApp>();
builder.UseCometHandlers();
return builder.Build();
```

Then define your app as a `View`:

```csharp
public class MyApp : View
{
    [Body]
    View body() => new CounterView();
}
```

## Hot Reload

MAUI's built-in hot reload works with Comet. Change a `[Body]` method, save, and the view updates on the running app. State is preserved across reloads.

## Fluent API

Layout and styling use method chaining:

```csharp
new VStack {
    new Text(() => $"Count: {count.Value}")
        .FontSize(48)
        .Color(Colors.DodgerBlue),
    new HStack {
        new Button("OK", onOk),
        new Button("Cancel", onCancel)
    }
}
```

## Navigation

Comet includes a fluent Shell wrapper and typed navigation:

```csharp
CometShell.RegisterRoute<DetailPage>("detail");

var shell = new CometShell()
    .AddItem("Projects", item => item
        .WithRoute("//projects")
        .AddSection("Browse", section => section
            .AddContent<ListPage>("List")));

// Navigate with typed parameters
Navigation.Navigate<DetailPage>(new DetailProps { Id = 42 });
```

## MAUI Interop

Embed MAUI views in Comet or Comet views in MAUI:

- **`CometHost`** — use a Comet `View` inside a MAUI `ContentPage`
- **`MauiViewHost`** — use a MAUI `IView` inside a Comet view tree
- **`NativeHost`** — embed raw platform views (`UIView`, `Android.Views.View`)

## Samples

The [`sample/`](sample/) directory contains working apps:

| Sample | What it demonstrates |
|--------|---------------------|
| [CometControlsGallery](sample/CometControlsGallery) | 30+ controls with sidebar navigation |
| [Comet.Sample](sample/Comet.Sample) | 50+ component and feature demos |
| [CometMauiApp](sample/CometMauiApp) | Minimal starter template |
| [CometTaskApp](sample/CometTaskApp) | TabView navigation pattern |
| [CometBaristaNotes](sample/CometBaristaNotes) | Real app with Syncfusion gauges |
| [CometStressTest](sample/CometStressTest) | Performance and stress tests |

## Build

```bash
# Source generator first, then the framework
dotnet build src/Comet.SourceGenerator/Comet.SourceGenerator.csproj -c Release
dotnet build src/Comet/Comet.csproj -c Release

# Tests
dotnet build tests/Comet.Tests/Comet.Tests.csproj -c Release
dotnet test tests/Comet.Tests/Comet.Tests.csproj --no-build -c Release
```

## Platforms

Comet targets every platform .NET MAUI supports: **Android**, **iOS**, **macOS (Catalyst)**, and **Windows**.

## Disclaimer

Comet is a **proof of concept**. There is no official support. Use at your own risk.
