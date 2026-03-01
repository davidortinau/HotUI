using CometProjectManager.Pages;

namespace CometProjectManager;

/// <summary>
/// App entry point — equivalent to the template's AppShell with flyout.
/// Uses TabView with Dashboard, Projects, and Manage Meta tabs.
/// </summary>
public class ProjectManagerApp : CometApp
{
	[Body]
	View body() =>
		new TabView
		{
			new DashboardPage().Title("Dashboard"),
			new ProjectListPage().Title("Projects"),
			new ManageMetaPage().Title("Manage Meta"),
		};
}

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseCometApp<ProjectManagerApp>();
		return builder.Build();
	}
}
