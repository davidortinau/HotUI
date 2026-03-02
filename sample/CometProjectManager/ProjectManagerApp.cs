using CometProjectManager.Pages;
using Syncfusion.Maui.Toolkit.Hosting;

using MauiGrid = Microsoft.Maui.Controls.Grid;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiImage = Microsoft.Maui.Controls.Image;
using FontImageSource = Microsoft.Maui.Controls.FontImageSource;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;

namespace CometProjectManager;

public class ProjectManagerApp : CometApp
{
// Support launch argument --page=X for screenshot testing
static string? _forcePage = null;

public static void SetForcePage(string page) => _forcePage = page;

[Body]
View body()
{
if (_forcePage != null)
{
// Single-page mode for snapshot testing
var store = DataStore.Instance;
var firstProject = store.Projects.Value?.FirstOrDefault();
var firstTask = store.AllTasks.Value?.FirstOrDefault();
return _forcePage switch
{
"dashboard" => new DashboardPage(),
"projects" => new ProjectListPage(),
"manage" => new ManageMetaPage(),
"projectdetail" => new ProjectDetailPage(firstProject ?? new CometProjectManager.Models.Project()),
"taskdetail" => new TaskDetailPage(firstTask, firstTask?.ProjectID ?? 1),
_ => new DashboardPage(),
};
}

// Default: show dashboard (matching MAUI Shell initial page)
return new DashboardPage();
}
}

public static class MauiProgram
{
public static MauiApp CreateMauiApp()
{
// Check for --page= launch argument
var args = System.Environment.GetCommandLineArgs();
foreach (var arg in args)
{
if (arg.StartsWith("--page=", System.StringComparison.OrdinalIgnoreCase))
{
ProjectManagerApp.SetForcePage(arg.Substring(7).ToLowerInvariant());
}
}

var builder = MauiApp.CreateBuilder();
builder.UseCometApp<ProjectManagerApp>()
.ConfigureSyncfusionToolkit()
.ConfigureFonts(fonts =>
{
fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
fonts.AddFont("FluentSystemIcons-Regular.ttf", Fonts.FluentUI.FontFamily);
});
return builder.Build();
}
}
