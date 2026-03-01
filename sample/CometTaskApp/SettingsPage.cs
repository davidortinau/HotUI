namespace CometTaskApp;

/// <summary>
/// Settings page with toggles and action buttons.
/// Exercises: Toggle-like behavior with State<bool>, computed UI, danger zone actions.
/// </summary>
public class SettingsPage : View
{
	[State] readonly AppState _state = AppState.Instance;

	[Body]
	View body() =>
		new ScrollView
		{
			new VStack(spacing: 20)
			{
				new Text("⚙️ Settings")
					.FontSize(24)
					.FontWeight(FontWeight.Bold)
					.SemanticHeadingLevel(SemanticHeadingLevel.Level1),

				SettingRow(
					"Show Completed Tasks",
					"Include completed tasks in the list",
					_state.ShowCompletedTasks.Value
						? new Text("✅ Showing").Color(Colors.Green)
						: new Text("❌ Hidden").Color(Colors.Red),
					() => _state.ShowCompletedTasks.Value = !_state.ShowCompletedTasks.Value
				),

				SettingRow(
					"Dark Mode",
					"Switch between light and dark theme",
					_state.DarkMode.Value
						? new Text("🌙 Dark").Color(Colors.Purple)
						: new Text("☀️ Light").Color(Colors.Orange),
					() => _state.DarkMode.Value = !_state.DarkMode.Value
				),

				new Spacer().Frame(height: 8),

				new Text("Danger Zone")
					.FontSize(18)
					.FontWeight(FontWeight.Semibold)
					.Color(Colors.Red)
					.SemanticHeadingLevel(SemanticHeadingLevel.Level2),

				new Button("Clear All Completed", () =>
				{
					var list = new List<TaskItem>(_state.Tasks.Value ?? new List<TaskItem>());
					list.RemoveAll(t => t.IsCompleted);
					_state.Tasks.Value = list;
				})
				.Color(Colors.OrangeRed)
				.SemanticDescription("Remove all completed tasks"),

				new Button("Reset to Sample Data", () =>
				{
					_state.Tasks.Value = new List<TaskItem>
					{
						new() { Title = "Buy groceries", Description = "Milk, eggs, bread", Priority = TaskPriority.Medium, Category = TaskCategory.Shopping },
						new() { Title = "Finish report", Description = "Q4 summary", Priority = TaskPriority.High, Category = TaskCategory.Work },
						new() { Title = "Go for a run", Description = "30 min", Priority = TaskPriority.Low, Category = TaskCategory.Health },
					};
				})
				.Color(Colors.Red)
				.SemanticDescription("Reset all tasks to sample data"),

				new Spacer().Frame(height: 20),

				new Text("About")
					.FontSize(18)
					.FontWeight(FontWeight.Semibold)
					.SemanticHeadingLevel(SemanticHeadingLevel.Level2),

				new VStack(spacing: 4)
				{
					new Text("Comet Task Manager")
						.FontSize(14).FontWeight(FontWeight.Semibold),
					new Text("Built with Comet MVU for .NET MAUI")
						.FontSize(12).Color(Colors.Gray),
					new Text("Demonstrates: Navigation, Lists, Forms, Gestures, State Management, Accessibility")
						.FontSize(11).Color(Colors.DarkGray),
				}
				.Padding(12)
				.Background(new SolidPaint(Colors.WhiteSmoke))
				.ClipShape(new RoundedRectangle(8)),
			}
			.Padding(16)
		};

	View SettingRow(string title, string subtitle, View indicator, Action onTap)
	{
		return new HStack(spacing: 12)
		{
			new VStack(spacing: 2)
			{
				new Text(title).FontSize(16).FontWeight(FontWeight.Semibold),
				new Text(subtitle).FontSize(12).Color(Colors.Gray),
			},
			new Spacer(),
			indicator,
		}
		.Padding(12)
		.Background(new SolidPaint(Colors.WhiteSmoke))
		.ClipShape(new RoundedRectangle(8))
		.OnTap(_ => onTap())
		.SemanticDescription($"{title}: {subtitle}");
	}
}
