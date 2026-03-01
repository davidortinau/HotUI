using CometProjectManager.Models;

namespace CometProjectManager.Pages;

/// <summary>
/// Project detail — equivalent to the template's ProjectDetailPage.
/// Edit project name/description, pick category, select icon, manage tags, view tasks.
/// </summary>
public class ProjectDetailPage : View
{
	[State] readonly DataStore _store = DataStore.Instance;
	readonly Project _project;
	readonly bool _isNew;

	readonly State<string> _name;
	readonly State<string> _description;
	readonly State<int> _categoryIndex;
	readonly State<int> _iconIndex;

	static readonly string[] Icons = { "📁", "📱", "🌐", "🚀", "🏆", "📚", "🤖" };

	public ProjectDetailPage(Project project)
	{
		_project = project;
		_isNew = project.ID == 0;
		_name = new State<string>(project.Name);
		_description = new State<string>(project.Description);

		var categories = DataStore.Instance.Categories.Value ?? new List<Category>();
		_categoryIndex = new State<int>(
			Math.Max(0, categories.FindIndex(c => c.ID == project.CategoryID)));
		_iconIndex = new State<int>(
			Math.Max(0, Array.IndexOf(Icons, project.Icon)));
	}

	[Body]
	View body()
	{
		var categories = _store.Categories.Value ?? new List<Category>();
		var tasks = _project.Tasks;
		var hasCompleted = tasks.Any(t => t.IsCompleted);

		return new NavigationView
		{
			new ScrollView
			{
				new VStack(spacing: 16)
				{
					// Basic Info Frame
					new Frame
					{
						Content = new VStack(spacing: 16)
						{
							// Name
							new Text("Name").FontSize(12).Color(Colors.Gray),
							new TextField(_name, "Project name")
								.FontSize(18)
								.SemanticDescription("Project name"),

							// Description
							new Text("Description").FontSize(12).Color(Colors.Gray),
							new TextField(_description, "Project description")
								.FontSize(14)
								.SemanticDescription("Project description"),

							// Category picker
							new Text("Category").FontSize(12).Color(Colors.Gray),
							new Picker(
								_categoryIndex,
								categories.Select(c => c.Title).ToArray()
							).SemanticDescription("Project category"),
						}
						.Padding(new Thickness(12)),
						CornerRadius = 8,
						HasShadow = true,
					},

					// Icon picker Frame
					new Frame
					{
						Content = new VStack(spacing: 8)
						{
							new Text("Icon").FontSize(12).Color(Colors.Gray),
							new ScrollView(Orientation.Horizontal)
							{
								new HStack(spacing: 12)
								{
									Icons.Select((icon, idx) =>
									{
										var isSelected = _iconIndex.Value == idx;
										return new VStack
										{
											new Text(icon).FontSize(28),
											isSelected
												? new ShapeView(new RoundedRectangle(2))
													.Frame(height: 3)
													.Background(new SolidPaint(Colors.DodgerBlue))
												: new Spacer().Frame(height: 3) as View,
										}
										.OnTap(_ => _iconIndex.Value = idx)
										.SemanticDescription($"Icon: {icon}") as View;
									}).ToArray()
								}
							},
						}
						.Padding(new Thickness(12)),
						CornerRadius = 8,
						HasShadow = true,
					},

					// Tags
					new Frame
					{
						Content = new VStack(spacing: 8)
						{
							new Text("Tags").FontSize(12).Color(Colors.Gray),
							new ScrollView(Orientation.Horizontal)
							{
								new HStack(spacing: 8)
								{
									(_store.Tags.Value ?? new List<Tag>()).Select(tag =>
									{
										var isSelected = _project.Tags.Any(t => t.ID == tag.ID);
										return new Text(tag.Title)
											.FontSize(14)
											.Color(isSelected ? Colors.White : Colors.Black)
											.Background(new SolidPaint(isSelected ? tag.DisplayColor : Colors.LightGray))
											.Padding(new Thickness(12, 6))
											.ClipShape(new RoundedRectangle(16))
											.OnTap(_ =>
											{
												if (isSelected)
													_project.Tags.RemoveAll(t => t.ID == tag.ID);
												else
													_project.Tags.Add(new Tag { ID = tag.ID, Title = tag.Title, ColorHex = tag.ColorHex });
												_store.SaveProject(_project);
											})
											.SemanticDescription($"Tag: {tag.Title}, {(isSelected ? "selected" : "not selected")}") as View;
									}).ToArray()
								}
							},
						}
						.Padding(new Thickness(12)),
						CornerRadius = 8,
						HasShadow = true,
					},

					// Save button
					new Button("Save", () =>
					{
						var categories2 = _store.Categories.Value ?? new List<Category>();
						_project.Name = _name.Value ?? "";
						_project.Description = _description.Value ?? "";
						if (_categoryIndex.Value >= 0 && _categoryIndex.Value < categories2.Count)
							_project.CategoryID = categories2[_categoryIndex.Value].ID;
						if (_iconIndex.Value >= 0 && _iconIndex.Value < Icons.Length)
							_project.Icon = Icons[_iconIndex.Value];

						_store.SaveProject(_project);
						Navigation?.Dismiss();
					})
					.IsEnabled(!string.IsNullOrWhiteSpace(_name.Value))
					.SemanticDescription("Save project"),

					// Tasks section
					new HStack
					{
						new Text("Tasks")
							.FontSize(18)
							.FontWeight(FontWeight.Semibold)
							.SemanticHeadingLevel(SemanticHeadingLevel.Level2),
						new Spacer(),
						hasCompleted
							? new Text("🧹")
								.FontSize(16)
								.OnTap(_ =>
								{
									var completed = tasks.Where(t => t.IsCompleted).Select(t => t.ID).ToList();
									foreach (var id in completed) _store.DeleteTask(id);
								})
								.SemanticDescription("Clean completed tasks")
							: new Spacer().Frame(width: 0) as View,
					},

					new VStack(spacing: 4)
					{
						tasks.Select(task => new HStack(spacing: 10)
						{
							new Text(task.IsCompleted ? "✅" : "⬜")
								.FontSize(16)
								.OnTap(_ => _store.ToggleTaskComplete(task.ID)),
							new Text(task.Title)
								.FontSize(14)
								.Color(task.IsCompleted ? Colors.Gray : Colors.Black),
							new Spacer(),
							new Text("✏️")
								.FontSize(14)
								.OnTap(_ => Navigation?.Navigate(new TaskDetailPage(task, _project.ID))),
						}
						.Padding(new Thickness(4))
						.SemanticDescription($"Task: {task.Title}") as View).ToArray()
					},

					new Button("+ Add Task", () =>
					{
						Navigation?.Navigate(new TaskDetailPage(null, _project.ID));
					})
					.SemanticDescription("Add task to project"),

					// Delete button (only for existing projects)
					!_isNew
						? new Button("Delete Project", () =>
						{
							_store.DeleteProject(_project.ID);
							Navigation?.Dismiss();
						})
						.Color(Colors.Red)
						.SemanticDescription("Delete this project")
						: new Spacer().Frame(height: 0) as View,
				}
				.Padding(new Thickness(16))
				.FlowDirection(FlowDirection.LeftToRight)
			}
		}
		.Title("Project");
	}
}
