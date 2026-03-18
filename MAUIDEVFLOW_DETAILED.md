# MauiDevFlow Architecture Analysis: Tree Walker & Extension Points

## Project Structure

### Core Projects
1. **MauiDevFlow.Agent.Core** (1,337 lines) - Platform-agnostic core
   - Located: `/src/MauiDevFlow.Agent.Core/`
   - NuGet: `Redth.MauiDevFlow.Agent.Core`
   - Key Classes:
     - `VisualTreeWalker` - Core tree walker (1,337 lines)
     - `ElementInfo` - Node representation
     - `DevFlowAgentService` - Main HTTP API (3,360 lines)
     - `CssSelectorEngine`, `ElementInfoOps` - CSS selector support

2. **MauiDevFlow.Agent** (Platform-specific) - Platform implementations
   - Located: `/src/MauiDevFlow.Agent/`
   - Key Classes:
     - `PlatformAgentService : DevFlowAgentService`
     - `PlatformVisualTreeWalker : VisualTreeWalker`
     - Native implementations for iOS, Android, macOS, Windows

3. **MauiDevFlow.Agent.Gtk** (Linux-specific)
   - Located: `/src/MauiDevFlow.Agent.Gtk/`
   - Key Classes:
     - `GtkVisualTreeWalker : VisualTreeWalker`

## Tree Walker Architecture

### Core Class: VisualTreeWalker

**File**: `/src/MauiDevFlow.Agent.Core/VisualTreeWalker.cs`

**Responsibilities**:
1. Walk MAUI visual tree via `IVisualTreeElement.GetVisualChildren()`
2. Generate stable element IDs using Element.Id + AutomationId
3. Extract properties into ElementInfo objects
4. Handle synthetic elements (buttons, tabs, navbars)
5. Support CSS selector queries
6. Hit-test by bounds
7. Reverse-lookup elements by ID

**Key Methods**:
```csharp
public List<ElementInfo> WalkTree(Application app, int maxDepth = 0, int? windowIndex = null)
public ElementInfo? WalkElement(IVisualTreeElement element, string? parentId, int currentDepth, int maxDepth)
public List<ElementInfo> Query(Application app, string? type, string? automationId, string? text)
public List<ElementInfo> QueryCss(Application app, string selector)
public object? GetElementById(string id, Application? app)
public string? GetIdForElement(IVisualTreeElement element)
```

## MAUI Interface Dependencies

### Primary Interface: IVisualTreeElement
- **Usage**: Tree traversal via `GetVisualChildren()`
- **Implemented by**: Element, VisualElement, Page, Shell, etc.

### Key MAUI Types Used:
- `Element` - Base type, has `Element.Id` (Guid)
- `VisualElement : Element` - Visible elements with bounds, opacity, focus, enabled/visible states
- `Page` - Includes ToolbarItems, SearchHandler (Shell), NavigationBar title
- `Shell` - Flyout items, tabs, flyout button
- `NavigationPage` - Navigation bar, back button
- `FlyoutPage` - Flyout toggle
- `TabbedPage` - Tab bar items
- `Application` - Windows (root)

## Node Representation: ElementInfo

**File**: `/src/MauiDevFlow.Agent.Core/ElementInfo.cs`

### Properties Extracted:
```csharp
public class ElementInfo
{
    public string Id { get; set; }                          // Stable ID (AutomationId or derived)
    public string? ParentId { get; set; }
    public string Type { get; set; }                        // Element.GetType().Name
    public string FullType { get; set; }                    // Element.GetType().FullName
    public string? AutomationId { get; set; }
    public string? Text { get; set; }                       // Label.Text, Button.Text, etc.
    public string? Value { get; set; }                      // Switch.IsToggled, Slider.Value, etc.
    public bool IsVisible { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsFocused { get; set; }
    public double Opacity { get; set; }
    public BoundsInfo? Bounds { get; set; }                 // Frame.X/Y/Width/Height
    public BoundsInfo? WindowBounds { get; set; }           // Platform-native absolute coordinates
    public List<string>? Gestures { get; set; }             // [tap, swipe, pan, etc.]
    public List<string>? StyleClass { get; set; }           // CSS style classes
    public string? NativeType { get; set; }                 // Platform handler type
    public Dictionary<string, string?>? NativeProperties { get; set; }
    public List<ElementInfo>? Children { get; set; }        // Recursively nested
}
```

### Type Name Resolution:
- **Short name**: `element.GetType().Name` (e.g., "Button", "Label")
- **Full name**: `element.GetType().FullName` (e.g., "Microsoft.Maui.Controls.Button")

