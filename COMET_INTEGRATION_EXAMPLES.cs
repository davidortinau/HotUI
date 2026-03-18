// ═══════════════════════════════════════════════════════════════════════════════
// COMET INTEGRATION EXAMPLES FOR MAUIDEVFLOW
// ═══════════════════════════════════════════════════════════════════════════════

// EXAMPLE 1: LIGHTWEIGHT EXTENSION - CREATE CUSTOM WALKER
// ─────────────────────────────────────────────────────

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using MauiDevFlow.Agent.Core;

namespace MyApp.DevTools;

/// <summary>
/// Comet-aware visual tree walker that adds Comet-specific metadata to elements.
/// This is the RECOMMENDED approach for minimal integration.
/// </summary>
public class CometAwareVisualTreeWalker : VisualTreeWalker
{
    protected override void PopulateNativeInfo(ElementInfo info, VisualElement ve)
    {
        base.PopulateNativeInfo(info, ve);
        
        // Detect Comet controls and add metadata
        info.NativeProperties ??= new Dictionary<string, string?>();
        
        // Example 1: Mark Comet buttons
        if (ve is CometButton cometBtn)
        {
            info.NativeProperties["cometControl"] = "Button";
            info.NativeProperties["cometCommand"] = cometBtn.Command?.GetType().Name ?? "None";
            info.NativeProperties["cometCanExecute"] = cometBtn.IsEnabled.ToString();
        }
        
        // Example 2: Mark Comet data forms
        if (ve is CometDataForm form)
        {
            info.NativeProperties["cometControl"] = "DataForm";
            info.NativeProperties["cometDataType"] = form.BindingContext?.GetType().Name ?? "Unknown";
        }
        
        // Example 3: Mark Comet popups
        if (ve is CometPopup popup)
        {
            info.NativeProperties["cometControl"] = "Popup";
            info.NativeProperties["cometPopupType"] = popup.Content?.GetType().Name ?? "Unknown";
        }
        
        // Example 4: Capture Comet-specific state
        if (ve is CometControl comet)
        {
            info.NativeProperties["cometRole"] = GetCometRole(comet);
            info.NativeProperties["cometState"] = GetCometState(comet);
            info.NativeProperties["cometHasBinding"] = (comet.BindingContext != null).ToString();
        }
    }
    
    protected override void PopulateSyntheticNativeInfo(ElementInfo info, object marker)
    {
        base.PopulateSyntheticNativeInfo(info, marker);
        
        // Example: Mark synthetic elements as Comet-aware
        info.NativeProperties ??= new Dictionary<string, string?>();
        info.NativeProperties["isSynthetic"] = "true";
        
        // Add Comet context to synthetics
        if (marker is FlyoutButtonMarker flyout)
        {
            info.NativeProperties["cometNavigationStyle"] = "MauiShell";
        }
    }
    
    private static string? GetCometRole(CometControl control)
    {
        return control switch
        {
            CometButton => "button",
            CometEntry => "textinput",
            CometLabel => "label",
            CometCheckBox => "checkbox",
            CometSwitch => "toggle",
            CometDataForm => "form",
            CometPopup => "overlay",
            _ => control.GetType().Name.Replace("Comet", "").ToLowerInvariant()
        };
    }
    
    private static string? GetCometState(CometControl control)
    {
        // Return control-specific state
        return control switch
        {
            CometButton btn => $"enabled:{btn.IsEnabled}",
            CometEntry entry => $"empty:{string.IsNullOrEmpty(entry.Text)}",
            CometCheckBox cb => $"checked:{cb.IsChecked}",
            _ => "unknown"
        };
    }
}


// EXAMPLE 2: CUSTOM AGENT SERVICE
// ──────────────────────────────

using MauiDevFlow.Agent;
using MauiDevFlow.Agent.Core;

namespace MyApp.DevTools;

/// <summary>
/// Comet-aware agent service that uses CometAwareVisualTreeWalker.
/// Register this in DI instead of PlatformAgentService.
/// </summary>
public class CometAgentService : PlatformAgentService
{
    public CometAgentService(AgentOptions? options = null) 
        : base(options) { }

    protected override VisualTreeWalker CreateTreeWalker() 
        => new CometAwareVisualTreeWalker();
}


// EXAMPLE 3: DI REGISTRATION
// ──────────────────────────

public static class CometDevFlowExtensions
{
    /// <summary>
    /// Adds Comet-aware MauiDevFlow Agent to the app.
    /// Usage in MauiProgram.cs:
    ///   .AddCometDevFlowAgent(options => options.EnableNetworkMonitoring = true)
    /// </summary>
    public static MauiAppBuilder AddCometDevFlowAgent(
        this MauiAppBuilder builder,
        Action<AgentOptions>? configure = null)
    {
        var options = new AgentOptions();
        configure?.Invoke(options);

        // Use Comet-aware service instead of default
        var service = new CometAgentService(options);

        builder.Services.AddSingleton<DevFlowAgentService>(service);

        // ... rest of registration (copy from AgentServiceExtensions) ...
        
        return builder;
    }
}


// EXAMPLE 4: SYNTHETIC MARKER FOR COMET
// ────────────────────────────────────

