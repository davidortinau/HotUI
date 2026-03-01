namespace CometTaskApp;

/// <summary>
/// Detail view for a single task with edit capability.
/// Exercises: BindingObject property binding, TextField, Picker, navigation.
/// </summary>
public class TaskDetailPage : View
{
	[State] readonly AppState _state = AppState.Instance;
	readonly TaskItem _task;

	readonly State<string> _editTitle;
	readonly State<string> _editDescription;
	readonly State<int> _editPriority;
	readonly State<int> _editCategory;

	public TaskDetailPage(TaskItem task)
	{
		_task = task;
		_editTitle = new State<string>(task.Title);
		_editDescription = new State<string>(task.Description);
		_editPriority = new State<int>((int)task.Priority);
		_editCategory = new State<int>((int)task.Category);
	}

	[Body]
	View body() =>
		new NavigationView
		{
			new ScrollView
			{
				new VStack(spacing: 16)
				{
					new HStack(spacing: 8)
					{
						new Text(_task.IsCompleted ? "✅ Completed" : "⏳ Pending")
							.FontSize(14)
							.Color(_task.IsCompleted ? Colors.Green : Colors.Orange),
						new Spacer(),
						new Text($"Created: {_task.CreatedAt:MMM dd, yyyy}")
							.FontSize(12)
							.Color(Colors.Gray),
					},

					new Text("Title").FontSize(12).Color(Colors.Gray),
					new TextField(_editTitle, "Task title...")
						.FontSize(18)
						.SemanticDescription("Task title"),

					new Text("Description").FontSize(12).Color(Colors.Gray),
					new TextField(_editDescription, "Task description...")
						.FontSize(14)
						.SemanticDescription("Task description"),

					new Text("Priority").FontSize(12).Color(Colors.Gray),
					new Picker(_editPriority, "Low", "Medium", "High", "Critical")
						.SemanticDescription("Task priority"),

					new Text("Category").FontSize(12).Color(Colors.Gray),
					new Picker(_editCategory, "Personal", "Work", "Shopping", "Health", "Learning", "Other")
						.SemanticDescription("Task category"),

					new Spacer().Frame(height: 20),

					new Button("Save Changes", () =>
					{
						_task.Title = _editTitle.Value ?? "";
						_task.Description = _editDescription.Value ?? "";
						_task.Priority = (TaskPriority)_editPriority.Value;
						_task.Category = (TaskCategory)_editCategory.Value;
						_state.Tasks.Value = new List<TaskItem>(_state.Tasks.Value!);
						Navigation?.Dismiss();
					})
					.SemanticDescription("Save task changes"),

					new HStack(spacing: 12)
					{
						new Button(_task.IsCompleted ? "Mark Pending" : "Mark Complete", () =>
						{
							_state.ToggleComplete(_task.Id);
							Navigation?.Dismiss();
						})
						.Color(_task.IsCompleted ? Colors.Orange : Colors.Green)
						.SemanticDescription("Toggle task completion"),

						new Button("Delete", () =>
						{
							_state.RemoveTask(_task.Id);
							Navigation?.Dismiss();
						})
						.Color(Colors.Red)
						.SemanticDescription("Delete this task"),
					},
				}
				.Padding(16)
			}
		}
		.Title("Task Details");
}
