# Comet ☄️ — Copilot Instructions

## Build & Test

Requires .NET 10 SDK with the MAUI workload (`dotnet workload install maui`).

```bash
# Build order matters — source generator must build first
dotnet build src/Comet.SourceGenerator/Comet.SourceGenerator.csproj -c Release
dotnet build src/Comet/Comet.csproj -c Release

# Tests reference the maccatalyst DLL directly (not a project reference)
dotnet build tests/Comet.Tests/Comet.Tests.csproj -c Release
dotnet test tests/Comet.Tests/Comet.Tests.csproj --no-build -c Release

# Run a single test
dotnet test tests/Comet.Tests/Comet.Tests.csproj --no-build -c Release --filter "FullyQualifiedName~ClassName.MethodName"
```

The test project references `src/Comet/bin/$(Configuration)/net10.0-maccatalyst/Comet.dll` directly, so Comet must be built for maccatalyst before tests will compile.

## Architecture

Comet is an **MVU (Model-View-Update) framework** built on top of .NET MAUI. It provides a declarative, code-only UI layer where views have a `Body` lambda that returns the UI tree, and state changes automatically trigger re-rendering.

### Core layers

- **`src/Comet`** — The framework library, a single multi-targeted .NET MAUI project (`net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows`).
- **`src/Comet.SourceGenerator`** — A Roslyn source generator (targets `netstandard2.0`) that generates View wrapper classes from MAUI interfaces and auto-implements `[Body]` methods.
- **`tests/Comet.Tests`** — xUnit tests. Test parallelization is disabled (`[assembly: CollectionBehavior(DisableTestParallelization = true)]`). Tests inherit from `TestBase` which calls `UI.Init()`.

### Key abstractions

- **`View`** (`Controls/View.cs`) — Base class for all Comet views. Implements `IView`, `IHotReloadableView`, and many other MAUI interfaces. Has a `Body` property (a `Func<View>`) that returns the view tree.
- **`State<T>`** — A reactive state wrapper extending `BindingObject`. Assigning to `.Value` triggers automatic UI updates via `INotifyPropertyRead`/`INotifyPropertyChanged`.
- **`BindingObject`** — Base class for observable objects. Implements `INotifyPropertyRead` (extends `INotifyPropertyChanged` with `PropertyRead` events) for automatic dependency tracking.
- **`Binding<T>`** — Wraps either a value or a `Func<T>` for automatic databinding. The framework tracks which state properties are read during `Func` evaluation.
- **`CometApp`** — The application entry point, extends `View` and implements `IApplication`. Registered via `builder.UseCometApp<TApp>()`.

### Source generator (`CometGenerateAttribute`)

Many controls (Button, Text, TextField, Slider, Toggle, etc.) are **generated at compile time** from MAUI interfaces using `[assembly: CometGenerate(...)]` attributes in `Controls/ControlsGenerator.cs`. The generator in `CometViewSourceGenerator` reads these attributes and produces View subclasses with constructor parameters, `Binding<T>` properties, and environment key mappings.

The `[Body]` attribute on a method triggers the `AutoNotifyGenerator` to wire up the Body property automatically.

### Handler architecture

Comet views map to MAUI handlers. `CometViewHandler` (with platform-specific files `.Android.cs`, `.iOS.cs`, `.Windows.cs`) bridges Comet's virtual view tree to native platform views. Handlers are registered in `AppHostBuilderExtensions.UseCometHandlers()`.

### Platform-specific code convention

Controlled by `Directory.Build.targets` — files are included/excluded based on target framework:
- `*.iOS.cs` / `iOS/` folder — iOS only
- `*.Android.cs` / `Android/` folder — Android only
- `*.Windows.cs` / `Windows/` folder — Windows only
- `*.Mac.cs` / `Mac/` or `MacCatalyst/` folder — Mac Catalyst only
- `*.Standard.cs` / `Standard/` folder — netstandard/net6.0/net7.0

### Environment system

Properties like colors, fonts, and layout are set via a fluent extension method API (e.g., `.Background(Colors.Red)`, `.FontSize(18)`) that stores values in an environment dictionary keyed by `EnvironmentKeys` constants. These propagate down the view tree.

## NuGet Packing

The main package is `Clancey.Comet`. CI uses `dotnet pack` with a dynamically computed version:

```bash
# Pack the main library (CI computes version as 0.4.$MINOR where $MINOR = $BASE + $GITHUB_RUN_NUMBER)
dotnet pack src/Comet/Comet.csproj -c Release -p:PackageVersion=$VERSION -o ./artifacts

# Push to NuGet.org
dotnet nuget push "artifacts/*.nupkg" --source https://api.nuget.org/v3/index.json --api-key $KEY --skip-duplicate
```

The `.nuspec` files at the repo root are **legacy** (reference net9.0 / Xamarin targets) and are not used by CI. Current packing uses `dotnet pack` which reads packaging metadata from the `.csproj`. The nuspec files are:
- `Comet.nuspec` — main package (legacy, references net9.0 TFMs)
- `Comet.Skia.nuspec` — Skia controls extension (legacy, references Xamarin/UWP TFMs)
- `Comet.Reload.nuspec` — hot reload plugin (legacy, depends on Reloadify3000)

The source generator (`Comet.SourceGenerator.dll` + `Stubble.Core.dll`) is included in the NuGet package under `analyzers/cs/`. `Directory.Build.targets` is included as a build target in the package.