namespace MauiDevFlow.Agent.Core;

/// <summary>
/// Marker class for Comet popup/overlay components that aren't in visual tree.
/// Inject this during tree walk to make Comet popups discoverable.
/// </summary>
public class CometPopupMarker
{
    public CometPopup Popup { get; init; } = null!;
    public VisualElement Host { get; init; } = null!;  // Parent element
    public string Role { get; init; } = "popup";
}

public class CometDataGridMarker
{
    public CometDataGrid Grid { get; init; } = null!;
    public VisualElement Host { get; init; } = null!;
    public int ItemCount { get; init; }
}

// Usage in extended WalkElement():
/*
if (element is VisualElement ve && ve.BindingContext is CometViewModel vm)
{
    // Check if ViewModel has Comet popups
    if (vm.CurrentPopup != null)
    {
        var popupMarker = new CometPopupMarker 
        { 
            Popup = vm.CurrentPopup, 
            Host = ve 
        };
        var popupId = GenerateObjectId(popupMarker, "CometPopup");
        info.Children ??= new List<ElementInfo>();
        info.Children.Add(CreateCometPopupInfo(popupMarker, popupId, id));
    }
}
*/


// EXAMPLE 5: QUERY COMET CONTROLS
// ────────────────────────────────

// Usage in client (automated testing):
/*
// Find all Comet buttons
var buttons = walker.Query(app, type: "CometButton");

// Find Comet controls with command binding
var css = await client.GetAsync("/api/query?selector=Button[cometCommand]");

// Find Comet popups
var popups = walker.QueryCss(app, "CometPopup");

// Hit test for Comet control at coordinates
var elements = walker.HitTestByBounds(x: 100, y: 200, app);
foreach (var el in elements)
{
    if (el.NativeProperties?["cometControl"] != null)
    {
        // Found Comet control
        var cometType = el.NativeProperties["cometControl"];
    }
}
*/


// EXAMPLE 6: EXTEND QUERY WITH COMET SELECTORS
// ────────────────────────────────────────────

// In ElementInfoOps.GetAttribute() virtual method:
/*
protected override string? GetAttribute(ElementInfo el, string name)
{
    // Add Comet-specific attributes
    if (name.StartsWith("comet-", StringComparison.OrdinalIgnoreCase))
    {
        return name.ToLowerInvariant() switch
        {
            "comet-button" => el.NativeProperties?["cometControl"] == "Button" ? "true" : null,
            "comet-form" => el.NativeProperties?["cometControl"] == "DataForm" ? "true" : null,
            "comet-bound" => el.NativeProperties?["cometHasBinding"] == "true" ? "true" : null,
            "comet-enabled" => el.NativeProperties?["cometCanExecute"] == "true" ? "true" : null,
            _ => null
        };
    }
    return base.GetAttribute(el, name);
}

// CSS selectors for testing:
// [comet-button]                    - All Comet buttons
// [comet-form][comet-bound]         - Bound Comet forms
// Button[cometCommand]              - MAUI buttons + Comet metadata
// CometButton[cometCanExecute="true"]
*/


// EXAMPLE 7: ADVANCED - DETECT COMET BINDINGS
// ──────────────────────────────────────────

public class CometBindingDetector : VisualTreeWalker
{
    protected override void PopulateNativeInfo(ElementInfo info, VisualElement ve)
    {
        base.PopulateNativeInfo(info, ve);
        
        info.NativeProperties ??= new Dictionary<string, string?>();
        
        // Detect MVVM/binding patterns
        if (ve is Entry entry && entry.BindingContext != null)
        {
            var binding = entry.GetBinding(Entry.TextProperty);
            if (binding != null)
            {
                info.NativeProperties["bindingPath"] = binding.Path;
                info.NativeProperties["bindingMode"] = binding.Mode.ToString();
            }
        }
        
        // Detect Comet commands
        if (ve is Button btn && btn.Command != null)
        {
            info.NativeProperties["commandType"] = btn.Command.GetType().Name;
            info.NativeProperties["commandParameter"] = btn.CommandParameter?.ToString() ?? "null";
        }
        
        // Mark controls with binding context
        if (ve.BindingContext != null)
        {
            info.NativeProperties["hasBoundData"] = "true";
            info.NativeProperties["boundDataType"] = ve.BindingContext.GetType().Name;
        }
    }
}


// EXAMPLE 8: HTTP API USAGE (Client-side)
// ───────────────────────────────────────

/*
// Get full tree with Comet metadata
GET http://localhost:8000/api/tree
Response: ElementInfo[] with NativeProperties["cometControl"]

// Query Comet buttons
GET http://localhost:8000/api/query?type=CometButton
Response: Flat list of CometButton elements

// Query with CSS
GET http://localhost:8000/api/query?selector=[cometControl]
Response: All elements with cometControl property

// Get single element
GET http://localhost:8000/api/element?id=MyButton_12345678
Response: ElementInfo with children, NativeProperties

// Tap a Comet button
POST http://localhost:8000/api/tap
Body: { "elementId": "MyButton_12345678" }

// Get bounds for positioning
GET http://localhost:8000/api/element?id=MyPopup_87654321
Response: ElementInfo with Bounds/WindowBounds
*/

