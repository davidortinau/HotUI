using CometProjectManager.Pages;
using Syncfusion.Maui.Toolkit.Hosting;

namespace CometProjectManager;

public class ProjectManagerApp : CometApp
{
[Body]
View body() =>
new TabView
{
new DashboardPage().Title("Dashboard"),
new ProjectListPage().Title("Projects"),
new ManageMetaPage().Title("Categories and Tags"),
};
}

public static class MauiProgram
{
public static MauiApp CreateMauiApp()
{
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
