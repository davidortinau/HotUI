using CometBaristaNotes.Pages;
using CometBaristaNotes.Components;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.HotReload;
using MauiPage = Microsoft.Maui.Controls.ContentPage;
using MauiShell = Microsoft.Maui.Controls.Shell;

namespace CometBaristaNotes;

/// <summary>
/// Bridges Comet View rebuilds to the MAUI ContentPage container.
/// When Comet state changes trigger body() rebuild, this re-extracts
/// the MauiViewHost content and swaps it into the page.
/// </summary>
class CometPageReloadHandler : IReloadHandler
{
readonly Microsoft.Maui.Controls.ContentView _container;
readonly Comet.View _cometView;

public CometPageReloadHandler(Microsoft.Maui.Controls.ContentView container, Comet.View cometView)
{
_container = container;
_cometView = cometView;
}

public void Reload()
{
try
{
var renderView = _cometView.GetView();
IView viewToRender = (renderView != null && renderView != _cometView) ? renderView : _cometView;

if (viewToRender is Comet.MauiViewHost mvh)
{
var hostedView = mvh.HostedView;
if (hostedView is Microsoft.Maui.Controls.View mauiView)
{
_container.Content = mauiView;
}
}
}
catch (Exception ex)
{
Console.WriteLine($"[CometPageReloadHandler] Reload failed: {ex.Message}");
}
}
}

public class BaristaShell : MauiShell
{
public BaristaShell()
{
// Nav bar matches page background for seamless look (no visible bar boundary)
BackgroundColor = Theme.Background;
MauiShell.SetForegroundColor(this, Theme.TextPrimary);
MauiShell.SetNavBarHasShadow(this, false);
MauiShell.SetTitleColor(this, Theme.TextPrimary);
MauiShell.SetTabBarBackgroundColor(this, Theme.Surface);
MauiShell.SetTabBarForegroundColor(this, Theme.Primary);
MauiShell.SetTabBarTitleColor(this, Theme.Primary);
MauiShell.SetTabBarUnselectedColor(this, Theme.TextSecondary);

var tabBar = new TabBar();

tabBar.Items.Add(new Microsoft.Maui.Controls.ShellContent
{
Title = "New Shot",
Icon = new Microsoft.Maui.Controls.FontImageSource { FontFamily = Icons.FontFamily, Glyph = Icons.Coffee, Size = 32, Color = Theme.TextPrimary },
ContentTemplate = new DataTemplate(() => MakeCometPage(new ShotLoggingPage(), "New Shot")),
Route = "newshot"
});

tabBar.Items.Add(new Microsoft.Maui.Controls.ShellContent
{
Title = "Activity",
Icon = new Microsoft.Maui.Controls.FontImageSource { FontFamily = Icons.FontFamily, Glyph = Icons.Feed, Size = 32, Color = Theme.TextPrimary },
ContentTemplate = new DataTemplate(() => MakeCometPage(new ActivityFeedPage(), "Shot History")),
Route = "activity"
});

tabBar.Items.Add(new Microsoft.Maui.Controls.ShellContent
{
Title = "Settings",
Icon = new Microsoft.Maui.Controls.FontImageSource { FontFamily = Icons.FontFamily, Glyph = Icons.Settings, Size = 32, Color = Theme.TextPrimary },
ContentTemplate = new DataTemplate(() => MakeCometPage(new SettingsPage(), "Settings")),
Route = "settings"
});

Items.Add(tabBar);

Routing.RegisterRoute("bean-detail", typeof(BeanDetailShellPage));
Routing.RegisterRoute("bag-detail", typeof(BagDetailShellPage));
Routing.RegisterRoute("equipment-detail", typeof(EquipmentDetailShellPage));
Routing.RegisterRoute("beans", typeof(BeanManagementShellPage));
Routing.RegisterRoute("equipment", typeof(EquipmentManagementShellPage));
Routing.RegisterRoute("profiles", typeof(UserProfileManagementShellPage));
Routing.RegisterRoute("profile-form", typeof(ProfileFormShellPage));
}

static MauiPage MakeCometPage(Comet.View cometView, string title)
{
var page = new MauiPage
{
Title = title,
BackgroundColor = Theme.Background,
};

page.On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Always);

var container = new Microsoft.Maui.Controls.ContentView
{
BackgroundColor = Theme.Background,
};

page.Content = container;

page.Loaded += (s, e) =>
{
if (page.Handler?.MauiContext == null) return;
EmbedCometView(container, cometView);
cometView.ReloadHandler = new CometPageReloadHandler(container, cometView);
};

MauiShell.SetNavBarIsVisible(page, true);
return page;
}

