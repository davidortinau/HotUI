namespace CometTaskApp;

/// <summary>
/// Main app using TabView for multi-page navigation.
/// Exercises: TabView, CometApp, UseCometApp builder pattern.
/// </summary>
public class TaskApp : CometApp
{
	[Body]
	View body() =>
		new TabView
		{
			new TaskListPage().Title("Tasks"),
			new StatsPage().Title("Stats"),
			new SettingsPage().Title("Settings"),
		};
}

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseCometApp<TaskApp>();
		return builder.Build();
	}
}
