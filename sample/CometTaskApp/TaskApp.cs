namespace CometTaskApp;

/// <summary>
/// Main app using TabView for multi-page navigation.
/// Exercises: TabView, CometApp, UseCometApp builder pattern.
/// </summary>
public class TaskApp : CometApp
{
	public TaskApp()
	{
		Body = CreateRootView;
	}

	public static View CreateRootView() =>
		TabView(
			("Tasks", new TaskListPage()),
			("Stats", new StatsPage()),
			("Settings", new SettingsPage())
		);
}

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

#if DEBUG
		builder.UseCometSampleDebugHost(TaskApp.CreateRootView);
#else
		builder.UseCometApp<TaskApp>();
#endif

#if DEBUG
		builder.EnableSampleRuntimeDebugging();
#endif
		return builder.Build();
	}
}
