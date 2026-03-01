using CometProjectManager.Pages;

namespace CometProjectManager;

/// <summary>
/// App entry point — matches the template's AppShell with flyout navigation.
/// Uses TabView with Dashboard, Projects, and Manage Meta tabs (matching Shell tabs).
/// Title "Categories and Tags" matches the template's ManageMetaPage title.
/// </summary>
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
		builder.UseCometApp<ProjectManagerApp>();
		return builder.Build();
	}
}
