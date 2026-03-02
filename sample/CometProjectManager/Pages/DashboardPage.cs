using CometProjectManager.Controls;
using CometProjectManager.Models;

using MauiGrid = Microsoft.Maui.Controls.Grid;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiImage = Microsoft.Maui.Controls.Image;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;
using MauiBorder = Microsoft.Maui.Controls.Border;
using MauiImageButton = Microsoft.Maui.Controls.ImageButton;
using FontImageSource = Microsoft.Maui.Controls.FontImageSource;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;

namespace CometProjectManager.Pages;

public class DashboardPage : View
{
[State] readonly DataStore _store = DataStore.Instance;

static readonly Color Primary = Color.FromArgb("#512BD4");
static readonly Color LightSecondaryBg = Color.FromArgb("#E0E0E0");
static readonly Color DarkOnLightBg = Color.FromArgb("#0D0D0D");
static readonly Color Gray400 = Color.FromArgb("#919191");
static readonly Color LightBg = Color.FromArgb("#F2F2F2");

MauiBorder BuildProjectCard(Project p)
{
var stack = new Microsoft.Maui.Controls.VerticalStackLayout { Spacing = 15 };

// Icon (FontImageSource)
stack.Add(new MauiImage
{
Source = new FontImageSource
{
Glyph = p.Icon,
FontFamily = Fonts.FluentUI.FontFamily,
Color = DarkOnLightBg,
Size = 20,
},
HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Start,
Aspect = Aspect.Center,
});

// Name (uppercase, gray, 14px)
stack.Add(new MauiLabel
{
Text = p.Name.ToUpperInvariant(),
TextColor = Gray400,
FontSize = 14,
});

// Description (WordWrap, default body size 17px)
stack.Add(new MauiLabel
{
Text = p.Description,
TextColor = DarkOnLightBg,
LineBreakMode = LineBreakMode.WordWrap,
});

// Tags (HorizontalStackLayout with colored pills)
var tagLayout = new Microsoft.Maui.Controls.HorizontalStackLayout { Spacing = 15 };
foreach (var tag in p.Tags)
{
tagLayout.Add(new MauiBorder
{
StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
HeightRequest = 32,
StrokeThickness = 0,
Background = new SolidColorBrush(tag.DisplayColor),
Padding = new Thickness(12, 0),
Content = new MauiLabel
{
Text = tag.Title,
TextColor = Colors.White,
FontSize = 14,
VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
VerticalTextAlignment = Microsoft.Maui.TextAlignment.Center,
}
});
}
stack.Add(tagLayout);

var card = new MauiBorder
{
StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(20) },
Background = new SolidColorBrush(LightSecondaryBg),
StrokeThickness = 0,
Padding = new Thickness(15),
WidthRequest = 200,
Content = stack,
};

var tap = new Microsoft.Maui.Controls.TapGestureRecognizer();
tap.Tapped += (s, e) => Navigation?.Navigate(new ProjectDetailPage(p));
card.GestureRecognizers.Add(tap);

return card;
}

TaskViewControl BuildTaskRow(ProjectTask task)
{
return new TaskViewControl(
task.Title,
task.IsCompleted,
_ => _store.ToggleTaskComplete(task.ID),
() => Navigation?.Navigate(new TaskDetailPage(task, task.ProjectID))
);
}

[Body]
View body()
{
var tasks = _store.AllTasks.Value ?? new System.Collections.Generic.List<ProjectTask>();
var projects = _store.Projects.Value ?? new System.Collections.Generic.List<Project>();

var chartData = _store.GetCategoryChartData();
var chartItems = chartData.Select(d => new ChartDataItem
{
Title = d.Title,
Count = d.Count,
ChartColor = d.Color,
}).ToList();

// Match MAUI reference: LayoutSpacing=5 (phone), LayoutPadding=15 (phone)
var contentStack = new Microsoft.Maui.Controls.VerticalStackLayout
{
Spacing = 5,
Padding = new Thickness(15),
};

// 1. Category chart (Margin="0, 12" matching XAML)
var chart = new CategoryChartControl(chartItems);
chart.Margin = new Thickness(0, 12);
contentStack.Add(chart);

// 2. Projects header — Title2 style: 22px, semibold
contentStack.Add(new MauiLabel
{
Text = "Projects",
FontSize = 22,
FontFamily = ".SFUI-SemiBold",
TextColor = DarkOnLightBg,
});

// 3. Horizontal scrolling project cards
var projectsHStack = new Microsoft.Maui.Controls.HorizontalStackLayout
{
Spacing = 15,
Padding = new Thickness(30, 0),
};
foreach (var p in projects)
projectsHStack.Add(BuildProjectCard(p));

contentStack.Add(new MauiScrollView
{
Orientation = ScrollOrientation.Horizontal,
Content = projectsHStack,
Margin = new Thickness(-30, 0),
});

// 4. Tasks header with clean button
var tasksHeaderGrid = new MauiGrid { HeightRequest = 44 };
tasksHeaderGrid.Add(new MauiLabel
{
Text = "Tasks",
FontSize = 22,
FontFamily = ".SFUI-SemiBold",
TextColor = DarkOnLightBg,
VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
});

bool hasCompleted = tasks.Any(t => t.IsCompleted);
if (hasCompleted)
{
var cleanButton = new MauiImageButton
{
Source = new FontImageSource
{
Glyph = Fonts.FluentUI.broom_32_regular,
FontFamily = Fonts.FluentUI.FontFamily,
Color = DarkOnLightBg,
Size = 24,
},
HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.End,
VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
HeightRequest = 44,
WidthRequest = 44,
BackgroundColor = Colors.Transparent,
BorderWidth = 0,
Aspect = Aspect.Center,
};
cleanButton.Clicked += (s, e) => _store.CleanCompletedTasks();
tasksHeaderGrid.Add(cleanButton);
}

contentStack.Add(tasksHeaderGrid);

// 5. Task rows (spacing=15 matching XAML)
var tasksStack = new Microsoft.Maui.Controls.VerticalStackLayout { Spacing = 15 };
foreach (var task in tasks)
tasksStack.Add(BuildTaskRow(task));
contentStack.Add(tasksStack);

// Root Grid overlay: ScrollView + FAB
var rootGrid = new MauiGrid();
rootGrid.Add(new MauiScrollView { Content = contentStack });

var fab = new AddButtonControl(() =>
{
Navigation?.Navigate(new ProjectDetailPage(new Project()));
});
rootGrid.Add(fab);

return new NavigationView
{
new MauiViewHost(rootGrid),
}
.Title(_store.Today);
}
}
