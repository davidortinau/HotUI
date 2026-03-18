# MauiDevFlow Architecture Analysis - Executive Summary

## Overview
MauiDevFlow is a MAUI testing/debugging agent that walks the visual tree and exposes it via HTTP REST API. It uses a hierarchical tree walker pattern with support for synthetic elements (tabs, navbars, buttons) that don't exist in the MAUI visual tree but are present in the UI.

## Architecture

### 1. Core Components

**VisualTreeWalker** (1,337 lines - `/src/MauiDevFlow.Agent.Core/VisualTreeWalker.cs`)
- Primary responsibility: Walk the visual tree and generate ElementInfo representations
- Core method: `WalkTree(Application app)` → returns tree with full hierarchy
- Uses: `IVisualTreeElement.GetVisualChildren()` for traversal
- Generates stable IDs from Element.Id and AutomationId
- Handles synthetic elements (NavBars, Tabs, Buttons, etc.)

**ElementInfo** (117 lines - `/src/MauiDevFlow.Agent.Core/ElementInfo.cs`)
- DTO representing a visual element
- Contains: ID, type, text, value, bounds, gestures, style classes, native properties
- Recursive: Children[] for hierarchy

**DevFlowAgentService** (3,360 lines)
- HTTP API server providing REST endpoints
- Coordinates tree walking, element interaction, profiling, screenshot capture
- Creates tree walker via virtual method `CreateTreeWalker()`

**PlatformVisualTreeWalker** (623 lines - platform-specific implementations)
- Extends VisualTreeWalker for iOS/Android/Windows/macOS
- Overrides platform-specific methods:
  - `PopulateNativeInfo()` - Extracts native view properties
  - `ResolveWindowBounds()` - Calculates platform-native coordinates
  - `ResolveSyntheticBounds()` - Platform-specific synthetic element positioning

### 2. MAUI Interface Dependencies

| Interface/Type | Purpose | How Used |
|---|---|---|
| `IVisualTreeElement` | Tree traversal API | `GetVisualChildren()` for enumerating nodes |
| `Element` | Base element type | Source of stable ID (Element.Id: Guid) |
| `VisualElement : Element` | Visible elements | Frame, opacity, visibility, focus, enabled state |
| `Page` | Pages/screens | ToolbarItems, SearchHandler, NavigationBar title |
| `Shell` | Navigation shell | Flyout items, tabs, flyout button |
| `NavigationPage` | Navigation container | Back button, navigation bar |
| `Application` | App root | Windows collection |
| `INavigation` | Navigation stack | Back button detection |

### 3. Node Representation

