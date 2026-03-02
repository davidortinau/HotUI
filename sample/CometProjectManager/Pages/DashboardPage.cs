using CometProjectManager.Controls;
using CometProjectManager.Models;

namespace CometProjectManager.Pages;

public class DashboardPage : View
{
[State] readonly DataStore _store = DataStore.Instance;

static readonly Color Primary = Color.FromArgb("#512BD4");
static readonly Color CardBg = Color.FromArgb("#E0E0E0");
static readonly Color DarkText = Color.FromArgb("#0D0D0D");

View ProjectCard(Project project)
{
var stack = new Microsoft.Maui.Controls.VerticalStackLayout { Spacing = 10 };
stack.Add(new Microsoft.Maui.Controls.Label
{
Text = project.Icon,
FontSize = 20,
});
stack.Add(new Microsoft.Maui.Controls.Label
{
Text = project.Name.ToUpperInvariant(),
TextColor = Color.FromArgb("#919191"),
FontSize = 14,
});
stack.Add(new Microsoft.Maui.Controls.Label
{
Text = project.Description,
TextColor = DarkText,
FontSize = 17,
LineBreakMode = Microsoft.Maui.LineBreakMode.WordWrap,
});

var border = new Microsoft.Maui.Controls.Border
{
StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(20) },
Background = new Microsoft.Maui.Controls.SolidColorBrush(CardBg),
StrokeThickness = 0,
Padding = new Thickness(15),
WidthRequest = 200,
Content = stack,
};

var tapGesture = new Microsoft.Maui.Controls.TapGestureRecognizer();
tapGesture.Tapped += (s, e) => Navigation?.Navigate(new ProjectDetailPage(project));
border.GestureRecognizers.Add(tapGesture);

return new MauiViewHost(border).Frame(width: 200, height: 220);
}

View TaskRow(ProjectTask task)
{
var grid = new Microsoft.Maui.Controls.Grid
{
ColumnSpacing = 15,
Padding = new Thickness(15),
};
grid.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Auto));
grid.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Star));

var checkBox = new Microsoft.Maui.Controls.CheckBox
{
IsChecked = task.IsCompleted,
VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
};
checkBox.CheckedChanged += (s, e) => _store.ToggleTaskComplete(task.ID);

var label = new Microsoft.Maui.Controls.Label
{
Text = task.Title,
VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
TextColor = DarkText,
FontSize = 17,
};

Microsoft.Maui.Controls.Grid.SetColumn(checkBox, 0);
Microsoft.Maui.Controls.Grid.SetColumn(label, 1);
grid.Children.Add(checkBox);
grid.Children.Add(label);

var border = new Microsoft.Maui.Controls.Border
{
StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
Background = new Microsoft.Maui.Controls.SolidColorBrush(CardBg),
StrokeThickness = 0,
Content = grid,
};

return new MauiViewHost(border).Frame(height: 60);
}

[Body]
View body()
{
var tasks = _store.AllTasks.Value ?? new List<ProjectTask>();
var projects = _store.Projects.Value ?? new List<Project>();

return new NavigationView
{
new ScrollView
{
new VStack(spacing: 10)
{
// Projects section
new Text("Projects").FontSize(22).FontWeight(FontWeight.Semibold),

new ScrollView(Orientation.Horizontal)
{
new HStack(spacing: 15)
{
projects.Select(p => ProjectCard(p) as View).ToArray()
}
.Padding(new Thickness(20, 0))
}
.Margin(new Thickness(-20, 0)),

// Tasks section
new Text("Tasks").FontSize(22).FontWeight(FontWeight.Semibold)
.Margin(new Thickness(0, 10, 0, 0)),

new VStack(spacing: 8)
{
tasks.Select(t => TaskRow(t) as View).ToArray()
},
}
.Padding(new Thickness(20, 10))
}
}
.Title(_store.Today);
}
}
