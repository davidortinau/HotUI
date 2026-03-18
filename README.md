<img src="https://repobeats.axiom.co/api/embed/f917a77cbbdeee19b87fa1f2f932895d1df18b56.svg" />

# Comet ☄️

[![dev-build](https://github.com/dotnet/Comet/actions/workflows/dev.yml/badge.svg)](https://github.com/dotnet/Comet/actions/workflows/dev.yml)  [![Clancey.Comet on fuget.org](https://www.fuget.org/packages/Clancey.Comet/badge.svg)](https://www.fuget.org/packages/Clancey.Comet)
[Chat on Discord](https://discord.gg/7Ms7ptM)


What is Comet? Comet is a modern way of writing cross-platform UIs. Based on [.NET MAUI](https://docs.microsoft.com/en-us/dotnet/maui/what-is-maui), it follows the Model View Update (MVU) pattern and magically databinds for you!

Watch this video to get a preview of the developer experience:

[![Video Demo](http://img.youtube.com/vi/-Ieg9UadN8s/0.jpg)](http://www.youtube.com/watch?v=-Ieg9UadN8s)

## Getting Started

When you're ready to take a ride on the Comet, head over to the wiki and follow the [Getting Started](https://github.com/Clancey/Comet/wiki/Getting-Started) guide.

## Evolved MVU Surface

Comet now ships an evolved, component-first MVU surface alongside the classic `[Body]` API. The project name, package name, and namespaces stay **Comet** — you migrate the API surface, not the brand.

``` cs
public class CounterPage : Component<CounterState>
{
public override View Render() =>
new VStack
{
new Text($"Count: {State.Count}"),
new Button("Increment", () => SetState(s => s.Count++)),
};
}
```

Use the evolved surface when you want:

- `Component<TState>` for local state managed with `SetState(...)`
- `Component<TState, TProps>` for typed props passed during navigation
- `Reactive<T>` for lightweight reactive values outside component state classes
- typed navigation through `Navigation.Navigate<TView>(props)` or `CometShell.GoToAsync<TView>(props)`

Reference implementations:

- [Comet Counter sample](sample/CometMauiApp/README.md)
- [Barista Notes coffee sample](sample/CometBaristaNotes/README.md)
- [Comet TaskApp sample](sample/CometTaskApp)
- [Comet AllTheLists sample](sample/CometAllTheLists)
- [Migration guide](docs/migration-guide.md)

For sample-grade tab layouts today, prefer `TabView` + `NavigationView` tabs while `TabbedPage` handler wiring remains unfinished.

## Key Concepts

### Classic `[Body]` surface (still supported)

Comet is based on the MVU architecture:

![MVU pattern](art/mvu-pattern.png)

`View` is a screen. Views have a `Body` method that you can assign either by using an attribute `[Body]`:

``` cs
public class MyPage : View {
    [Body]
    View body () => new Text("Hello World");
}
```

Or manually from your constructor:

``` cs
public class MyPage : View {
    public MyPage() {
        Body = body;
    }
    View body () => new Text("Hello World");
}
```

## Navigation

Comet now includes a fluent Shell wrapper plus typed navigation helpers so you can keep route names out of call sites:

``` cs
CometShell.RegisterRoute<ProjectDetailPage>("project-detail");

var shell = new CometShell()
    .AddItem("Projects", item => item
        .WithRoute("//projects")
        .AddSection("Browse", section => section
            .AddContent<ProjectListPage>("List")));

await new Button("Open").GoToAsync<ProjectDetailPage>(new { id = 42 });
```

If the destination is a `Component<TState, TProps>`, typed navigation will apply a matching props object before the view is presented.

Inside a `NavigationView`, you can use the same typed-parameter pattern without route strings:

``` cs
Navigation.Navigate<ProjectDetailPage>(new ProjectDetailProps { Id = 42 });
```

## Interop

Comet now ships a three-way host bridge:

- `MauiViewHost` embeds a MAUI `IView` inside a Comet view tree.
- `NativeHost` embeds a raw platform view and lets you synchronize native properties from Comet state.
- `CometHost` embeds a Comet `View` inside MAUI pages and controls.

``` cs
var host = new NativeHost(ctx => CreateNativeLabel(ctx))
    .Sync("Text", "Hello native", (native, text) => UpdateNativeLabel(native, text))
    .Frame(height: 44);
```

Use the `NativeHost` factory to return the raw platform view you want to host (`UIView`, `Android.Views.View`, or `FrameworkElement`).

## Hot Reload

Using Hot Reload is the fastest way to develop your user interface.

The setup is simple and only requires a few steps:
1. Install the Visual Studio extension `Comet.Reload` from [Releases](https://github.com/dotnet/Comet/releases) (or [Comet for .NET Mobile](https://marketplace.visualstudio.com/items?itemName=Clancey.comet-debug) if you use Visual Studio Code)
2. Install the [Comet project template](https://www.nuget.org/packages/Clancey.Comet.Templates.Multiplatform) available on NuGet.
3. Add this short snippet to your `AppDelegate.cs` and/or `MainActivity.cs`, or equivalent.

``` cs
#if DEBUG
Comet.Reload.Init();
#endif
```

 See the sample projects [here](https://github.com/dotnet/Comet/tree/dev/sample) for examples.

## State

As of right now there are two supported families of state APIs. The evolved surface uses `Component<TState>` / `Component<TState, TProps>` with `SetState(...)`; the classic surface below uses `State<T>` and `[State]`.

### 1. Simple data types like int, bool?

Just add a `State<T>` field to your View

``` cs
class MyPage : View {
    readonly State<int> clickCount = 1;
}
```

`View` is state aware. When the state changes, databinding will automatically update, or rebuild the view as needed.

### 2. Do you want to use more complex data types?

You can either implement [INotifyPropertyRead](https://github.com/Clancey/Comet/blob/master/src/Comet/BindingObject.cs#L13) or you can use [BindingObject](https://github.com/Clancey/Comet/blob/master/src/Comet/BindingObject.cs) to make it even simpler.

Add it as a Field/Property, and add the `[State]` attribute!


``` cs
public class MainPage : View {
    class MyBindingObject : BindingObject {
        public bool CanEdit {
            get => GetProperty<bool> ();
            set => SetProperty (value);
        }
        public string Text {
            get => GetProperty<string> ();
            set => SetProperty (value);
        }
    }

    [State]
    readonly MyBindingObject state;
}

```

`INotifyPropertyRead` is just like `INotifyPropertyChanged`. Just call `PropertyRead` whenever a property getter is called. And `PropertyChanged` whenever a property value changes.

### How do I use the State?

Simply update the stateful value and the framework handles the rest.

``` cs
public class MyPage : View {

    readonly State<int> clickCount = 1;
    readonly State<string> text = "Hello World";

    public MyPage() {
        Body = () => new VStack {
            new Text (text),
            new Button("Update Text", () => state.Text = $"Click Count: {clickCount.Value++}")
        };

    }
}
```

That is all!, now when the text changes everything updates.

### What if I want to format my value without an extra state property?

While `new Button("Update Text", () => state.Text = $"Click Count: {clickCount.Value++}" )` works, it isn't efficient.

Instead, use `new Text(()=> $"Click Count: {clickCount}")`.

``` cs
public class MyPage : View {

    readonly State<int> clickCount = new State<int> (1);

    public MyPage() {
        Body = () => new VStack {
            new Text (() => $"Click Count: {clickCount}"),
            new Button("Update Text", () => {
                clickCount.Value++;
            }
        };
    }
}

```


## What platforms are supported?

Comet is developed on top of .NET MAUI handlers, providing its own implementation for interfaces such as `Microsoft.Maui.IButton` and other controls. Any platform supported by .NET MAUI can be targeted:

* Windows
* Android
* iOS
* macOS
* Blazor

Non-MAUI application models, such as UWP or WPF, are not supported.

# Disclaimer

Comet is a **proof of concept**. There is **no** official support. Use at your own risk.
