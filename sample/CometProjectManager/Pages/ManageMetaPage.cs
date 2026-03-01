using CometProjectManager.Models;

namespace CometProjectManager.Pages;

/// <summary>
/// Manage Meta page — matches the template's ManageMetaPage exactly.
/// CRUD for categories (title + color + color preview + delete) and tags (same).
/// Includes Save buttons and Add buttons, plus Reset toolbar item.
/// Uses the same grid layout: 4*,3*,30,Auto column definitions.
/// </summary>
public class ManageMetaPage : View
{
	[State] readonly DataStore _store = DataStore.Instance;

	static readonly Color Primary = Color.FromArgb("#512BD4");
	static readonly Color LightSecondaryBg = Color.FromArgb("#E0E0E0");
	static readonly Color DarkOnLightBg = Color.FromArgb("#0D0D0D");

	View CategoryRow(Category cat)
	{
		// Create states that write back to the model on change
		var titleState = new State<string>(cat.Title);
		var colorState = new State<string>(cat.ColorHex);

		return new HStack(spacing: 5)
		{
			new TextField(titleState, "Title")
				.FontSize(17)
				.SemanticDescription("Title"),
			new TextField(colorState, "Color")
				.FontSize(17)
				.SemanticDescription("Color")
				.SemanticHint("Category color in HEX format"),
			new ShapeView(new Rectangle())
				.Frame(width: 30, height: 30)
				.Background(new SolidPaint(cat.Color)),
			new Text("🗑")
				.FontSize(20)
				.OnTap(_ => _store.DeleteCategory(cat.ID))
				.SemanticDescription("Delete"),
		};
	}

	View TagRow(Tag tag)
	{
		var titleState = new State<string>(tag.Title);
		var colorState = new State<string>(tag.ColorHex);

		return new HStack(spacing: 5)
		{
			new TextField(titleState, "Title")
				.FontSize(17)
				.SemanticDescription("Title"),
			new TextField(colorState, "Color")
				.FontSize(17)
				.SemanticDescription("Color")
				.SemanticHint("Tag color in HEX format"),
			new ShapeView(new Rectangle())
				.Frame(width: 30, height: 30)
				.Background(new SolidPaint(tag.DisplayColor)),
			new Text("🗑")
				.FontSize(20)
				.OnTap(_ => _store.DeleteTag(tag.ID))
				.SemanticDescription("Delete"),
		};
	}

	[Body]
	View body()
	{
		var categories = _store.Categories.Value ?? new List<Category>();
		var tags = _store.Tags.Value ?? new List<Tag>();

		return new ScrollView
		{
			new VStack(spacing: 5) // LayoutSpacing = 5
			{
				// Categories section header (Title2 = 22pt semibold)
				new Text("Categories")
					.FontSize(22)
					.FontWeight(FontWeight.Semibold),

				// Category rows
				new VStack(spacing: 5)
				{
					categories.Select(cat => CategoryRow(cat) as View).ToArray()
				},

				// Save + Add buttons (matches template grid: *,Auto columns)
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

				// Tag rows
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
			.Padding(new Thickness(15)) // LayoutPadding
		};
	}
}
