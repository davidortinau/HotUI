using CometProjectManager.Models;

namespace CometProjectManager.Pages;

/// <summary>
/// Task detail — equivalent to the template's TaskDetailPage.
/// Edit task title, toggle completion, assign to project.
/// </summary>
public class TaskDetailPage : View
{
	[State] readonly DataStore _store = DataStore.Instance;
	readonly ProjectTask? _existingTask;
	readonly int _defaultProjectId;

	readonly State<string> _title;
	readonly State<bool> _isCompleted;
	readonly State<int> _projectIndex;

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
			new ScrollView
			{
				new VStack(spacing: 16)
				{
					// Task Info Frame
					new Frame
					{
						Content = new VStack(spacing: 16)
						{
							// Title
							new Text("Task").FontSize(12).Color(Colors.Gray),
							new TextField(_title, "What needs to be done?")
								.FontSize(18)
								.SemanticDescription("Task title"),

							// Completed toggle
							new HStack(spacing: 12)
							{
								new Text("Completed").FontSize(14),
								new Spacer(),
								new Text(_isCompleted.Value ? "✅ Yes" : "⬜ No")
									.FontSize(16)
									.OnTap(_ => _isCompleted.Value = !_isCompleted.Value)
									.SemanticDescription("Toggle task completion")
									.SemanticHint("Tap to toggle completed status"),
							}
							.Padding(new Thickness(0, 8)),

							// Project picker
							new Text("Project").FontSize(12).Color(Colors.Gray),
							new Picker(
								_projectIndex,
								projects.Select(p => $"{p.Icon} {p.Name}").ToArray()
							).SemanticDescription("Assign to project"),
						}
						.Padding(new Thickness(12)),
						CornerRadius = 8,
						HasShadow = true,
					},

					new Spacer().Frame(height: 20),

					// Save
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
							// Force refresh
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
					.IsEnabled(!string.IsNullOrWhiteSpace(_title.Value))
					.SemanticDescription("Save task"),

					// Delete (only for existing tasks)
					isExisting
						? new Button("Delete Task", () =>
						{
							_store.DeleteTask(_existingTask!.ID);
							Navigation?.Dismiss();
						})
						.Color(Colors.Red)
						.SemanticDescription("Delete this task")
						: new Spacer().Frame(height: 0) as View,
				}
				.Padding(new Thickness(16))
				.FlowDirection(FlowDirection.LeftToRight)
			}
		}
		.Title("Task");
	}
}
