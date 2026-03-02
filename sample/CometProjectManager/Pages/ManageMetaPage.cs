using CometProjectManager.Controls;
using CometProjectManager.Models;

using MauiGrid = Microsoft.Maui.Controls.Grid;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiButton = Microsoft.Maui.Controls.Button;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;
using MauiEntry = Microsoft.Maui.Controls.Entry;
using MauiBoxView = Microsoft.Maui.Controls.BoxView;
using MauiImageButton = Microsoft.Maui.Controls.ImageButton;
using ColumnDefinition = Microsoft.Maui.Controls.ColumnDefinition;
using FontImageSource = Microsoft.Maui.Controls.FontImageSource;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;

namespace CometProjectManager.Pages;

/// <summary>
/// Manage Meta page — matches the template's ManageMetaPage exactly.
/// Uses MauiViewHost for the entire content to ensure pixel-perfect match.
/// </summary>
public class ManageMetaPage : View
{
[State] readonly DataStore _store = DataStore.Instance;

static readonly Color Primary = Color.FromArgb("#512BD4");
static readonly Color DarkOnLightBg = Color.FromArgb("#0D0D0D");
static readonly Color LightBg = Color.FromArgb("#F2F2F2");

MauiGrid BuildCategoryRow(Category cat)
{
var titleEntry = new MauiEntry
{
Text = cat.Title,
FontSize = 16,
TextColor = DarkOnLightBg,
BackgroundColor = Colors.Transparent,
FontFamily = "OpenSansRegular",
MinimumHeightRequest = 44,
MinimumWidthRequest = 44,
};
titleEntry.TextChanged += (s, e) => cat.Title = e.NewTextValue;

var colorEntry = new MauiEntry
{
Text = cat.ColorHex,
FontSize = 16,
TextColor = DarkOnLightBg,
BackgroundColor = Colors.Transparent,
FontFamily = "OpenSansRegular",
MinimumHeightRequest = 44,
MinimumWidthRequest = 44,
};
colorEntry.TextChanged += (s, e) => cat.ColorHex = e.NewTextValue;

var colorPreview = new MauiBoxView
{
Color = cat.Color,
HeightRequest = 30,
WidthRequest = 30,
VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
};

var deleteBtn = new MauiButton
{
ImageSource = new FontImageSource
{
Glyph = Fonts.FluentUI.delete_32_regular,
FontFamily = Fonts.FluentUI.FontFamily,
Color = DarkOnLightBg,
Size = 20,
},
BackgroundColor = Colors.Transparent,
};
deleteBtn.Clicked += (s, e) => _store.DeleteCategory(cat.ID);

var grid = new MauiGrid
{
ColumnDefinitions =
{
new ColumnDefinition(new GridLength(4, GridUnitType.Star)),
new ColumnDefinition(new GridLength(3, GridUnitType.Star)),
new ColumnDefinition(new GridLength(30, GridUnitType.Absolute)),
new ColumnDefinition(GridLength.Auto),
},
ColumnSpacing = 5,
};

MauiGrid.SetColumn(titleEntry, 0);
MauiGrid.SetColumn(colorEntry, 1);
MauiGrid.SetColumn(colorPreview, 2);
MauiGrid.SetColumn(deleteBtn, 3);

grid.Add(titleEntry);
grid.Add(colorEntry);
grid.Add(colorPreview);
grid.Add(deleteBtn);

return grid;
}

MauiGrid BuildTagRow(Tag tag)
{
var titleEntry = new MauiEntry
{
Text = tag.Title,
FontSize = 16,
TextColor = DarkOnLightBg,
BackgroundColor = Colors.Transparent,
FontFamily = "OpenSansRegular",
MinimumHeightRequest = 44,
MinimumWidthRequest = 44,
};
titleEntry.TextChanged += (s, e) => tag.Title = e.NewTextValue;

var colorEntry = new MauiEntry
{
Text = tag.ColorHex,
FontSize = 16,
TextColor = DarkOnLightBg,
BackgroundColor = Colors.Transparent,
FontFamily = "OpenSansRegular",
MinimumHeightRequest = 44,
MinimumWidthRequest = 44,
};
colorEntry.TextChanged += (s, e) => tag.ColorHex = e.NewTextValue;

var colorPreview = new MauiBoxView
{
Color = tag.DisplayColor,
HeightRequest = 30,
WidthRequest = 30,
VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
};

var deleteBtn = new MauiButton
{
ImageSource = new FontImageSource
{
Glyph = Fonts.FluentUI.delete_32_regular,
FontFamily = Fonts.FluentUI.FontFamily,
Color = DarkOnLightBg,
Size = 20,
},
BackgroundColor = Colors.Transparent,
};
deleteBtn.Clicked += (s, e) => _store.DeleteTag(tag.ID);

var grid = new MauiGrid
{
ColumnDefinitions =
{
new ColumnDefinition(new GridLength(4, GridUnitType.Star)),
new ColumnDefinition(new GridLength(3, GridUnitType.Star)),
new ColumnDefinition(new GridLength(30, GridUnitType.Absolute)),
new ColumnDefinition(GridLength.Auto),
},
ColumnSpacing = 5,
};

MauiGrid.SetColumn(titleEntry, 0);
MauiGrid.SetColumn(colorEntry, 1);
MauiGrid.SetColumn(colorPreview, 2);
MauiGrid.SetColumn(deleteBtn, 3);

grid.Add(titleEntry);
grid.Add(colorEntry);
grid.Add(colorPreview);
grid.Add(deleteBtn);

return grid;
}

[Body]
View body()
{
var categories = _store.Categories.Value ?? new List<Category>();
var tags = _store.Tags.Value ?? new List<Tag>();

// Build entire page with MAUI Controls for pixel-perfect match
var contentStack = new Microsoft.Maui.Controls.VerticalStackLayout
{
// LayoutSpacing=5, LayoutPadding=15 (phone values)
Spacing = 5,
Padding = new Thickness(15),
};

// Categories header
contentStack.Add(new MauiLabel
{
Text = "Categories",
FontSize = 22,
FontFamily = ".SFUI-SemiBold",
TextColor = DarkOnLightBg,
});

// Category rows
var catStack = new Microsoft.Maui.Controls.VerticalStackLayout { Spacing = 5 };
foreach (var cat in categories)
catStack.Add(BuildCategoryRow(cat));
contentStack.Add(catStack);

// Save + Add buttons for categories
var catButtonGrid = new MauiGrid
{
ColumnDefinitions =
{
new ColumnDefinition(GridLength.Star),
new ColumnDefinition(GridLength.Auto),
},
ColumnSpacing = 5,
Margin = new Thickness(0, 10),
};

var saveCatBtn = new MauiButton
{
Text = "Save",
HeightRequest = 44,
};
saveCatBtn.Clicked += (s, e) => _store.SaveCategories(categories);
MauiGrid.SetColumn(saveCatBtn, 0);
catButtonGrid.Add(saveCatBtn);

var addCatBtn = new MauiButton
{
ImageSource = new FontImageSource
{
Glyph = Fonts.FluentUI.add_32_regular,
FontFamily = Fonts.FluentUI.FontFamily,
Color = Colors.White,
Size = 20,
},
};
addCatBtn.Clicked += (s, e) => _store.AddCategory(new Category
{
Title = "New Category",
ColorHex = "#808080"
});
MauiGrid.SetColumn(addCatBtn, 1);
catButtonGrid.Add(addCatBtn);

contentStack.Add(catButtonGrid);

// Tags header
contentStack.Add(new MauiLabel
{
Text = "Tags",
FontSize = 22,
FontFamily = ".SFUI-SemiBold",
TextColor = DarkOnLightBg,
});

// Tag rows
var tagStack = new Microsoft.Maui.Controls.VerticalStackLayout { Spacing = 5 };
foreach (var tag in tags)
tagStack.Add(BuildTagRow(tag));
contentStack.Add(tagStack);

// Save + Add buttons for tags
var tagButtonGrid = new MauiGrid
{
ColumnDefinitions =
{
new ColumnDefinition(GridLength.Star),
new ColumnDefinition(GridLength.Auto),
},
ColumnSpacing = 5,
Margin = new Thickness(0, 10),
};

var saveTagBtn = new MauiButton
{
Text = "Save",
HeightRequest = 44,
};
saveTagBtn.Clicked += (s, e) => _store.SaveTags(tags);
MauiGrid.SetColumn(saveTagBtn, 0);
tagButtonGrid.Add(saveTagBtn);

var addTagBtn = new MauiButton
{
ImageSource = new FontImageSource
{
Glyph = Fonts.FluentUI.add_32_regular,
FontFamily = Fonts.FluentUI.FontFamily,
Color = Colors.White,
Size = 20,
},
};
addTagBtn.Clicked += (s, e) => _store.AddTag(new Tag
{
Title = "New Tag",
ColorHex = "#808080"
});
MauiGrid.SetColumn(addTagBtn, 1);
tagButtonGrid.Add(addTagBtn);

contentStack.Add(tagButtonGrid);

return new NavigationView
{
new MauiViewHost(new MauiScrollView { Content = contentStack, BackgroundColor = LightBg }),
}
.Title("Categories and Tags")
.Background(LightBg);
}
}
