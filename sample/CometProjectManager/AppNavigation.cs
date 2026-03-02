using CometProjectManager.Models;
using CometProjectManager.Pages;
using Microsoft.Maui.Controls;
using MauiShell = Microsoft.Maui.Controls.Shell;

namespace CometProjectManager;

/// <summary>
/// Abstracts navigation so pages work both in Shell mode and Comet NavigationView mode.
/// </summary>
public static class AppNavigation
{
	public static bool IsShellMode => ProjectManagerApp.ForcePage == null;

	public static void NavigateToProject(Project project, NavigationView? cometNav = null)
	{
		if (IsShellMode && MauiShell.Current != null)
		{
			_ = MauiShell.Current.GoToAsync($"project?id={project.ID}");
		}
		else
		{
			cometNav?.Navigate(new ProjectDetailPage(project));
		}
	}

	public static void NavigateToTask(ProjectTask? task, int projectId, NavigationView? cometNav = null)
	{
		if (IsShellMode && MauiShell.Current != null)
		{
			var taskId = task?.ID ?? 0;
			_ = MauiShell.Current.GoToAsync($"task?id={taskId}");
		}
		else
		{
			cometNav?.Navigate(new TaskDetailPage(task, projectId));
		}
	}
}
