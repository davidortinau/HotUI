namespace CometTaskApp;

/// <summary>
/// Form for adding a new task.
/// Exercises: Multiple TextField bindings, Picker control, form validation, navigation.
/// </summary>
public class AddTaskPage : View
{
	readonly State<string> _title = new("");
	readonly State<string> _description = new("");
	readonly State<int> _priority = new(1); // Medium default
	readonly State<int> _category = new(0); // Personal default
	readonly State<string> _errorMessage = new("");

	[Body]
	View body() =>
		new NavigationView
		{
			new ScrollView
			{
				new VStack(spacing: 16)
				{
					new Text("Create New Task")
						.FontSize(24)
						.FontWeight(FontWeight.Bold)
						.SemanticHeadingLevel(SemanticHeadingLevel.Level1),

					string.IsNullOrEmpty(_errorMessage.Value)
						? (View)new Spacer().Frame(height: 0)
						: new Text(_errorMessage.Value!)
							.FontSize(14)
							.Color(Colors.Red),

					new Text("Title *").FontSize(12).Color(Colors.Gray),
					new TextField(_title, "What needs to be done?")
						.FontSize(16)
						.SemanticDescription("Task title, required"),

					new Text("Description").FontSize(12).Color(Colors.Gray),
					new TextField(_description, "Add more details...")
						.FontSize(14)
						.SemanticDescription("Task description"),

					new Text("Priority").FontSize(12).Color(Colors.Gray),
					new Picker(_priority, "🟢 Low", "🟡 Medium", "🟠 High", "🔴 Critical")
						.SemanticDescription("Task priority level"),

					new Text("Category").FontSize(12).Color(Colors.Gray),
					new Picker(_category, "🏠 Personal", "💼 Work", "🛒 Shopping", "💪 Health", "📚 Learning", "📌 Other")
						.SemanticDescription("Task category"),

					new Spacer().Frame(height: 20),

					new Button("Create Task", () =>
					{
						if (string.IsNullOrWhiteSpace(_title.Value))
						{
							_errorMessage.Value = "Please enter a task title.";
							return;
						}

						AppState.Instance.AddTask(new TaskItem
						{
							Title = _title.Value!.Trim(),
							Description = _description.Value ?? "",
							Priority = (TaskPriority)_priority.Value,
							Category = (TaskCategory)_category.Value,
						});

						Navigation?.Dismiss();
					})
					.SemanticDescription("Create the task"),

					new Button("Cancel", () => Navigation?.Dismiss())
						.Color(Colors.Gray)
						.SemanticDescription("Cancel and go back"),
				}
				.Padding(16)
			}
		}
		.Title("New Task");
}
