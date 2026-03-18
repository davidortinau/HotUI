# MauiDevFlow Quick Reference for Comet Integration

## What is MauiDevFlow?
HTTP-based visual tree inspector for MAUI apps. Walks the visual tree and exposes elements via REST API. Can be extended to detect and annotate Comet controls.

## Core Architecture (Simple Version)

```
Application
    └─ Walk via IVisualTreeElement.GetVisualChildren()
        ├─ Generate ElementInfo (ID, type, text, bounds, etc.)
        ├─ Add synthetic elements (tabs, navbars, buttons)
        └─ Return hierarchical tree
            └─ Expose via HTTP REST API
```

## Key Classes & Files

| Class | File | Lines | Purpose |
|-------|------|-------|---------|
| **VisualTreeWalker** | Agent.Core/VisualTreeWalker.cs | 1,337 | Core tree walker |
| **ElementInfo** | Agent.Core/ElementInfo.cs | 117 | Node representation |
| **DevFlowAgentService** | Agent.Core/DevFlowAgentService.cs | 3,360 | HTTP API |
| **PlatformVisualTreeWalker** | Agent/VisualTreeWalker.cs | 623 | Platform-specific impl |
| **PlatformAgentService** | Agent/DevFlowAgentService.cs | ~50 | Platform service |

## MAUI Interfaces Used

- **IVisualTreeElement** → GetVisualChildren() for tree traversal
- **Element** → Element.Id (Guid) for stable IDs
- **VisualElement** → Bounds, opacity, visibility, focus
- **Page** → ToolbarItems, nav bar
- **Shell** → Flyout, tabs
- **Application** → Windows (root)

## Node Properties Extracted

```csharp
ElementInfo
├─ Identity: Id, ParentId, Type, FullType, AutomationId
├─ Content: Text, Value
├─ State: IsVisible, IsEnabled, IsFocused, Opacity
├─ Position: Bounds, WindowBounds
├─ Interaction: Gestures[], StyleClass[]
├─ Platform: NativeType, NativeProperties{}
└─ Children: ElementInfo[]
```

## Synthetic Elements (8 types)

Non-visual-tree elements injected by tree walker:

| Type | Purpose | Example |
|------|---------|---------|
| NavBarTitleMarker | Navigation bar titles | "My Page" title in Shell |
| BackButtonMarker | Back button in nav stack | "← Previous" button |
| SearchHandlerMarker | Shell search box | Search bar in Shell |
| FlyoutButtonMarker | Shell flyout menu button | ☰ hamburger menu |
| ShellFlyoutItemMarker | Flyout menu items | "Home", "Settings" in menu |
| ShellTabMarker | Bottom tab bar items | Tabs in Shell |
| FlyoutToggleMarker | FlyoutPage toggle | Menu toggle button |
| TabbedPageTabMarker | TabbedPage tabs | Tabs in TabbedPage |

Each has a marker class wrapping the MAUI object (Page, Shell, etc.).

## How to Extend for Comet

### Approach 1: Subclass VisualTreeWalker (Recommended ✓)

```csharp
public class CometAwareVisualTreeWalker : VisualTreeWalker
{
    protected override void PopulateNativeInfo(ElementInfo info, VisualElement ve)
    {
        base.PopulateNativeInfo(info, ve);
        
        // Detect Comet controls
        if (ve is CometButton)
            info.NativeProperties ??= new();
            info.NativeProperties["cometControl"] = "Button";
        
        if (ve is CometDataForm)
            info.NativeProperties["cometControl"] = "DataForm";
    }
}

// Use it:
public class CometAgentService : PlatformAgentService
{
    protected override VisualTreeWalker CreateTreeWalker() 
        => new CometAwareVisualTreeWalker();
}
```

**Cost:** ~50-100 lines  
**Time:** < 1 hour  
**Integration:** Easy - just register in DI instead of default

### Approach 2: Create Comet Marker Types

Create `CometComponentMarker` to represent Comet-specific UI elements not in MAUI tree.

```csharp
public class CometComponentMarker
{
    public object CometComponent { get; init; }
    public VisualElement Host { get; init; }
}
```

Inject during tree walk if needed.

### Approach 3: Extend ElementInfo

Add `Dictionary<string, object> CometMetadata` to store Comet-specific data.

## Virtual Methods to Override

