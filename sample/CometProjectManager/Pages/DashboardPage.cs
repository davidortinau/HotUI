using CometProjectManager.Controls;
using CometProjectManager.Models;

namespace CometProjectManager.Pages;

public class DashboardPage : View
{
[State] readonly DataStore _store = DataStore.Instance;

static readonly Color Primary = Color.FromArgb("#512BD4");
static readonly Color CardBg = Color.FromArgb("#E0E0E0");
static readonly Color DarkText = Color.FromArgb("#0D0D0D");

[Body]
View body()
{
var tasks = _store.AllTasks.Value ?? new List<ProjectTask>();
var projects = _store.Projects.Value ?? new List<Project>();

return new NavigationView
{
new ScrollView
{
new VStack(spacing: 15)
{
new Text("Pure Comet Text Above").FontSize(20).Color(Colors.Green),

// Test 1: MAUI Label - explicit both dimensions
new MauiViewHost(new Microsoft.Maui.Controls.Label
{
Text = "MAUI Label via MauiViewHost",
TextColor = Microsoft.Maui.Graphics.Colors.White,
FontSize = 20,
BackgroundColor = Microsoft.Maui.Graphics.Colors.Purple,
HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center,
VerticalTextAlignment = Microsoft.Maui.TextAlignment.Center,
}).Frame(width: 350, height: 60),

// Test 2: MAUI Button
new MauiViewHost(new Microsoft.Maui.Controls.Button
{
Text = "MAUI Button Click Me!",
BackgroundColor = Microsoft.Maui.Graphics.Colors.DarkRed,
TextColor = Microsoft.Maui.Graphics.Colors.White,
FontSize = 18,
CornerRadius = 10,
}).Frame(width: 300, height: 50),

// Test 3: MAUI Border with content
new MauiViewHost(new Microsoft.Maui.Controls.Border
{
StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) },
BackgroundColor = Microsoft.Maui.Graphics.Colors.LightBlue,
StrokeThickness = 0,
Padding = new Thickness(20),
Content = new Microsoft.Maui.Controls.StackLayout
{
Children =
{
new Microsoft.Maui.Controls.Label
{
Text = "Inside MAUI Border",
FontSize = 18,
TextColor = Microsoft.Maui.Graphics.Colors.DarkBlue,
},
new Microsoft.Maui.Controls.Label
{
Text = "With nested content!",
FontSize = 14,
TextColor = Microsoft.Maui.Graphics.Colors.Gray,
},
}
}
}).Frame(width: 350, height: 100),

new Text("Pure Comet Text Below").FontSize(20).Color(Colors.Blue),

// Projects
new Text("Projects").FontSize(22).FontWeight(FontWeight.Semibold),
new ScrollView(Orientation.Horizontal)
{
new HStack(spacing: 15)
{
projects.Select(p =>
{
return new Border
{
Content = new VStack(spacing: 10)
{
new Text(p.Icon).FontSize(20),
new Text(p.Name.ToUpperInvariant()).FontSize(14).Color(Color.FromArgb("#919191")),
new Text(p.Description).FontSize(16),
}
.Padding(new Thickness(15)),
}
.Frame(width: 200)
.Background(new SolidPaint(CardBg))
.ClipShape(new RoundedRectangle(20))
.OnTap(_ => Navigation?.Navigate(new ProjectDetailPage(p))) as View;
}).ToArray()
}
.Padding(new Thickness(20, 0))
}
.Margin(new Thickness(-20, 0)),

// Tasks
new Text("Tasks").FontSize(22).FontWeight(FontWeight.Semibold)
.Margin(new Thickness(0, 10, 0, 0)),

new VStack(spacing: 8)
{
tasks.Select(t =>
{
// Use MauiViewHost for CheckBox + Label
return new MauiViewHost(new Microsoft.Maui.Controls.Grid
{
ColumnDefinitions =
{
new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Auto),
new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Star),
},
ColumnSpacing = 10,
BackgroundColor = Microsoft.Maui.Graphics.Colors.WhiteSmoke,
Children =
{
new Microsoft.Maui.Controls.CheckBox
{
IsChecked = t.IsCompleted,
Color = Microsoft.Maui.Graphics.Colors.Purple,
},
new Microsoft.Maui.Controls.Label
{
Text = t.Title,
VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
FontSize = 16,
}.Apply(lbl => Microsoft.Maui.Controls.Grid.SetColumn(lbl, 1)),
}
}).Frame(height: 50) as View;
}).ToArray()
},
}
.Padding(new Thickness(20, 10))
}
}
.Title(_store.Today);
}
}

static class ViewExtensions
{
public static T Apply<T>(this T view, Action<T> action) where T : Microsoft.Maui.Controls.BindableObject
{
action(view);
return view;
}
}
