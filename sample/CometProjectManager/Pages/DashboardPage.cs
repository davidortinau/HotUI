using CometProjectManager.Models;

namespace CometProjectManager.Pages;

/// <summary>
/// Dashboard page — pixel-perfect match of the MAUI template's MainPage.
/// Shows: category chart (in card), horizontal project cards, task list, FAB.
/// Colors/fonts/spacing match AppStyles.xaml and Colors.xaml exactly.
/// </summary>
public class DashboardPage : View
{
	[State] readonly DataStore _store = DataStore.Instance;

	// Colors from the MAUI template Colors.xaml
	static readonly Color Primary = Color.FromArgb("#512BD4");
	static readonly Color LightSecondaryBg = Color.FromArgb("#E0E0E0");
	static readonly Color DarkOnLightBg = Color.FromArgb("#0D0D0D");
	static readonly Color LightBg = Color.FromArgb("#F2F2F2");

	/// <summary>
	/// Category chart card — matches the template's CategoryChart control.
	/// Uses horizontal bar representation since we can't use Syncfusion RadialBarSeries.
	/// Wrapped in a card (CardStyle: RoundRectangle 20, secondary bg, no stroke, 15 padding).
	/// </summary>
	View CategoryChart()
	{
		var data = _store.GetCategoryChartData();
		var maxCount = data.Max(d => d.Count);
		if (maxCount == 0) maxCount = 1;

		return new Border
		{
			Content = new HStack(spacing: 12)
			{
				// Chart bars (left side)
				new VStack(spacing: 6)
				{
					data.Select(d =>
					{
						var barWidth = Math.Max(8, (float)(d.Count * 120.0 / maxCount));
						return new HStack(spacing: 0)
						{
							new ShapeView(new RoundedRectangle(8))
								.Frame(width: barWidth, height: 16)
								.Background(new SolidPaint(d.Color)),
						} as View;
					}).ToArray()
				},
				// Legend (right side) — matches the template legend
				new VStack(spacing: 6)
				{
					data.Select(d => new HStack(spacing: 6)
					{
						new ShapeView(new Circle())
							.Frame(width: 10, height: 10)
							.Background(new SolidPaint(d.Color)),
						new Text($"{d.Title}: {d.Count}")
							.FontSize(16),
					}
					.SemanticDescription($"{d.Title}: {d.Count} tasks") as View).ToArray()
				},
			}
			.Padding(new Thickness(15)),
		}
		.Frame(height: 200)
		.Margin(new Thickness(0, 12))
		.Background(new SolidPaint(LightSecondaryBg))
		.ClipShape(new RoundedRectangle(20));
	}

	/// <summary>
	/// Project card — matches the template's ProjectCardView.
	/// CardStyle border, icon, name (uppercase gray), description, tag pills.
	/// Width 200 as in the template.
	/// </summary>
	View ProjectCard(Project project)
	{
		return new Border
		{
			Content = new VStack(spacing: 15)
			{
				// Icon (FluentUI glyph or fallback)
				new Text(project.Icon)
					.FontSize(20),
				// Name (uppercase, gray, 14pt — matches template)
				new Text(project.Name.ToUpperInvariant())
					.FontSize(14)
					.Color(Color.FromArgb("#919191")),
				// Description
				new Text(project.Description)
					.FontSize(17),
				// Tag pills
				new HStack(spacing: 15)
				{
					project.Tags.Select(tag =>
						new Text(tag.Title)
							.FontSize(13)
							.Color(Colors.White)
							.Background(new SolidPaint(tag.DisplayColor))
							.Padding(new Thickness(8, 3))
							.ClipShape(new RoundedRectangle(10))
						as View).ToArray()
				},
			}
			.Padding(new Thickness(15)),
		}
		.Frame(width: 200)
		.Background(new SolidPaint(LightSecondaryBg))
		.ClipShape(new RoundedRectangle(20))
		.OnTap(_ => Navigation?.Navigate(new ProjectDetailPage(project)))
		.SemanticDescription($"{project.Name} project. {project.Description}");
	}

	/// <summary>
	/// Task row — matches the template's TaskView control.
	/// Border with RoundRectangle 20, secondary bg, CheckBox + Label inside.
	/// </summary>
	View TaskRow(ProjectTask task)
	{
		return new Border
		{
			Content = new HStack(spacing: 15)
			{
				new Text(task.IsCompleted ? "☑" : "☐")
					.FontSize(22)
					.OnTap(_ => _store.ToggleTaskComplete(task.ID))
					.SemanticDescription(task.Title),
				new Text(task.Title)
					.FontSize(17)
					.Color(DarkOnLightBg),
				new Spacer(),
			}
			.Padding(new Thickness(15)),
		}
		.Background(new SolidPaint(LightSecondaryBg))
		.ClipShape(new RoundedRectangle(20))
		.OnTap(_ => Navigation?.Navigate(new TaskDetailPage(task, task.ProjectID)))
		.SemanticDescription($"Task: {task.Title}, {(task.IsCompleted ? "completed" : "pending")}");
	}

	/// <summary>
	/// Floating action button — matches the template's AddButton control.
	/// Purple circle button, bottom-right, 60x60, CornerRadius 30.
	/// </summary>
	View FloatingAddButton()
	{
		return new Button("+", () =>
		{
			Navigation?.Navigate(new TaskDetailPage(null, 0));
		})
		.Frame(width: 60, height: 60)
		.Background(new SolidPaint(Primary))
		.Color(Colors.White)
		.FontSize(28)
		.Margin(new Thickness(30))
		.SemanticDescription("Add task");
	}

	[Body]
	View body()
	{
		var tasks = _store.AllTasks.Value ?? new List<ProjectTask>();
		var projects = _store.Projects.Value ?? new List<Project>();
		var hasCompleted = tasks.Any(t => t.IsCompleted);

		return new NavigationView
		{
			new Grid
			{
				// Main scrollable content
				new ScrollView
				{
					new VStack(spacing: 5) // LayoutSpacing = 5 on phone
					{
						// Category chart card
						CategoryChart(),

						// "Projects" title — Title2 style (22pt, semibold)
						new Text("Projects")
							.FontSize(22)
							.FontWeight(FontWeight.Semibold),

						// Horizontal scrolling project cards (margin -30,0 + padding 30,0 for edge-to-edge)
						new ScrollView(Orientation.Horizontal)
						{
							new HStack(spacing: 15)
							{
								projects.Select(p => ProjectCard(p) as View).ToArray()
							}
							.Padding(new Thickness(30, 0))
						}
						.Margin(new Thickness(-30, 0)),

						// "Tasks" header with clean button
						new Grid
						{
							new Text("Tasks")
								.FontSize(22)
								.FontWeight(FontWeight.Semibold),
						}
						.Frame(height: 44),

						// Task list
						new VStack(spacing: 15)
						{
							tasks.Select(t => TaskRow(t) as View).ToArray()
						},
					}
					.Padding(new Thickness(15)) // LayoutPadding = 15 on phone
				},

				// FAB overlay — bottom right
				FloatingAddButton(),
			}
		}
		.Title(_store.Today);
	}
}
