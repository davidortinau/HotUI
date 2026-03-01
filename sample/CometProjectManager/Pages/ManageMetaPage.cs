using CometProjectManager.Models;

namespace CometProjectManager.Pages;

/// <summary>
/// Manage Meta page — equivalent to the template's ManageMetaPage.
/// CRUD for categories (title + color) and tags (title + color).
/// </summary>
public class ManageMetaPage : View
{
	[State] readonly DataStore _store = DataStore.Instance;

	readonly State<string> _newCategoryTitle = new("");
	readonly State<string> _newCategoryColor = new("#2196F3");
	readonly State<string> _newTagTitle = new("");
	readonly State<string> _newTagColor = new("#9C27B0");

	[Body]
	View body()
	{
		var categories = _store.Categories.Value ?? new List<Category>();
		var tags = _store.Tags.Value ?? new List<Tag>();

		return new ScrollView
		{
			new VStack(spacing: 16)
			{
				// Categories section
				new Text("Categories")
					.FontSize(20)
					.FontWeight(FontWeight.Bold)
					.SemanticHeadingLevel(SemanticHeadingLevel.Level1),

				new VStack(spacing: 8)
				{
					categories.Select(cat => new HStack(spacing: 10)
					{
						new ShapeView(new Circle())
							.Frame(20, 20)
							.Background(new SolidPaint(cat.Color)),
						new Text(cat.Title)
							.FontSize(15),
						new Text(cat.ColorHex)
							.FontSize(12)
							.Color(Colors.Gray),
						new Spacer(),
						new Text("🗑️")
							.FontSize(14)
							.OnTap(_ => _store.DeleteCategory(cat.ID))
							.SemanticDescription($"Delete {cat.Title} category"),
					}
					.Padding(new Thickness(8, 6))
					.Background(new SolidPaint(Colors.WhiteSmoke))
					.ClipShape(new RoundedRectangle(8))
					.SemanticDescription($"Category: {cat.Title}") as View).ToArray()
				},

				// Add category form
				new HStack(spacing: 8)
				{
					new TextField(_newCategoryTitle, "New category...")
						.FontSize(14)
						.SemanticDescription("New category name"),
					new TextField(_newCategoryColor, "#hex")
						.FontSize(12)
						.Frame(width: 80)
						.SemanticDescription("Category color hex"),
					new Text("➕")
						.FontSize(18)
						.OnTap(_ =>
						{
							var title = _newCategoryTitle.Value?.Trim();
							if (!string.IsNullOrEmpty(title))
							{
								_store.AddCategory(new Category
								{
									Title = title,
									ColorHex = _newCategoryColor.Value ?? "#2196F3"
								});
								_newCategoryTitle.Value = "";
							}
						})
						.SemanticDescription("Add category"),
				}
				.Padding(new Thickness(8)),

				new Spacer().Frame(height: 12),

				// Tags section
				new Text("Tags")
					.FontSize(20)
					.FontWeight(FontWeight.Bold)
					.SemanticHeadingLevel(SemanticHeadingLevel.Level1),

				new VStack(spacing: 8)
				{
					tags.Select(tag => new HStack(spacing: 10)
					{
						new Text(tag.Title)
							.FontSize(14)
							.Color(Colors.White)
							.Background(new SolidPaint(tag.DisplayColor))
							.Padding(new Thickness(10, 4))
							.ClipShape(new RoundedRectangle(12)),
						new Text(tag.ColorHex)
							.FontSize(12)
							.Color(Colors.Gray),
						new Spacer(),
						new Text("🗑️")
							.FontSize(14)
							.OnTap(_ => _store.DeleteTag(tag.ID))
							.SemanticDescription($"Delete {tag.Title} tag"),
					}
					.Padding(new Thickness(8, 4))
					.SemanticDescription($"Tag: {tag.Title}") as View).ToArray()
				},

				// Add tag form
				new HStack(spacing: 8)
				{
					new TextField(_newTagTitle, "New tag...")
						.FontSize(14)
						.SemanticDescription("New tag name"),
					new TextField(_newTagColor, "#hex")
						.FontSize(12)
						.Frame(width: 80)
						.SemanticDescription("Tag color hex"),
					new Text("➕")
						.FontSize(18)
						.OnTap(_ =>
						{
							var title = _newTagTitle.Value?.Trim();
							if (!string.IsNullOrEmpty(title))
							{
								_store.AddTag(new Tag
								{
									Title = title,
									ColorHex = _newTagColor.Value ?? "#9C27B0"
								});
								_newTagTitle.Value = "";
							}
						})
						.SemanticDescription("Add tag"),
				}
				.Padding(new Thickness(8)),
			}
			.Padding(new Thickness(16))
		};
	}
}