internal static void EmbedCometView(
Microsoft.Maui.Controls.ContentView container,
Comet.View cometView)
{
try
{
var renderView = cometView.GetView();
IView viewToRender = (renderView != null && renderView != cometView) ? renderView : cometView;

if (viewToRender is Comet.MauiViewHost mvh)
{
var hostedView = mvh.HostedView;
if (hostedView is Microsoft.Maui.Controls.View mauiView)
{
container.Content = mauiView;
return;
}
}

container.Content = new Comet.CometHost(cometView);
}
catch (Exception ex)
{
Console.WriteLine($"[EmbedCometView] Failed: {ex.Message}");
}
}
}

// Shell page wrappers for detail routes
[QueryProperty(nameof(BeanId), "id")]
public class BeanDetailShellPage : MauiPage
{
string _beanId = "";
Microsoft.Maui.Controls.ContentView _container = new();
Comet.View? _cometView;
bool _embedded;

public BeanDetailShellPage() { Content = _container; BackgroundColor = Theme.Background; }

public string BeanId { get => _beanId; set { _beanId = value; LoadPage(); } }

void LoadPage()
{
if (!int.TryParse(_beanId, out var id)) return;
Title = id > 0 ? "Edit Bean" : "New Bean";
_cometView?.Dispose();
_cometView = new BeanDetailPage(id);
_embedded = false;
TryEmbed();
}

void TryEmbed()
{
if (_embedded || _cometView == null || Handler?.MauiContext == null) return;
BaristaShell.EmbedCometView(_container, _cometView);
_cometView.ReloadHandler = new CometPageReloadHandler(_container, _cometView);
_embedded = true;
}

protected override void OnHandlerChanged() { base.OnHandlerChanged(); TryEmbed(); }
}

[QueryProperty(nameof(BagId), "id")]
public class BagDetailShellPage : MauiPage
{
string _bagId = "";
Microsoft.Maui.Controls.ContentView _container = new();
Comet.View? _cometView;
bool _embedded;

public BagDetailShellPage() { Content = _container; BackgroundColor = Theme.Background; }

public string BagId { get => _bagId; set { _bagId = value; LoadPage(); } }

void LoadPage()
{
if (!int.TryParse(_bagId, out var id)) return;
Title = id > 0 ? "Bag Details" : "New Bag";
_cometView?.Dispose();
_cometView = new BagDetailPage(id);
_embedded = false;
TryEmbed();
}

void TryEmbed()
{
if (_embedded || _cometView == null || Handler?.MauiContext == null) return;
BaristaShell.EmbedCometView(_container, _cometView);
_cometView.ReloadHandler = new CometPageReloadHandler(_container, _cometView);
_embedded = true;
}

protected override void OnHandlerChanged() { base.OnHandlerChanged(); TryEmbed(); }
}