### Text & Value Extraction:
```csharp
info.Text = element switch
{
    Label l => l.Text,
    Button b => b.Text,
    Entry e => e.Text,
    SearchBar sb => sb.Text,
    BaseShellItem si => si.Title,  // Shell items
    _ => null
};

info.Value = element switch
{
    Switch sw => sw.IsToggled.ToString(),
    CheckBox cb => cb.IsChecked.ToString(),
    Slider sl => sl.Value.ToString("F2"),
    Picker pk => pk.SelectedItem?.ToString(),
    DatePicker dp => dp.Date.ToString(),
    _ => null
};
```

## Synthetic Elements Architecture

**Purpose**: Represent non-visual tree elements that exist in MAUI (NavBar, Tabs, Buttons) but aren't IVisualTreeElement

### Marker Types (Inner Classes in VisualTreeWalker):

```csharp
public class NavBarTitleMarker       { Title, Page }         // Navigation/Shell nav bar titles
public class BackButtonMarker        { Navigation, Title }   // NavigationPage back button
public class ToolbarItem             // Native MAUI ToolbarItem
public class SearchHandlerMarker     { Handler }             // Shell SearchHandler
public class FlyoutButtonMarker      { Shell }               // Shell flyout menu button (☰)
public class ShellFlyoutItemMarker   { Item, Shell }         // Shell flyout menu items
public class ShellTabMarker          { Section, Shell }      // Shell bottom tab bar
public class FlyoutToggleMarker      { FlyoutPage }          // FlyoutPage toggle button
public class TabbedPageTabMarker     { Page, TabbedPage }    // TabbedPage tab bar
```

### ID Generation for Synthetics:
- Uses `GenerateObjectId(object element, string? automationId)`
- Derives from backing MAUI object (Page, Shell, etc.)
- Handles collisions with suffixes

### Where Synthetics Are Injected:
- **Pages**: ToolbarItems, NavBarTitle, BackButton, SearchHandler
- **Shell**: FlyoutButton, FlyoutItems (flyout menu), TabBar (bottom tabs)
- **NavigationPage**: NavBar title + back button
- **FlyoutPage**: Flyout toggle button
- **TabbedPage**: Tab bar items

## Extension Points for Comet Integration

### 1. VisualTreeWalker - Subclass Pattern (OPEN)

**Location**: `/src/MauiDevFlow.Agent.Core/VisualTreeWalker.cs`

**Virtual Methods** (Protected):
```csharp
protected virtual VisualTreeWalker CreateTreeWalker() => new VisualTreeWalker();
protected virtual void PopulateSyntheticNativeInfo(ElementInfo info, object marker) { }
protected virtual BoundsInfo? ResolveSyntheticBounds(object marker) => null;
protected virtual BoundsInfo? ResolveWindowBounds(VisualElement ve) => null;
protected virtual string? EnsurePlatformStableId(object platformObj) => null;
protected virtual void PopulateNativeInfo(ElementInfo info, VisualElement ve)
```

**Current Implementations**:
- `PlatformVisualTreeWalker` (iOS/Android/Windows/macOS)
- `GtkVisualTreeWalker` (GTK/Linux)

### 2. DevFlowAgentService - Service Subclass (OPEN)

**Location**: `/src/MauiDevFlow.Agent.Core/DevFlowAgentService.cs` (3,360 lines)

**Virtual Methods**:
```csharp
protected virtual VisualTreeWalker CreateTreeWalker() => new VisualTreeWalker();
protected virtual IProfilerCollector CreateProfilerCollector()
protected virtual string PlatformName
protected virtual string DeviceTypeName
protected virtual string IdiomName
protected virtual double GetWindowDisplayDensity(IWindow? window)
protected virtual (double width, double height) GetNativeWindowSize(IWindow window)
protected virtual async Task<HttpResponse> HandleScreenshot(HttpRequest request)
protected virtual async Task<byte[]?> CaptureScreenshotAsync(VisualElement rootElement)
protected virtual async Task<byte[]?> CaptureElementScreenshotAsync(VisualElement element)
protected virtual Task<byte[]?> CaptureFullScreenAsync()
protected virtual bool TryNativeTap(VisualElement ve)
protected virtual void TryNativeResize(IWindow window, int width, int height)
protected virtual Task<bool> TryNativeScroll(VisualElement element, double deltaX, double deltaY)
```

**Current Implementation**:
- `PlatformAgentService : DevFlowAgentService`

### 3. ElementInfo - DTO (OPEN)

**File**: `/src/MauiDevFlow.Agent.Core/ElementInfo.cs`

**Extensibility**:
- `NativeProperties: Dictionary<string, string?>` - Already used for platform-specific data
- Can add Comet-specific annotations here

### 4. CreateElementInfo Method (CONTROLLED)

