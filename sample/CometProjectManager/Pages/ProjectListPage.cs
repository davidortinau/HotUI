using CometProjectManager.Models;

namespace CometProjectManager.Pages;

/// <summary>
/// Project list — equivalent to the template's ProjectListPage.
/// Shows all projects in a vertical list with name/description, add button.
/// </summary>
public class ProjectListPage : View
{
	[State] readonly DataStore _store = DataStore.Instance;

	[Body]
	View body()
	{
		var projects = _store.Projects.Value ?? new List<Project>();

		return new NavigationView
		{
			new VStack
			{
				new ListView<Project>(() => projects)
				{
					ViewFor = project => new HStack(spacing: 12)
					{
						new Text(project.Icon).FontSize(24),
						new VStack(spacing: 2)
						{
							new Text(project.Name)
								.FontSize(18)
								.FontWeight(FontWeight.Semibold),
							new Text(project.Description)
								.FontSize(13)
								.Color(Colors.Gray),
						},
						new Spacer(),
						new Text($"{project.Tasks.Count} tasks")
							.FontSize(12)
							.Color(Colors.DarkGray),
					}
					.Padding(new Thickness(12, 10))
					.OnTap(_ => Navigation?.Navigate(new ProjectDetailPage(project)))
					.SemanticDescription($"{project.Name} project"),
				},

				new Button("+ Add Project", () =>
				{
					var newProject = new Project
					{
						Name = "New Project",
						Description = "Tap to edit",
						Icon = "📁",
						CategoryID = 1,
					};
					_store.AddProject(newProject);
					Navigation?.Navigate(new ProjectDetailPage(newProject));
				})
				.Padding(new Thickness(16, 12))
				.SemanticDescription("Add a new project"),
			}
		}
		.Title("Projects");
	}
}
