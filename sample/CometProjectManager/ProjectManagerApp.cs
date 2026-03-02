using CometProjectManager.Pages;
using Syncfusion.Maui.Toolkit.Hosting;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using MauiPage = Microsoft.Maui.Controls.ContentPage;
using MauiShell = Microsoft.Maui.Controls.Shell;

namespace CometProjectManager;

/// <summary>
/// CometApp used only for snapshot/force-page testing mode.
/// Normal launch uses ShellMauiApp with real MAUI Shell navigation.
/// </summary>
public class ProjectManagerApp : CometApp
{
	static string? _forcePage = null;
	public static void SetForcePage(string page) => _forcePage = page;
	public static string? ForcePage => _forcePage;

	[Body]
	Comet.View body()
	{
		var store = DataStore.Instance;
		var firstProject = store.Projects.Value?.FirstOrDefault();
		var firstTask = store.AllTasks.Value?.FirstOrDefault();
		return (_forcePage ?? "dashboard") switch
		{
			"dashboard" => new DashboardPage(),
			"projects" => new ProjectListPage(),
			"manage" => new ManageMetaPage(),
			"projectdetail" => new ProjectDetailPage(firstProject ?? new CometProjectManager.Models.Project()),
			"taskdetail" => new TaskDetailPage(firstTask, firstTask?.ProjectID ?? 1),
			_ => new DashboardPage(),
		};
	}
}

/// <summary>
/// MAUI Shell providing real flyout/hamburger navigation identical to the XAML reference app.
/// Each page wraps Comet MVU views via MauiViewHost.
/// </summary>
public class ProjectManagerShell : MauiShell
{
	public ProjectManagerShell()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;

		Items.Add(new Microsoft.Maui.Controls.ShellContent
		{
			Title = "Dashboard",
			Icon = MakeIcon(Fonts.FluentUI.diagram_24_regular),
			ContentTemplate = new DataTemplate(() => MakeCometPage(new DashboardPage(wrapInNav: false), "Dashboard")),
			Route = "dashboard"
		});

		Items.Add(new Microsoft.Maui.Controls.ShellContent
		{
			Title = "Projects",
			Icon = MakeIcon(Fonts.FluentUI.list_24_regular),
			ContentTemplate = new DataTemplate(() => MakeCometPage(new ProjectListPage(wrapInNav: false), "Projects")),
			Route = "projects"
		});

		Items.Add(new Microsoft.Maui.Controls.ShellContent
		{
			Title = "Manage Meta",
			Icon = MakeIcon(Fonts.FluentUI.info_24_regular),
			ContentTemplate = new DataTemplate(() => MakeCometPage(new ManageMetaPage(wrapInNav: false), "Manage Meta")),
			Route = "manage"
		});

		// Register detail routes
		Routing.RegisterRoute("project", typeof(ProjectDetailShellPage));
		Routing.RegisterRoute("task", typeof(TaskDetailShellPage));
	}

	static Microsoft.Maui.Controls.FontImageSource MakeIcon(string glyph) => new Microsoft.Maui.Controls.FontImageSource
	{
		Glyph = glyph,
		FontFamily = Fonts.FluentUI.FontFamily,
		Color = Color.FromArgb("#0D0D0D"),
		Size = 24
	};

	/// <summary>
	/// Wraps a Comet View in a MAUI ContentPage for Shell hosting.
	/// </summary>
	static MauiPage MakeCometPage(Comet.View cometView, string title)
	{
		var page = new MauiPage
		{
			Title = title,
			Content = new CometHost(cometView),
			BackgroundColor = Color.FromArgb("#F2F2F2"),
		};
		MauiShell.SetNavBarIsVisible(page, true);
		return page;
	}
}

/// <summary>
/// Project detail page for Shell navigation
/// </summary>
[QueryProperty(nameof(ProjectId), "id")]
public class ProjectDetailShellPage : MauiPage
{
	string _projectId;
	public string ProjectId
	{
		get => _projectId;
		set
		{
			_projectId = value;
			LoadProject();
		}
	}

	void LoadProject()
	{
		if (int.TryParse(_projectId, out var id))
		{
			var project = DataStore.Instance.Projects.Value?.FirstOrDefault(p => p.ID == id)
				?? new CometProjectManager.Models.Project();
			Title = "Project";
			Content = new CometHost(new ProjectDetailPage(project, wrapInNav: false));
		}
	}
}

/// <summary>
/// Task detail page for Shell navigation
/// </summary>
[QueryProperty(nameof(TaskId), "id")]
public class TaskDetailShellPage : MauiPage
{
	string _taskId;
	public string TaskId
	{
		get => _taskId;
		set
		{
			_taskId = value;
			LoadTask();
		}
	}

	void LoadTask()
	{
		if (int.TryParse(_taskId, out var id))
		{
			var task = DataStore.Instance.AllTasks.Value?.FirstOrDefault(t => t.ID == id);
			var projectId = task?.ProjectID ?? 1;
			Title = "Task";
			Content = new CometHost(new TaskDetailPage(task, projectId, wrapInNav: false));
		}
	}
}

/// <summary>
/// Standard MAUI Application with Shell for proper flyout navigation.
/// </summary>
public class ShellMauiApp : Application
{
	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new ProjectManagerShell());
	}
}

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var args = System.Environment.GetCommandLineArgs();
		foreach (var arg in args)
		{
			if (arg.StartsWith("--page=", System.StringComparison.OrdinalIgnoreCase))
			{
				ProjectManagerApp.SetForcePage(arg.Substring(7).ToLowerInvariant());
			}
		}

		var builder = MauiApp.CreateBuilder();

		if (ProjectManagerApp.ForcePage != null)
		{
			// Snapshot testing mode: use CometApp
			builder.UseCometApp<ProjectManagerApp>();
		}
		else
		{
			// Normal mode: use real MAUI Shell
			builder.UseMauiApp<ShellMauiApp>();
		}

		builder.ConfigureSyncfusionToolkit()
			.UseCometHandlers()
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