**Location**: VisualTreeWalker.cs, lines 1193-1301

**Current Property Extraction**:
- Gesture recognizers → `Gestures[]`
- ItemsView item counts → `NativeProperties["itemCount"]`
- Style classes → `StyleClass[]`

**Hook Point**: Extend `PopulateNativeInfo(ElementInfo info, VisualElement ve)` virtual method

### 5. Synthetic Element Injection (CONTROLLED)

**Location**: VisualTreeWalker.cs, WalkElement() method (lines 326-403)

**Where to Hook**:
- Override `PopulateSyntheticNativeInfo()` for Comet-specific synthetic properties
- Override `ResolveSyntheticBounds()` for Comet-specific positioning
- Extend marker types with Comet-specific wrappers

### 6. CSS Selector Engine (CLOSED)

**File**: `/src/MauiDevFlow.Agent.Core/Css/CssSelectorEngine.cs` (85 lines)

**Current Features**:
- Type selectors (e.g., `Button`)
- Attribute selectors (e.g., `[automationId="MyButton"]`)
- Pseudo-class selectors (`:visible`, `:hidden`, `:enabled`, `:focused`)
- Child, descendant, sibling combinators

**ElementInfoOps** - Implements `IElementOps<ElementInfo>` for Fizzler
- Can extend with Comet-specific pseudo-classes

## Key Files & Line Counts

```
VisualTreeWalker.cs                  1,337 lines  (core walker)
DevFlowAgentService.cs               3,360 lines  (HTTP API)
ElementInfo.cs                         117 lines  (data model)
PlatformVisualTreeWalker.cs           623 lines   (platform-native implementations)
GtkVisualTreeWalker.cs                130 lines   (GTK implementation)
ElementInfoOps.cs                     249 lines   (CSS selector adapter)
CssSelectorEngine.cs                   85 lines   (CSS query engine)
AgentServiceExtensions.cs             252 lines   (DI registration)
```

## Recommended Comet Integration Approach

### Option 1: Lightweight Extension (Recommended for MVP)
```csharp
public class CometAwareVisualTreeWalker : VisualTreeWalker
{
    protected override void PopulateNativeInfo(ElementInfo info, VisualElement ve)
    {
        base.PopulateNativeInfo(info, ve);
        
        // Add Comet-specific properties
        if (ve is Button button && button.Command != null)
            info.NativeProperties ??= new();
            info.NativeProperties["cometCanExecute"] = button.IsEnabled.ToString();
    }
    
    protected override void PopulateSyntheticNativeInfo(ElementInfo info, object marker)
    {
        base.PopulateSyntheticNativeInfo(info, marker);
        
        // Add Comet-specific info to synthetic elements
        if (marker is FlyoutButtonMarker)
            info.NativeProperties?["cometRole"] = "navigation-menu";
    }
}

public class CometAgentService : PlatformAgentService
{
    protected override VisualTreeWalker CreateTreeWalker() 
        => new CometAwareVisualTreeWalker();
}
```

### Option 2: Custom Marker Wrapper
```csharp
// Wrap CometComponents to make them discoverable
public class CometComponentMarker
{
    public object CometComponent { get; init; }  // Comet component
    public VisualElement Host { get; init; }     // Host VisualElement
}
```

### Option 3: Extend ElementInfo with Comet Metadata
- Add `CometSpecific` dictionary to ElementInfo
- Populate during `PopulateNativeInfo()` override
- Return Comet control hierarchy separately

### Option 4: IVisualTreeElement Adapter for Comet
- Wrap Comet components in IVisualTreeElement facade
- Make them appear in the MAUI tree walker natively
- Requires Comet component model cooperation

## Summary: Key Extension Points

| Extension Point | Location | Type | Purpose |
|---|---|---|---|
| `CreateTreeWalker()` | DevFlowAgentService | Virtual Method | Use custom VisualTreeWalker subclass |
| `PopulateNativeInfo()` | VisualTreeWalker | Virtual Method | Add platform-specific properties |
| `PopulateSyntheticNativeInfo()` | VisualTreeWalker | Virtual Method | Extend synthetic element properties |
| `ResolveWindowBounds()` | VisualTreeWalker | Virtual Method | Calculate platform-native coordinates |
| `NativeProperties[]` | ElementInfo | Dictionary | Add arbitrary key-value metadata |
| `WalkElement()` | VisualTreeWalker | Main Algorithm | Modify tree traversal logic |
| `Query()` | VisualTreeWalker | Public Method | Filter elements by properties |
| `QueryCss()` | VisualTreeWalker | Public Method | CSS selector queries |
| Marker Types | VisualTreeWalker | Inner Classes | Extend with CometComponentMarker |

