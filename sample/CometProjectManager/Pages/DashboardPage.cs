using CometProjectManager.Controls;
using CometProjectManager.Models;

namespace CometProjectManager.Pages;

public class DashboardPage : View
{
[State] readonly DataStore _store = DataStore.Instance;

static readonly Color Primary = Color.FromArgb("#512BD4");
static readonly Color LightSecondaryBg = Color.FromArgb("#E0E0E0");
static readonly Color DarkOnLightBg = Color.FromArgb("#0D0D0D");
static readonly Color Gray400 = Color.FromArgb("#919191");
static readonly Color LightBg = Color.FromArgb("#F2F2F2");

View TagPill(Tag tag)
{
return new Border
{
Content = new Text(tag.Title)
.FontSize(14)
.Color(LightBg),
}
.Frame(height: 32)
.Background(new SolidPaint(tag.DisplayColor))
.ClipShape(new RoundedRectangle(16))
.Padding(new Thickness(12, 0))
.Margin(new Thickness(0, 0, 8, 4));
}

View ProjectCard(Project p)
{
var cardContent = new VStack(spacing: 15)
{
new MauiViewHost(new Microsoft.Maui.Controls.Image
{
Source = new Microsoft.Maui.Controls.FontImageSource
{
Glyph = p.Icon,
FontFamily = Fonts.FluentUI.FontFamily,
Color = DarkOnLightBg,
Size = 20,
},
HeightRequest = 20,
WidthRequest = 20,
HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Start,
}).Frame(width: 20, height: 20),

new Text(p.Name.ToUpperInvariant())
.FontSize(14)
.Color(Gray400),

new Text(p.Description)
.FontSize(16)
.Color(DarkOnLightBg),

new HStack(spacing: 0)
{
p.Tags.Select(t => TagPill(t) as View).ToArray()
},
}
.Padding(new Thickness(15));

return new Border
{
Content = cardContent,
}
.Frame(width: 200)
.Background(new SolidPaint(LightSecondaryBg))
.ClipShape(new RoundedRectangle(20))
.OnTap(_ => Navigation?.Navigate(new ProjectDetailPage(p)));
}

View TaskRow(ProjectTask task)
{
return new MauiViewHost(new TaskViewControl(
task.Title,
task.IsCompleted,
isChecked => _store.ToggleTaskComplete(task.ID),
() => Navigation?.Navigate(new TaskDetailPage(task, task.ProjectID))
)).Frame(height: 60);
}

[Body]
View body()
{
var tasks = _store.AllTasks.Value ?? new List<ProjectTask>();
var projects = _store.Projects.Value ?? new List<Project>();

var chartData = _store.GetCategoryChartData();
var chartItems = chartData.Select(d => new ChartDataItem
{
Title = d.Title,
Count = d.Count,
ChartColor = d.Color,
}).ToList();

return new NavigationView
{
new Grid
{
new ScrollView
{
new VStack(spacing: 15)
{
// Category chart (Syncfusion RadialBarSeries via MauiViewHost)
new MauiViewHost(new CategoryChartControl(chartItems))
.Frame(height: 200),

// Projects header
new Text("Projects")
.FontSize(22)
.FontWeight(FontWeight.Semibold)
.Color(DarkOnLightBg),

// Horizontal scrolling project cards
new ScrollView(Orientation.Horizontal)
{
new HStack(spacing: 15)
{
projects.Select(p => ProjectCard(p) as View).ToArray()
}
.Padding(new Thickness(30, 0))
}
.Margin(new Thickness(-30, 0)),

// Tasks header with Clean button
new HStack
{
new Text("Tasks")
.FontSize(22)
.FontWeight(FontWeight.Semibold)
.Color(DarkOnLightBg),
new Spacer(),
new Button("Clean", () => _store.CleanCompletedTasks())
.Color(Primary)
.FontSize(16),
},

// Task rows
new VStack(spacing: 8)
{
tasks.Select(t => TaskRow(t) as View).ToArray()
},
}
.Padding(new Thickness(30, 15))
},

// FAB (Add button)
new MauiViewHost(new AddButtonControl(() =>
{
Navigation?.Navigate(new ProjectDetailPage(new Project()));
}))
.Frame(width: 60, height: 60),
}
}
.Title(_store.Today);
}
}
