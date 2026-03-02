using CometProjectManager.Controls;
using CometProjectManager.Models;
using Syncfusion.Maui.Toolkit.TextInputLayout;

namespace CometProjectManager.Pages;

/// <summary>
/// Task detail — matches the template's TaskDetailPage exactly.
/// Delete toolbar icon, SfTextInputLayout for Title/Completed/Project,
/// Save button (44pt height).
/// </summary>
public class TaskDetailPage : View
{
	[State] readonly DataStore _store = DataStore.Instance;
	readonly ProjectTask? _existingTask;
	readonly int _defaultProjectId;

	static readonly Color DarkOnLightBg = Color.FromArgb("#0D0D0D");

	public TaskDetailPage(ProjectTask? task, int defaultProjectId)
	{
		_existingTask = task;
		_defaultProjectId = defaultProjectId;
	}

	[Body]
	View body()
	{
		var projects = _store.Projects.Value ?? new List<Project>();
		var isExisting = _existingTask != null;

		// MAUI input controls — values read directly on Save
		var titleEntry = new Microsoft.Maui.Controls.Entry
		{
			Text = _existingTask?.Title ?? "",
			Placeholder = "What needs to be done?",
			FontSize = 17,
		};

		var completedCheck = new Microsoft.Maui.Controls.CheckBox
		{
			IsChecked = _existingTask?.IsCompleted ?? false,
		};

		var projectPicker = new Microsoft.Maui.Controls.Picker
		{
			ItemsSource = projects.Select(p => p.Name).ToList(),
			SelectedIndex = Math.Max(0, projects.FindIndex(p =>
				p.ID == (_existingTask?.ProjectID ?? _defaultProjectId))),
		};

		return new NavigationView
		{
			new Grid
			{
				new ScrollView
				{
					new VStack(spacing: 5)
					{
						// Delete toolbar item (right-aligned, FluentUI delete icon)
						isExisting
							? new HStack
							{
								new Spacer(),
								new MauiViewHost(new Microsoft.Maui.Controls.Image
								{
									Source = new Microsoft.Maui.Controls.FontImageSource
									{
										Glyph = Fonts.FluentUI.delete_24_regular,
										FontFamily = Fonts.FluentUI.FontFamily,
										Color = DarkOnLightBg,
										Size = 24,
									},
									HeightRequest = 24,
									WidthRequest = 24,
								}).Frame(width: 24, height: 24),
							}
							.OnTap(_ =>
							{
								_store.DeleteTask(_existingTask!.ID);
								this.Dismiss();
							})
							.SemanticDescription("Delete task")
							as View
							: new Spacer().Frame(height: 0),

						// Task title (SfTextInputLayout > Entry)
						new MauiViewHost(new TextInputControl("Task", titleEntry))
							.Frame(height: 60)
							.SemanticDescription("Title"),

						// Completed (SfTextInputLayout > CheckBox)
						new MauiViewHost(new TextInputControl("Completed", completedCheck))
							.Frame(height: 60)
							.SemanticDescription("Status"),

						// Project picker (SfTextInputLayout > Picker, visible only for existing tasks)
						isExisting
							? new MauiViewHost(new TextInputControl("Project", projectPicker))
								.Frame(height: 60)
								.SemanticDescription("Project") as View
							: new Spacer().Frame(height: 0),

						// Save button (full width, 44pt)
						new Button("Save", () =>
						{
							var title = titleEntry.Text?.Trim();
							if (string.IsNullOrEmpty(title)) return;

							var allProjects = _store.Projects.Value ?? new List<Project>();
							var projectId = projectPicker.SelectedIndex >= 0 && projectPicker.SelectedIndex < allProjects.Count
								? allProjects[projectPicker.SelectedIndex].ID
								: _defaultProjectId;

							if (_existingTask != null)
							{
								_existingTask.Title = title;
								_existingTask.IsCompleted = completedCheck.IsChecked;
								_existingTask.ProjectID = projectId;
								_store.AllTasks.Value = new List<ProjectTask>(_store.AllTasks.Value!);
							}
							else
							{
								_store.AddTask(new ProjectTask
								{
									Title = title,
									IsCompleted = completedCheck.IsChecked,
									ProjectID = projectId,
								});
							}

							this.Dismiss();
						})
						.Frame(height: 44)
						.SemanticDescription("Save task"),
					}
					.Padding(new Thickness(15))
				},
			}
		}
		.Title("Task");
	}
}
