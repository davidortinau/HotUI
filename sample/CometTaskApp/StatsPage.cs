namespace CometTaskApp;

/// <summary>
/// Statistics dashboard with progress visualization.
/// Exercises: Computed state, ShapeView for visual charts, dynamic rendering.
/// </summary>
public class StatsPage : View
{
	[State] readonly AppState _state = AppState.Instance;

	[Body]
	View body()
	{
		var total = _state.TotalCount;
		var completed = _state.CompletedCount;
		var pending = _state.PendingCount;
		var percentage = total > 0 ? (int)(completed * 100.0 / total) : 0;

		return new ScrollView
		{
			new VStack(spacing: 20)
			{
				new Text("📊 Task Statistics")
					.FontSize(24)
					.FontWeight(FontWeight.Bold)
					.SemanticHeadingLevel(SemanticHeadingLevel.Level1),

				new ZStack
				{
					new ShapeView(new Circle())
						.Frame(120, 120)
						.Background(new SolidPaint(Colors.LightGray)),
					new ShapeView(new Circle())
						.Frame(100, 100)
						.Background(new SolidPaint(Colors.White)),
					new Text($"{percentage}%")
						.FontSize(28)
						.FontWeight(FontWeight.Bold)
						.Color(percentage >= 75 ? Colors.Green : percentage >= 50 ? Colors.Orange : Colors.Red),
				}
				.Frame(120, 120),

				StatCard("Total Tasks", total.ToString(), "📋", Colors.DodgerBlue),
				StatCard("Completed", completed.ToString(), "✅", Colors.Green),
				StatCard("Pending", pending.ToString(), "⏳", Colors.Orange),

				new Text("By Category")
					.FontSize(18)
					.FontWeight(FontWeight.Semibold)
					.SemanticHeadingLevel(SemanticHeadingLevel.Level2),

				CategoryBreakdown(),

				new Text("By Priority")
					.FontSize(18)
					.FontWeight(FontWeight.Semibold)
					.SemanticHeadingLevel(SemanticHeadingLevel.Level2),

				PriorityBreakdown(),
			}
			.Padding(16)
		};
	}

	View StatCard(string label, string value, string emoji, Color color)
	{
		return new HStack(spacing: 12)
		{
			new Text(emoji).FontSize(28),
			new VStack(spacing: 2)
			{
				new Text(value)
					.FontSize(22)
					.FontWeight(FontWeight.Bold)
					.Color(color),
				new Text(label)
					.FontSize(13)
					.Color(Colors.Gray),
			},
			new Spacer(),
		}
		.Padding(12)
		.Background(new SolidPaint(Colors.WhiteSmoke))
		.ClipShape(new RoundedRectangle(8))
		.SemanticDescription($"{label}: {value}");
	}

	View CategoryBreakdown()
	{
		var tasks = _state.Tasks.Value ?? new List<TaskItem>();
		var categories = Enum.GetValues<TaskCategory>();

		return new VStack(spacing: 6)
		{
			categories.Select(cat =>
			{
				var count = tasks.Count(t => t.Category == cat);
				var emoji = cat switch
				{
					TaskCategory.Personal => "🏠",
					TaskCategory.Work => "💼",
					TaskCategory.Shopping => "🛒",
					TaskCategory.Health => "💪",
					TaskCategory.Learning => "📚",
					_ => "📌"
				};
				return BarRow($"{emoji} {cat}", count, tasks.Count);
			}).ToArray()
		};
	}

	View PriorityBreakdown()
	{
		var tasks = _state.Tasks.Value ?? new List<TaskItem>();
		var priorities = Enum.GetValues<TaskPriority>();

		return new VStack(spacing: 6)
		{
			priorities.Select(p =>
			{
				var count = tasks.Count(t => t.Priority == p);
				return BarRow($"{p}", count, tasks.Count);
			}).ToArray()
		};
	}

	View BarRow(string label, int count, int total)
	{
		var fraction = total > 0 ? (double)count / total : 0;
		var barWidth = Math.Max(4, fraction * 200);

		return new HStack(spacing: 8)
		{
			new Text(label)
				.FontSize(13)
				.Frame(width: 100),
			new ShapeView(new RoundedRectangle(4))
				.Frame(width: (float)barWidth, height: 16)
				.Background(new SolidPaint(Colors.DodgerBlue)),
			new Text($"{count}")
				.FontSize(13)
				.Color(Colors.Gray),
		}
		.SemanticDescription($"{label}: {count} of {total}");
	}
}
