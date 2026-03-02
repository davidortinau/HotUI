using CometProjectManager.Controls;
using CometProjectManager.Models;
using Syncfusion.Maui.Toolkit.TextInputLayout;

namespace CometProjectManager.Pages;

/// <summary>
/// Project detail — matches the template's ProjectDetailPage exactly.
/// Delete toolbar icon, SfTextInputLayout for Name/Description/Category,
/// FluentUI icon picker, tag chips, Save button, TaskViewControl rows, AddButton FAB.
/// </summary>
public class ProjectDetailPage : View
{
	[State] readonly DataStore _store = DataStore.Instance;
	readonly Project _project;

	readonly State<int> _categoryIndex;
	readonly State<int> _iconIndex;

	static readonly Color Primary = Color.FromArgb("#512BD4");
	static readonly Color LightSecondaryBg = Color.FromArgb("#E0E0E0");
	static readonly Color DarkOnLightBg = Color.FromArgb("#0D0D0D");

	static readonly string[] Icons = { "\uea28", "\uf8fe", "\uf837", "\uf5a9", "\ue823", "\ue7ee", "\uea3a" };
	static readonly string[] IconDescriptions = { "Balance", "Education", "Fitness", "People", "Document", "Settings", "Target" };

	public ProjectDetailPage(Project project)
	{
		_project = project;

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

		// MAUI input controls — values read directly on Save, no State re-render on keystroke
		var nameEntry = new Microsoft.Maui.Controls.Entry
		{
			Text = _project.Name,
			FontSize = 17,
		};
		var descEntry = new Microsoft.Maui.Controls.Entry
		{
			Text = _project.Description,
			FontSize = 17,
		};
		var catPicker = new Microsoft.Maui.Controls.Picker
		{
			ItemsSource = categories.Select(c => c.Title).ToList(),
			SelectedIndex = _categoryIndex.Value,
		};

		return new NavigationView
		{
			new Grid
			{
				new ScrollView
				{
					new VStack(spacing: 5)
					{
						// Delete toolbar item (right-aligned, FluentUI delete icon)
						new HStack
						{
							new Spacer(),
							new MauiViewHost(new Microsoft.Maui.Controls.Image
							{
								Source = new Microsoft.Maui.Controls.FontImageSource
								{
									Glyph = Fonts.FluentUI.delete_24_regular,
									FontFamily = Fonts.FluentUI.FontFamily,
									Color = DarkOnLightBg,
									Size = 24,
								},
								HeightRequest = 24,
								WidthRequest = 24,
							}).Frame(width: 24, height: 24),
						}
						.OnTap(_ =>
						{
							_store.DeleteProject(_project.ID);
							this.Dismiss();
						})
						.SemanticDescription("Delete project"),

						// Name (SfTextInputLayout > Entry)
						new MauiViewHost(new TextInputControl("Name", nameEntry))
							.Frame(height: 60)
							.SemanticDescription("Name"),

						// Description (SfTextInputLayout > Entry)
						new MauiViewHost(new TextInputControl("Description", descEntry))
							.Frame(height: 60)
							.SemanticDescription("Description"),

						// Category (SfTextInputLayout > Picker)
						new MauiViewHost(new TextInputControl("Category", catPicker))
							.Frame(height: 60)
							.SemanticDescription("Category"),

						// Icon section header
						new Text("Icon")
							.FontSize(22)
							.FontWeight(FontWeight.Semibold),

						// Horizontal CollectionView of FluentUI icons with selection indicator
						new ScrollView(Orientation.Horizontal)
						{
							new HStack(spacing: 5)
							{
								Icons.Select((icon, idx) =>
								{
									var isSelected = _iconIndex.Value == idx;
									return new VStack(spacing: 6)
									{
										new MauiViewHost(new Microsoft.Maui.Controls.Image
										{
											Source = new Microsoft.Maui.Controls.FontImageSource
											{
												Glyph = icon,
												FontFamily = Fonts.FluentUI.FontFamily,
												Color = DarkOnLightBg,
												Size = 24,
											},
											HeightRequest = 24,
											WidthRequest = 24,
										}).Frame(width: 24, height: 24),

										// Selection indicator bar
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

						// Tags section header
						new Text("Tags")
							.FontSize(22)
							.FontWeight(FontWeight.Semibold),

						// Horizontal scrolling tag chips (toggle selected/unselected)
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

						// Save button (full width, 44pt height)
						new Button("Save", () =>
						{
							_project.Name = nameEntry.Text?.Trim() ?? "";
							_project.Description = descEntry.Text?.Trim() ?? "";

							var cats = _store.Categories.Value ?? new List<Category>();
							var catIdx = catPicker.SelectedIndex;
							if (catIdx >= 0 && catIdx < cats.Count)
								_project.CategoryID = cats[catIdx].ID;

							if (_iconIndex.Value >= 0 && _iconIndex.Value < Icons.Length)
								_project.Icon = Icons[_iconIndex.Value];

							_store.SaveProject(_project);
							this.Dismiss();
						})
						.Frame(height: 44)
						.SemanticDescription("Save project"),

						// Tasks header
						new Text("Tasks")
							.FontSize(22)
							.FontWeight(FontWeight.Semibold)
							.Margin(new Thickness(0, 10, 0, 0)),

						// Task rows (TaskViewControl via MauiViewHost)
						new VStack(spacing: 5)
						{
							tasks.Select(task =>
								new MauiViewHost(new TaskViewControl(
									task.Title,
									task.IsCompleted,
									isChecked => _store.ToggleTaskComplete(task.ID),
									() => Navigation?.Navigate(new TaskDetailPage(task, _project.ID))
								)).Frame(height: 60) as View
							).ToArray()
						},
					}
					.Padding(new Thickness(15))
				},

				// FAB for adding tasks
				new MauiViewHost(new AddButtonControl(() =>
				{
					Navigation?.Navigate(new TaskDetailPage(null, _project.ID));
				}))
				.Frame(width: 60, height: 60),
			}
		}
		.Title("Project");
	}
}
