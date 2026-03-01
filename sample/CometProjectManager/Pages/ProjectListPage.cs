using CometProjectManager.Models;

namespace CometProjectManager.Pages;

/// <summary>
/// Project list — matches the template's ProjectListPage exactly.
/// Vertical list of projects in Border cards, FAB button.
/// </summary>
public class ProjectListPage : View
{
	[State] readonly DataStore _store = DataStore.Instance;

	static readonly Color Primary = Color.FromArgb("#512BD4");
	static readonly Color LightSecondaryBg = Color.FromArgb("#E0E0E0");

	[Body]
	View body()
	{
		var projects = _store.Projects.Value ?? new List<Project>();

		return new NavigationView
		{
			new Grid
			{
				new ScrollView
				{
					new VStack(spacing: 5) // LayoutSpacing = 5
					{
						projects.Select(project =>
							new Border
							{
								Content = new VStack(spacing: 4)
								{
									new Text(project.Name)
										.FontSize(24),
									new Text(project.Description)
										.FontSize(17),
								}
								.Padding(new Thickness(10)),
							}
							.Background(new SolidPaint(LightSecondaryBg))
							.ClipShape(new RoundedRectangle(20))
							.OnTap(_ => Navigation?.Navigate(new ProjectDetailPage(project)))
							.SemanticDescription($"{project.Name} project")
						as View).ToArray()
					}
					.Padding(new Thickness(15)) // LayoutPadding
				},

				// FAB
				new Button("+", () =>
				{
					var newProject = new Project
					{
						Name = "New Project",
						Description = "Tap to edit",
						Icon = "\uea28",
						CategoryID = 1,
					};
					_store.AddProject(newProject);
					Navigation?.Navigate(new ProjectDetailPage(newProject));
				})
				.Frame(width: 60, height: 60)
				.Background(new SolidPaint(Primary))
				.Color(Colors.White)
				.FontSize(28)
				.Margin(new Thickness(30))
				.SemanticDescription("Add project"),
			}
		}
		.Title("Projects");
	}
}
