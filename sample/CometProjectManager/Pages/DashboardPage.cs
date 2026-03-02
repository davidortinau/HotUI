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
var tags = project.Tags?.Select(t => (t.Title, t.DisplayColor)).ToList()
?? new List<(string, Color)>();
return new MauiViewHost(new ProjectCardControl(
project.Icon, project.Name, project.Description, tags,
() => Navigation?.Navigate(new ProjectDetailPage(project))
)).Frame(width: 200, height: 220);
}

View TaskRow(ProjectTask task)
{
return new MauiViewHost(new TaskViewControl(
task.Title, task.IsCompleted,
isChecked => _store.ToggleTaskComplete(task.ID),
() => Navigation?.Navigate(new TaskDetailPage(task, task.ProjectID))
)).Frame(height: 60);
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