**VisualTreeWalker:**
```csharp
protected override void PopulateNativeInfo(ElementInfo info, VisualElement ve)
    // Add properties to NativeProperties{}

protected override void PopulateSyntheticNativeInfo(ElementInfo info, object marker)
    // Enrich synthetic elements

protected override BoundsInfo? ResolveWindowBounds(VisualElement ve)
    // Platform-native coordinates

protected override BoundsInfo? ResolveSyntheticBounds(object marker)
    // Synthetic element positioning

protected override string? EnsurePlatformStableId(object platformObj)
    // Stable ID generation
```

**DevFlowAgentService:**
```csharp
protected override VisualTreeWalker CreateTreeWalker()
    // ← KEY METHOD: return custom walker
```

## HTTP API (What Clients See)

```bash
# Get full tree
GET http://localhost:8000/api/tree

# Query by type
GET http://localhost:8000/api/query?type=CometButton

# CSS selectors
GET http://localhost:8000/api/query?selector=Button[cometControl]

# Get single element
GET http://localhost:8000/api/element?id=MyButton_xyz

# Interact
POST http://localhost:8000/api/tap
Body: { "elementId": "MyButton_xyz" }
```

## ElementInfo.NativeProperties Usage

```csharp
// In PopulateNativeInfo override, populate NativeProperties{}
info.NativeProperties = new()
{
    ["cometControl"] = "Button",
    ["cometCommand"] = "MyCommand",
    ["cometRole"] = "action",
    ["cometCanExecute"] = "true"
};

// Client queries:
// GET /api/query?selector=[cometControl]
// Returns all elements with cometControl property
```

## ID Generation Strategy

Priority order:
1. **AutomationId** (if set on VisualElement)
2. **Element.Id** (first 12 chars of Guid)
3. **Platform stamp** (EnsurePlatformStableId)
4. **Hash** (fallback)

Handles collisions by suffixing with platform ID or Guid.

## CSS Selectors Supported

```css
/* Type */
Button, Label, Entry

/* AutomationId */
#myButton [automationId="save"]

/* Attribute */
[text="Click me"] [type="Button"]

/* Class */
.errorStyle

/* Pseudo-class */
:visible :enabled :focused

/* Custom MAUI */
[__maui-visible] [__maui-focused]

/* Combinators */
Page > ScrollView   /* child */
Page ScrollView     /* descendant */
Button + Label      /* adjacent */
Button ~ Label      /* sibling */
```

## Recommended Integration Steps

1. **Study**: Read VisualTreeWalker.cs (1,337 lines) to understand tree walk
2. **Create**: Make `CometAwareVisualTreeWalker : VisualTreeWalker`
3. **Override**: `PopulateNativeInfo()` to detect Comet controls
4. **Store**: Put Comet metadata in `NativeProperties{}`
5. **Service**: Create `CometAgentService : PlatformAgentService`
6. **Register**: Use in DI instead of default `PlatformAgentService`
7. **Test**: Query via REST API, see Comet controls annotated

## Files to Read (Priority Order)

1. **VisualTreeWalker.cs** (1,337 lines) - Understand core algorithm
2. **ElementInfo.cs** (117 lines) - Know the node structure
3. **PlatformVisualTreeWalker.cs** (623 lines) - See pattern for platform-specific code
4. **DevFlowAgentService.cs** (~50 lines in Agent/) - See how walker is created
5. **AgentServiceExtensions.cs** (252 lines) - DI registration pattern

## Test It

```bash
# Start app with agent
dotnet run

# Get tree
curl http://localhost:8000/api/tree | jq

# Query Comet controls (after implementing)
curl 'http://localhost:8000/api/query?selector=[cometControl]' | jq

# Get single element
curl 'http://localhost:8000/api/element?id=MyButton_xyz' | jq
```

## Effort Estimate

| Task | Time |
|------|------|
| Study VisualTreeWalker.cs | 1-2 hours |
| Create CometAwareVisualTreeWalker | 30 min |
| Override PopulateNativeInfo() | 30 min |
| Create CometAgentService | 15 min |
| Test with curl/Postman | 30 min |
| **Total** | **~3-4 hours** |

## Key Takeaway

MauiDevFlow is **designed for extension**. The walker is abstract with virtual methods and uses DI. To add Comet awareness:

1. Subclass `VisualTreeWalker` (lightweight)
2. Override `PopulateNativeInfo()` (detect Comet)
3. Populate `NativeProperties{}` (metadata)
4. Subclass `PlatformAgentService` (create walker)
5. Register in DI (use custom service)

**~100 lines of code, no core changes needed.**

