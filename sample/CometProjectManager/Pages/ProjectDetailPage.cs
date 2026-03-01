using CometProjectManager.Models;

namespace CometProjectManager.Pages;

/// <summary>
/// Project detail — matches the template's ProjectDetailPage exactly.
/// Name/Description entries, Category picker, Icon picker, Tags, Save button, Tasks section, FAB.
/// Uses the same layout structure: VerticalStackLayout with LayoutPadding/Spacing.
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

	static readonly Color Primary = Color.FromArgb("#512BD4");
	static readonly Color LightSecondaryBg = Color.FromArgb("#E0E0E0");
	static readonly Color DarkOnLightBg = Color.FromArgb("#0D0D0D");

	// Same icons as the template's IconData set
	static readonly string[] Icons = { "\uea28", "\uf8fe", "\uf837", "\uf5a9", "\ue823", "\ue7ee", "\uea3a" };
	static readonly string[] IconDescriptions = { "Balance", "Education", "Fitness", "People", "Document", "Settings", "Target" };

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
		.OnTap(_ => Navigation?.Navigate(new TaskDetailPage(task, _project.ID)));
	}

	[Body]
	View body()
	{
		var categories = _store.Categories.Value ?? new List<Category>();
		var tasks = _project.Tasks;
		var hasCompleted = tasks.Any(t => t.IsCompleted);

		return new NavigationView
		{
			new Grid
			{
				new ScrollView
				{
					new VStack(spacing: 5) // LayoutSpacing = 5
					{
						// Name field (matches SfTextInputLayout Hint="Name")
						new Text("Name").FontSize(12).Color(Colors.Gray),
						new TextField(_name, "Project name")
							.FontSize(17)
							.SemanticDescription("Name"),

						// Description field
						new Text("Description").FontSize(12).Color(Colors.Gray),
						new TextField(_description, "Project description")
							.FontSize(17)
							.SemanticDescription("Description"),

						// Category picker
						new Text("Category").FontSize(12).Color(Colors.Gray),
						new Picker(
							_categoryIndex,
							categories.Select(c => c.Title).ToArray()
						).SemanticDescription("Category"),

						// Icon picker — matches template's horizontal CollectionView with selection indicator
						new Text("Icon")
							.FontSize(22)
							.FontWeight(FontWeight.Semibold),
						new ScrollView(Orientation.Horizontal)
						{
							new HStack(spacing: 5)
							{
								Icons.Select((icon, idx) =>
								{
									var isSelected = _iconIndex.Value == idx;
									return new VStack(spacing: 6)
									{
										new Text(icon)
											.FontSize(24),
										// Selection indicator bar (matches template's BoxView)
										isSelected
											? new ShapeView(new RoundedRectangle(2))
												.Frame(height: 4)
												.Background(new SolidPaint(Primary))
											: new Spacer().Frame(height: 4) as View,
									}
									.OnTap(_ => _iconIndex.Value = idx)
									.SemanticDescription($"Icon: {(idx < IconDescriptions.Length ? IconDescriptions[idx] : "icon")}") as View;
								}).ToArray()
							}
						}
						.Frame(height: 44)
						.Margin(new Thickness(0, 0, 0, 15)),

						// Tags — matches template's chip selector
						new Text("Tags")
							.FontSize(22)
							.FontWeight(FontWeight.Semibold),
						new ScrollView(Orientation.Horizontal)
						{
							new HStack(spacing: 5)
							{
								(_store.Tags.Value ?? new List<Tag>()).Select(tag =>
								{
									var isSelected = _project.Tags.Any(t => t.ID == tag.ID);
									return new Border
									{
										Content = new Text(tag.Title)
											.FontSize(16)
											.Color(isSelected ? Colors.White : DarkOnLightBg),
									}
									.Frame(height: 44)
									.Background(new SolidPaint(isSelected ? tag.DisplayColor : LightSecondaryBg))
									.ClipShape(new RoundedRectangle(22))
									.Padding(new Thickness(18, 0))
									.OnTap(_ =>
									{
										if (isSelected)
											_project.Tags.RemoveAll(t => t.ID == tag.ID);
										else
											_project.Tags.Add(new Tag { ID = tag.ID, Title = tag.Title, ColorHex = tag.ColorHex });
										_store.SaveProject(_project);
									})
									.SemanticDescription($"{tag.Title}") as View;
								}).ToArray()
							}
						}
						.Frame(height: 44)
						.Margin(new Thickness(0, 0, 0, 15)),

						// Save button (matches template: full width, 44pt height)
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
						.Frame(height: 44)
						.IsEnabled(!string.IsNullOrWhiteSpace(_name.Value))
						.SemanticDescription("Save project"),

						// Tasks header with clean button
						new Grid
						{
							new Text("Tasks")
								.FontSize(22)
								.FontWeight(FontWeight.Semibold),
						}
						.Frame(height: 44),

						// Task list
						new VStack(spacing: 5)
						{
							tasks.Select(task => TaskRow(task) as View).ToArray()
						},
					}
					.Padding(new Thickness(15)) // LayoutPadding
				},

				// FAB for adding tasks
				new Button("+", () =>
				{
					Navigation?.Navigate(new TaskDetailPage(null, _project.ID));
				})
				.Frame(width: 60, height: 60)
				.Background(new SolidPaint(Primary))
				.Color(Colors.White)
				.FontSize(28)
				.Margin(new Thickness(30))
				.SemanticDescription("Add task"),
			}
		}
		.Title("Project");
	}
}