**What gets extracted per element:**
- **Identity**: ID (from AutomationId or Element.Id), parent ID, type name (short and full)
- **Content**: Text (Label.Text, Button.Text, etc.), Value (Switch.IsToggled, Slider.Value)
- **State**: IsVisible, IsEnabled, IsFocused, Opacity
- **Position**: Bounds (logical frame), WindowBounds (platform-native absolute)
- **Interaction**: Gestures (tap, swipe, pan), StyleClass (CSS classes)
- **Platform**: NativeType (handler's platform view type), NativeProperties (custom key-value dict)

**Type name resolution:**
- Short: `element.GetType().Name` (e.g., "Button")
- Full: `element.GetType().FullName` (e.g., "Microsoft.Maui.Controls.Button")

### 4. Synthetic Elements

Eight marker types represent non-visual-tree elements:
- **NavBarTitleMarker** - Navigation bar title (Shell/NavigationPage)
- **BackButtonMarker** - Back button in navigation stack
- **SearchHandlerMarker** - Shell search box
- **FlyoutButtonMarker** - Shell flyout menu button (☰)
- **ShellFlyoutItemMarker** - Items in flyout menu
- **ShellTabMarker** - Bottom tab bar items
- **FlyoutToggleMarker** - FlyoutPage toggle button
- **TabbedPageTabMarker** - Tab bar in TabbedPage

These are injected during `WalkElement()` and given synthetic IDs, bounds, and properties.

## Extension Points for Comet Integration

### **Option 1: Subclass VisualTreeWalker (RECOMMENDED)**

```csharp
public class CometAwareVisualTreeWalker : VisualTreeWalker
{
    protected override void PopulateNativeInfo(ElementInfo info, VisualElement ve)
    {
        base.PopulateNativeInfo(info, ve);
        
        if (ve is CometButton btn)
            info.NativeProperties?["cometControl"] = "Button";
        if (ve is CometDataForm form)
            info.NativeProperties?["cometControl"] = "DataForm";
    }
}

public class CometAgentService : PlatformAgentService
{
    protected override VisualTreeWalker CreateTreeWalker() 
        => new CometAwareVisualTreeWalker();
}
```

**Pros:**
- Minimal code changes
- Leverages existing tree walker
- Easy to detect Comet controls via NativeProperties

**Cons:**
- Limited to enriching existing elements
- Can't represent Comet-only logic

### **Option 2: Create Comet Marker Types**

```csharp
public class CometComponentMarker
{
    public object CometComponent { get; init; }
    public VisualElement Host { get; init; }
    public string Role { get; init; }
}

// In WalkElement() override, inject:
if (vm?.CurrentPopup != null)
{
    var marker = new CometComponentMarker 
    { 
        CometComponent = vm.CurrentPopup, 
        Host = element 
    };
    // Add to children as synthetic element
}
```

**Pros:**
- Represents Comet-specific UI elements
- Parallel tree structure

**Cons:**
- Need custom marker classes
- Custom ID generation, bounds resolution

### **Option 3: Extend ElementInfo with Comet Metadata**

Add `CometSpecific` property to ElementInfo:
```csharp
public Dictionary<string, object> CometMetadata { get; set; }
```

**Pros:**
- Minimal structure change
- Flexible property storage

**Cons:**
- Still limited to MAUI visual tree

### **Option 4: IVisualTreeElement Adapter (Advanced)**

```csharp
public class CometComponentAdapter : IVisualTreeElement
{
    public IEnumerable<IVisualTreeElement> GetVisualChildren()
    {
        // Expose Comet component hierarchy as MAUI tree
    }
}

// Wrap Comet components during app initialization
```

**Pros:**
- Fully native integration
- Works with existing tree walker unchanged
- Comet components appear as regular MAUI elements

**Cons:**
- Requires Comet component model changes
- More complex implementation

## Project Structure

```
MauiDevFlow.Agent.Core (NuGet package)
  ├─ VisualTreeWalker.cs              (1,337 lines) [CORE]
  ├─ ElementInfo.cs                   (117 lines)
  ├─ DevFlowAgentService.cs           (3,360 lines) [HTTP API]
  ├─ Css/
  │  ├─ CssSelectorEngine.cs
  │  └─ ElementInfoOps.cs
  └─ Network, Profiling, Logging

MauiDevFlow.Agent (Platform implementations)
  ├─ PlatformAgentService             [iOS/Android/Windows/macOS]
  ├─ PlatformVisualTreeWalker         (623 lines)
  └─ AgentServiceExtensions           [DI Registration]

MauiDevFlow.Agent.Gtk (GTK/Linux)
  └─ GtkVisualTreeWalker              (130 lines)
```

## Virtual Methods for Extension

**VisualTreeWalker:**
- `PopulateNativeInfo(ElementInfo info, VisualElement ve)` - Add native view properties
- `PopulateSyntheticNativeInfo(ElementInfo info, object marker)` - Enrich synthetic elements
- `ResolveWindowBounds(VisualElement ve)` - Platform-specific bounds
- `ResolveSyntheticBounds(object marker)` - Synthetic element positioning
- `EnsurePlatformStableId(object platformObj)` - Stable ID stamping

**DevFlowAgentService:**
- `CreateTreeWalker()` - **← KEY EXTENSION POINT FOR COMET**
- `CreateProfilerCollector()`
- `TryNativeTap(VisualElement)`, `TryNativeScroll()`, `CaptureScreenshotAsync()`

## CSS Selector Support

**Selectors:** Type (`Button`), Attribute (`[text="Hi"]`), Class (`.myClass`), Pseudo-class (`:visible`, `:enabled`), Combinators (`>`, ` `, `+`, `~`)

**Custom MAUI pseudo-classes:** `[__maui-visible]`, `[__maui-hidden]`, `[__maui-enabled]`, `[__maui-focused]`

**Extension:** ElementInfoOps.GetAttribute() can map Comet-specific selectors

## HTTP API Endpoints

- `GET /api/tree` - Full visual tree
- `GET /api/query?type=Label&automationId=...&text=...` - Query by properties
- `GET /api/query?selector=Button[automationId]` - CSS selector query
- `GET /api/element?id=...` - Get single element with children
- `POST /api/tap` - Tap element
- `POST /api/screenshot` - Capture screen

## Key Takeaway

MauiDevFlow is **extensible by design**. The recommended approach for Comet integration is:

1. **Subclass VisualTreeWalker** to detect and annotate Comet controls
2. **Store metadata in ElementInfo.NativeProperties** for Comet-specific info
3. **Subclass PlatformAgentService** and override `CreateTreeWalker()`
4. **Register custom agent in DI** instead of default PlatformAgentService

This requires ~100-200 lines of code and integrates seamlessly with the existing HTTP API.

## Files to Focus On

**For Comet integration:**
1. `/src/MauiDevFlow.Agent.Core/VisualTreeWalker.cs` - Understand tree walk algorithm
2. `/src/MauiDevFlow.Agent.Core/ElementInfo.cs` - Know the node structure
3. `/src/MauiDevFlow.Agent/DevFlowAgentService.cs` - Service lifecycle
4. `/src/MauiDevFlow.Agent/AgentServiceExtensions.cs` - DI registration pattern

**For advanced:**
1. `/src/MauiDevFlow.Agent.Core/Css/ElementInfoOps.cs` - CSS selector adapter
2. `/src/MauiDevFlow.Agent/VisualTreeWalker.cs` - Platform-specific implementation pattern
3. `/src/MauiDevFlow.Agent.Gtk/GtkVisualTreeWalker.cs` - GTK example

