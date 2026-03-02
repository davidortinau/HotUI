using CometProjectManager.Models;
using CometProjectManager.Controls;

namespace CometProjectManager.Pages;

/// <summary>
/// Dashboard page — uses MauiViewHost to embed Syncfusion chart and
/// MAUI Controls for task rows and project cards.
/// </summary>
public class DashboardPage : View
{
    [State] readonly DataStore _store = DataStore.Instance;

    View CategoryChart()
    {
        var data = _store.GetCategoryChartData();
        var chartData = data.Select(d => new ChartDataItem
        {
            Title = d.Title,
            Count = d.Count,
            ChartColor = d.Color,
        }).ToList();

        return new MauiViewHost(new CategoryChartControl(chartData))
            .Frame(height: 220);
    }

    View ProjectCard(Project project)
    {
        var tags = project.Tags.Select(t => (t.Title, t.DisplayColor)).ToList();
        return new MauiViewHost(new ProjectCardControl(
            project.Icon, project.Name, project.Description, tags,
            () => Navigation?.Navigate(new ProjectDetailPage(project))
        )).Frame(width: 200, height: 200);
    }

    View TaskRow(ProjectTask task)
    {
        return new MauiViewHost(new TaskViewControl(
            task.Title, task.IsCompleted,
            isOn => _store.ToggleTaskComplete(task.ID),
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
            new Grid
            {
                new ScrollView
                {
                    new VStack(spacing: 5)
                    {
                        CategoryChart(),

                        new Text("Projects")
                            .FontSize(22).FontWeight(FontWeight.Semibold),

                        new ScrollView(Orientation.Horizontal)
                        {
                            new HStack(spacing: 15)
                            {
                                projects.Select(p => ProjectCard(p) as View).ToArray()
                            }
                            .Padding(new Thickness(30, 0))
                        }
                        .Margin(new Thickness(-30, 0))
                        .Frame(height: 200),

                        new Text("Tasks")
                            .FontSize(22).FontWeight(FontWeight.Semibold)
                            .Frame(height: 44),

                        new VStack(spacing: 15)
                        {
                            tasks.Select(t => TaskRow(t) as View).ToArray()
                        },
                    }
                    .Padding(new Thickness(15))
                },

                new MauiViewHost(new AddButtonControl(
                    () => Navigation?.Navigate(new TaskDetailPage(null, 0))
                )).Frame(width: 60, height: 60),
            }
        }
        .Title(_store.Today);
    }
}