## Samples

The `sample/` directory contains 10 projects. Only `Comet.Sample` is included in the main `Comet.sln`; the others are standalone projects built individually.

```bash
# Build a sample (must build Comet first)
dotnet build sample/Comet.Sample/Comet.Sample.csproj -c Release

# Run a sample on a specific platform
dotnet build sample/CometMauiApp/CometMauiApp.csproj -t:Run -f net10.0-maccatalyst
```

### Sample inventory

| Sample | Purpose | Platforms |
|--------|---------|-----------|
| **Comet.Sample** | 50+ component/feature demos (reference app) | Android, iOS, macCatalyst, Windows |
| **CometMauiApp** | Minimal starter template | Android, iOS, macCatalyst, Windows |
| **CometFeatureShowcase** | Educational showcase of 5 core features (has README) | iOS, macCatalyst |
| **CometAllTheLists** | 5 different list/collection implementations | iOS, macCatalyst |
| **CometTaskApp** | Task manager demonstrating TabView navigation | macCatalyst, Windows |
| **CometProjectManager** | Complex app with Shell, themes, 3rd-party toolkits | iOS, macCatalyst, Windows |
| **CometBaristaNotes** | Coffee note-taking app with Syncfusion gauges | Android, iOS, macCatalyst, Windows |
| **CometWeather** | Weather app with reactive data display | iOS, macCatalyst |
| **CometStressTest** | 6 categories of performance/stress tests | iOS, macCatalyst |
| **MauiReference** | Pure MAUI XAML comparison (does **not** use Comet) | Android, iOS, macCatalyst, Windows, Tizen |

### App entry patterns

Samples use two architectural patterns:
1. **CometApp direct** — `builder.UseCometApp<MyApp>()` where `MyApp` extends `View` with a `[Body]` method. Used by Comet.Sample, CometMauiApp, CometTaskApp.
2. **MAUI Shell + Comet views** — Standard MAUI `Shell` for navigation with Comet views embedded inside `ContentView` containers. Used by CometAllTheLists, CometStressTest, CometFeatureShowcase, CometWeather, CometProjectManager, CometBaristaNotes.

## Hot Reload

Comet has deep hot reload integration built on MAUI's `Microsoft.Maui.HotReload` framework. When code changes are applied, the view tree is intelligently diffed and updated while preserving state.

### How it works

1. IDE detects a code change and sends the updated type.
2. `MauiHotReloadHelper.RegisterReplacedView(className, newType)` registers the replacement.
3. `MauiHotReloadHelper.TriggerReload()` fires, calling `IHotReloadableView.Reload()` on all active views.
4. `View.Reload(isHotReload: true)` triggers `ResetView()` which rebuilds the body, diffs the old and new view trees, transfers state, and reuses platform handlers where possible.

### Key interfaces

- **`IHotReloadableView`** — implemented by `View`. Provides `TransferState(IView newView)` (copies changed properties to the new view) and `Reload()` (triggers a full view reset on the main thread).
- **`IReloadHandler`** — implemented by platform-specific `CometView` classes (iOS, Android, Windows). Called after view tree is rebuilt to update native views.

### Smart diff algorithm

Located in `Helpers/DatabindingExtensions.cs`. The `Diff()` method:
- Checks type equality including hot-reloaded replacement types via `MauiHotReloadHelper.IsReplacedView()`
- Recursively diffs nested/built views
- Detects added, removed, and shifted items in container views
- Reuses platform handlers when view types match
- Runs updates on the main thread

### State transfer

During reload, `TransferState()` copies only **changed** properties from the old view to the new view via `GetState().ChangedProperties`. Environment data is transferred via `PopulateFromEnvironment()`. Handlers and navigation state are also carried over.

### Setup in apps

Samples use a simple `Reload.cs` stub wrapped in `#if DEBUG`:

```csharp
#if DEBUG
public static partial class Reload
{
    public static MauiAppBuilder EnableHotReload(this MauiAppBuilder builder,
        string? ideIp = null, int idePort = 9988)
    {
        // MAUI's built-in Hot Reload handles this automatically.
        return builder;
    }
}
#endif
```

The project template (`templates/single-project/`) includes a more complete `Reload.cs` that optionally connects to the **Reloadify3000** external hot reload server with retry logic (3 attempts, 5-second intervals, default port 9988).

### Testing hot reload

Tests are in `tests/Comet.Tests/` and require `MauiHotReloadHelper.IsEnabled = true`:
- `HotReloadTestsNoParameters.cs` — basic view replacement
- `HotReloadWithParameters.cs` — view replacement with constructor parameters
- `ReloadTransfersStateTest.cs` — state preservation across reloads

## Code Conventions

- **Tabs for indentation** (not spaces) — configured in `.editorconfig`
- **Allman brace style** for types, methods, control blocks, properties, and accessors
- **`var` preferred** when type is apparent or for built-in types
- **No `this.` qualifier** on members
- **Implicit usings are disabled** — all `using` statements are explicit
- Layout containers (`VStack`, `HStack`, `ZStack`, `Grid`) use C# collection initializer syntax for child views
- State fields are typically `readonly State<T> fieldName = defaultValue;` (implicit conversion from `T` to `State<T>`)
- Complex state uses `[State]` attribute on `BindingObject`-derived fields
