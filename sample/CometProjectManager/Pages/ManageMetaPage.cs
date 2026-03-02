using CometProjectManager.Controls;
using CometProjectManager.Models;

namespace CometProjectManager.Pages;

/// <summary>
/// Manage Meta page — matches the template's ManageMetaPage exactly.
/// CRUD for categories and tags with Grid rows (4*,3*,30,Auto columns),
/// Save + Add buttons, Reset App toolbar item.
/// </summary>
public class ManageMetaPage : View
{
	[State] readonly DataStore _store = DataStore.Instance;

	static readonly Color Primary = Color.FromArgb("#512BD4");
	static readonly Color DarkOnLightBg = Color.FromArgb("#0D0D0D");

	View CategoryRow(Category cat)
	{
		var titleEntry = new Microsoft.Maui.Controls.Entry { Text = cat.Title, FontSize = 17 };
		titleEntry.TextChanged += (s, e) => cat.Title = e.NewTextValue;

		var colorEntry = new Microsoft.Maui.Controls.Entry { Text = cat.ColorHex, FontSize = 17 };
		colorEntry.TextChanged += (s, e) => cat.ColorHex = e.NewTextValue;

		var colorPreview = new Microsoft.Maui.Controls.BoxView
		{
			Color = cat.Color,
			HeightRequest = 30,
			WidthRequest = 30,
		};

		var deleteBtn = new Microsoft.Maui.Controls.ImageButton
		{
			Source = new Microsoft.Maui.Controls.FontImageSource
			{
				Glyph = Fonts.FluentUI.delete_24_regular,
				FontFamily = Fonts.FluentUI.FontFamily,
				Color = DarkOnLightBg,
				Size = 20,
			},
			BackgroundColor = Colors.Transparent,
		};
		deleteBtn.Clicked += (s, e) => _store.DeleteCategory(cat.ID);

		var grid = new Microsoft.Maui.Controls.Grid
		{
			ColumnDefinitions =
			{
				new Microsoft.Maui.Controls.ColumnDefinition(new GridLength(4, GridUnitType.Star)),
				new Microsoft.Maui.Controls.ColumnDefinition(new GridLength(3, GridUnitType.Star)),
				new Microsoft.Maui.Controls.ColumnDefinition(new GridLength(30, GridUnitType.Absolute)),
				new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Auto),
			},
			ColumnSpacing = 5,
		};

		Microsoft.Maui.Controls.Grid.SetColumn(colorEntry, 1);
		Microsoft.Maui.Controls.Grid.SetColumn(colorPreview, 2);
		Microsoft.Maui.Controls.Grid.SetColumn(deleteBtn, 3);

		grid.Add(titleEntry);
		grid.Add(colorEntry);
		grid.Add(colorPreview);
		grid.Add(deleteBtn);

		return new MauiViewHost(grid).Frame(height: 44);
	}

	View TagRow(Tag tag)
	{
		var titleEntry = new Microsoft.Maui.Controls.Entry { Text = tag.Title, FontSize = 17 };
		titleEntry.TextChanged += (s, e) => tag.Title = e.NewTextValue;

		var colorEntry = new Microsoft.Maui.Controls.Entry { Text = tag.ColorHex, FontSize = 17 };
		colorEntry.TextChanged += (s, e) => tag.ColorHex = e.NewTextValue;

		var colorPreview = new Microsoft.Maui.Controls.BoxView
		{
			Color = tag.DisplayColor,
			HeightRequest = 30,
			WidthRequest = 30,
		};

		var deleteBtn = new Microsoft.Maui.Controls.ImageButton
		{
			Source = new Microsoft.Maui.Controls.FontImageSource
			{
				Glyph = Fonts.FluentUI.delete_24_regular,
				FontFamily = Fonts.FluentUI.FontFamily,
				Color = DarkOnLightBg,
				Size = 20,
			},
			BackgroundColor = Colors.Transparent,
		};
		deleteBtn.Clicked += (s, e) => _store.DeleteTag(tag.ID);

		var grid = new Microsoft.Maui.Controls.Grid
		{
			ColumnDefinitions =
			{
				new Microsoft.Maui.Controls.ColumnDefinition(new GridLength(4, GridUnitType.Star)),
				new Microsoft.Maui.Controls.ColumnDefinition(new GridLength(3, GridUnitType.Star)),
				new Microsoft.Maui.Controls.ColumnDefinition(new GridLength(30, GridUnitType.Absolute)),
				new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Auto),
			},
			ColumnSpacing = 5,
		};

		Microsoft.Maui.Controls.Grid.SetColumn(colorEntry, 1);
		Microsoft.Maui.Controls.Grid.SetColumn(colorPreview, 2);
		Microsoft.Maui.Controls.Grid.SetColumn(deleteBtn, 3);

		grid.Add(titleEntry);
		grid.Add(colorEntry);
		grid.Add(colorPreview);
		grid.Add(deleteBtn);

		return new MauiViewHost(grid).Frame(height: 44);
	}

	[Body]
	View body()
	{
		var categories = _store.Categories.Value ?? new List<Category>();
		var tags = _store.Tags.Value ?? new List<Tag>();

		return new NavigationView
		{
			new ScrollView
			{
				new VStack(spacing: 5)
				{
					// Reset App (right-aligned toolbar-style button)
					new HStack
					{
						new Spacer(),
						new Button("Reset App", () => _store.ResetData())
							.Color(Primary)
							.FontSize(14)
							.SemanticDescription("Reset App"),
					},

					// Categories section header
					new Text("Categories")
						.FontSize(22)
						.FontWeight(FontWeight.Semibold),

					// Category rows (Grid: 4*,3*,30,Auto)
					new VStack(spacing: 5)
					{
						categories.Select(cat => CategoryRow(cat) as View).ToArray()
					},

					// Save + Add buttons
					new HStack(spacing: 5)
					{
						new Button("Save", () =>
						{
							_store.SaveCategories(categories);
						})
						.Frame(height: 44)
						.SemanticDescription("Save categories"),
						new Button("+", () =>
						{
							_store.AddCategory(new Category
							{
								Title = "New Category",
								ColorHex = "#808080"
							});
						})
						.SemanticDescription("Add category"),
					}
					.Margin(new Thickness(0, 10)),

					// Tags section header
					new Text("Tags")
						.FontSize(22)
						.FontWeight(FontWeight.Semibold),

					// Tag rows (Grid: 4*,3*,30,Auto)
					new VStack(spacing: 5)
					{
						tags.Select(tag => TagRow(tag) as View).ToArray()
					},

					// Save + Add buttons
					new HStack(spacing: 5)
					{
						new Button("Save", () =>
						{
							_store.SaveTags(tags);
						})
						.Frame(height: 44)
						.SemanticDescription("Save tags"),
						new Button("+", () =>
						{
							_store.AddTag(new Tag
							{
								Title = "New Tag",
								ColorHex = "#808080"
							});
						})
						.SemanticDescription("Add tag"),
					}
					.Margin(new Thickness(0, 10)),
				}
				.Padding(new Thickness(15))
			}
		}
		.Title("Categories and Tags");
	}
}
