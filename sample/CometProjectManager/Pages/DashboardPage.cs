using CometProjectManager.Models;

namespace CometProjectManager.Pages;

/// <summary>
/// Dashboard page — equivalent to the template's MainPage.
/// Shows: category chart, horizontal project cards, task list with completion toggle, add task FAB.
/// </summary>
public class DashboardPage : View
{
	[State] readonly DataStore _store = DataStore.Instance;

	View CategoryChart()
	{
		var data = _store.GetCategoryChartData();
		var maxCount = data.Max(d => d.Count);
		if (maxCount == 0) maxCount = 1;

		return new VStack(spacing: 8)
		{
			data.Select(d => new HStack(spacing: 8)
			{
				new Text(d.Title)
					.FontSize(13)
					.Frame(width: 100),
				new ShapeView(new RoundedRectangle(4))
					.Frame(width: (float)(d.Count * 180.0 / maxCount) + 4, height: 20)
					.Background(new SolidPaint(d.Color)),
				new Text($"{d.Count}")
					.FontSize(13)
					.Color(Colors.Gray),
			}
			.SemanticDescription($"{d.Title}: {d.Count} tasks") as View).ToArray()
		};
	}

	View ProjectCard(Project project)
	{
		var taskCount = project.Tasks.Count;
		var completedCount = project.Tasks.Count(t => t.IsCompleted);

		return new VStack(spacing: 6)
		{
			new Text(project.Icon).FontSize(28),
			new Text(project.Name)
				.FontSize(16)
				.FontWeight(FontWeight.Semibold),
			new Text(project.Description)
				.FontSize(12)
				.Color(Colors.Gray),
			new Text($"{completedCount}/{taskCount} tasks")
				.FontSize(11)
				.Color(Colors.DarkGray),
		}
		.Padding(new Thickness(12))
		.Frame(width: 180)
		.Background(new SolidPaint(Colors.WhiteSmoke))
		.ClipShape(new RoundedRectangle(12))
		.OnTap(_ => Navigation?.Navigate(new ProjectDetailPage(project)))
		.SemanticDescription($"{project.Name} project. {project.Description}");
	}

	View TaskRow(ProjectTask task)
	{
		return new HStack(spacing: 10)
		{
			new Text(task.IsCompleted ? "✅" : "⬜")
				.FontSize(18)
				.OnTap(_ => _store.ToggleTaskComplete(task.ID)),
			new Text(task.Title)
				.FontSize(15)
				.Color(task.IsCompleted ? Colors.Gray : Colors.Black),
			new Spacer(),
		}
		.Padding(new Thickness(8, 6))
		.SemanticDescription($"Task: {task.Title}, {(task.IsCompleted ? "completed" : "pending")}");
	}

	[Body]
	View body()
	{
		var tasks = _store.AllTasks.Value ?? new List<ProjectTask>();
		var projects = _store.Projects.Value ?? new List<Project>();
		var hasCompleted = tasks.Any(t => t.IsCompleted);

		return new NavigationView
		{
			new ScrollView
			{
				new VStack(spacing: 20)
				{
					new Text("Task Categories")
						.FontSize(20)
						.FontWeight(FontWeight.Bold)
						.SemanticHeadingLevel(SemanticHeadingLevel.Level1),
					CategoryChart(),

					new Text("Projects")
						.FontSize(20)
						.FontWeight(FontWeight.Bold)
						.SemanticHeadingLevel(SemanticHeadingLevel.Level1),
					new ScrollView(Orientation.Horizontal)
					{
						new HStack(spacing: 12)
						{
							projects.Select(p => ProjectCard(p) as View).ToArray()
						}
					},

					new HStack
					{
						new Text("Tasks")
							.FontSize(20)
							.FontWeight(FontWeight.Bold)
							.SemanticHeadingLevel(SemanticHeadingLevel.Level1),
						new Spacer(),
						hasCompleted
							? new Text("🧹 Clean")
								.FontSize(14)
								.Color(Colors.DodgerBlue)
								.OnTap(_ => _store.CleanCompletedTasks())
								.SemanticDescription("Clean completed tasks")
							: new Spacer().Frame(width: 0) as View,
					},

					new VStack(spacing: 4)
					{
						tasks.Select(t => TaskRow(t) as View).ToArray()
					},

					new Button("+ Add Task", () =>
					{
						Navigation?.Navigate(new TaskDetailPage(null, 0));
					})
					.SemanticDescription("Add a new task"),
				}
				.Padding(new Thickness(16))
			}
		}
		.Title(_store.Today);
	}
}