[QueryProperty(nameof(EquipmentId), "id")]
public class EquipmentDetailShellPage : MauiPage
{
string _equipmentId = "";
Microsoft.Maui.Controls.ContentView _container = new();
Comet.View? _cometView;
bool _embedded;

public EquipmentDetailShellPage() { Content = _container; BackgroundColor = Theme.Background; }

public string EquipmentId { get => _equipmentId; set { _equipmentId = value; LoadPage(); } }

void LoadPage()
{
if (!int.TryParse(_equipmentId, out var id)) return;
Title = id > 0 ? "Edit Equipment" : "New Equipment";
_cometView?.Dispose();
_cometView = new EquipmentDetailPage(id);
_embedded = false;
TryEmbed();
}

void TryEmbed()
{
if (_embedded || _cometView == null || Handler?.MauiContext == null) return;
BaristaShell.EmbedCometView(_container, _cometView);
_cometView.ReloadHandler = new CometPageReloadHandler(_container, _cometView);
_embedded = true;
}

protected override void OnHandlerChanged() { base.OnHandlerChanged(); TryEmbed(); }
}

public class BeanManagementShellPage : MauiPage
{
Microsoft.Maui.Controls.ContentView _container = new();
Comet.View _cometView = new BeanManagementPage();
bool _embedded;

public BeanManagementShellPage() { Title = "Beans"; Content = _container; BackgroundColor = Theme.Background; }

void TryEmbed()
{
if (_embedded || Handler?.MauiContext == null) return;
BaristaShell.EmbedCometView(_container, _cometView);
_cometView.ReloadHandler = new CometPageReloadHandler(_container, _cometView);
_embedded = true;
}

protected override void OnHandlerChanged() { base.OnHandlerChanged(); TryEmbed(); }
}

public class EquipmentManagementShellPage : MauiPage
{
Microsoft.Maui.Controls.ContentView _container = new();
Comet.View _cometView = new EquipmentManagementPage();
bool _embedded;

public EquipmentManagementShellPage() { Title = "Equipment"; Content = _container; BackgroundColor = Theme.Background; }

void TryEmbed()
{
if (_embedded || Handler?.MauiContext == null) return;
BaristaShell.EmbedCometView(_container, _cometView);
_cometView.ReloadHandler = new CometPageReloadHandler(_container, _cometView);
_embedded = true;
}

protected override void OnHandlerChanged() { base.OnHandlerChanged(); TryEmbed(); }
}

public class UserProfileManagementShellPage : MauiPage
{
Microsoft.Maui.Controls.ContentView _container = new();
Comet.View _cometView = new UserProfileManagementPage();
bool _embedded;

public UserProfileManagementShellPage() { Title = "User Profiles"; Content = _container; BackgroundColor = Theme.Background; }

void TryEmbed()
{
if (_embedded || Handler?.MauiContext == null) return;
BaristaShell.EmbedCometView(_container, _cometView);
_cometView.ReloadHandler = new CometPageReloadHandler(_container, _cometView);
_embedded = true;
}

protected override void OnHandlerChanged() { base.OnHandlerChanged(); TryEmbed(); }
}

[QueryProperty(nameof(ProfileId), "id")]
public class ProfileFormShellPage : MauiPage
{
string _profileId = "";
Microsoft.Maui.Controls.ContentView _container = new();
Comet.View? _cometView;
bool _embedded;

public ProfileFormShellPage() { Content = _container; BackgroundColor = Theme.Background; }

public string ProfileId { get => _profileId; set { _profileId = value; LoadPage(); } }

void LoadPage()
{
int.TryParse(_profileId, out var id);
Title = id > 0 ? "Edit Profile" : "New Profile";
_cometView?.Dispose();
_cometView = new ProfileFormPage(id);
_embedded = false;
TryEmbed();
}

void TryEmbed()
{
if (_embedded || _cometView == null || Handler?.MauiContext == null) return;
BaristaShell.EmbedCometView(_container, _cometView);
_cometView.ReloadHandler = new CometPageReloadHandler(_container, _cometView);
_embedded = true;
}

protected override void OnHandlerChanged() { base.OnHandlerChanged(); TryEmbed(); }
}
