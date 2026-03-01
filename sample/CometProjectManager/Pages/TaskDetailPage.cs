using CometProjectManager.Models;

namespace CometProjectManager.Pages;

/// <summary>
/// Task detail — matches the template's TaskDetailPage exactly.
/// Task title entry, completed toggle, project picker, save/delete buttons.
/// Uses the same layout structure: VerticalStackLayout with LayoutPadding/Spacing.
/// </summary>
public class TaskDetailPage : View
{
	[State] readonly DataStore _store = DataStore.Instance;
	readonly ProjectTask? _existingTask;
	readonly int _defaultProjectId;

	readonly State<string> _title;
	readonly State<bool> _isCompleted;
	readonly State<int> _projectIndex;

	static readonly Color Primary = Color.FromArgb("#512BD4");

	public TaskDetailPage(ProjectTask? task, int defaultProjectId)
	{
		_existingTask = task;
		_defaultProjectId = defaultProjectId;
		_title = new State<string>(task?.Title ?? "");
		_isCompleted = new State<bool>(task?.IsCompleted ?? false);

		var projects = DataStore.Instance.Projects.Value ?? new List<Project>();
		var projectId = task?.ProjectID ?? defaultProjectId;
		_projectIndex = new State<int>(
			Math.Max(0, projects.FindIndex(p => p.ID == projectId)));
	}

	[Body]
	View body()
	{
		var projects = _store.Projects.Value ?? new List<Project>();
		var isExisting = _existingTask != null;

		return new NavigationView
		{
			new Grid
			{
				new ScrollView
				{
					new VStack(spacing: 5) // LayoutSpacing = 5
					{
						// Task title (matches SfTextInputLayout Hint="Task")
						new Text("Task").FontSize(12).Color(Colors.Gray),
						new TextField(_title, "What needs to be done?")
							.FontSize(17)
							.SemanticDescription("Title"),

						// Completed toggle (matches template's CheckBox in SfTextInputLayout)
						new Text("Completed").FontSize(12).Color(Colors.Gray),
						new Toggle(_isCompleted)
						.SemanticDescription("Status")
						.SemanticHint("Indicates if this task is completed"),

						// Project picker (matches template's Picker in SfTextInputLayout)
						isExisting
							? new VStack(spacing: 5)
							{
								new Text("Project").FontSize(12).Color(Colors.Gray),
								new Picker(
									_projectIndex,
									projects.Select(p => p.Name).ToArray()
								)
								.SemanticDescription("Project")
								.SemanticHint("Which project this task belongs to"),
							} as View
							: new Spacer().Frame(height: 0),

						// Save button (matches template: full width, 44pt)
						new Button("Save", () =>
						{
							var title = _title.Value?.Trim();
							if (string.IsNullOrEmpty(title)) return;

							var allProjects = _store.Projects.Value ?? new List<Project>();
							var projectId = _projectIndex.Value >= 0 && _projectIndex.Value < allProjects.Count
								? allProjects[_projectIndex.Value].ID
								: _defaultProjectId;

							if (_existingTask != null)
							{
								_existingTask.Title = title;
								_existingTask.IsCompleted = _isCompleted.Value;
								_existingTask.ProjectID = projectId;
								_store.AllTasks.Value = new List<ProjectTask>(_store.AllTasks.Value!);
							}
							else
							{
								_store.AddTask(new ProjectTask
								{
									Title = title,
									IsCompleted = _isCompleted.Value,
									ProjectID = projectId,
								});
							}

							Navigation?.Dismiss();
						})
						.Frame(height: 44)
						.IsEnabled(!string.IsNullOrWhiteSpace(_title.Value))
						.SemanticDescription("Save task"),
					}
					.Padding(new Thickness(15)) // LayoutPadding
				},
			}
		}
		.Title("Task");
	}
}
