using CometProjectManager.Models;

namespace CometProjectManager.Pages;

public class DashboardPage : View
{
    [State] readonly DataStore _store = DataStore.Instance;

    static readonly Color Primary = Color.FromArgb("#512BD4");
    static readonly Color LightSecondaryBg = Color.FromArgb("#E0E0E0");
    static readonly Color DarkOnLightBg = Color.FromArgb("#0D0D0D");

    /// <summary>
    /// Test: embed a MAUI Controls Label via MauiViewHost to verify the pipeline.
    /// </summary>
    View EmbeddedMauiLabel()
    {
        return new MauiViewHost(new Microsoft.Maui.Controls.Label
        {
            Text = "✅ Embedded MAUI Label via MauiViewHost!",
            TextColor = Microsoft.Maui.Graphics.Colors.Purple,
            FontSize = 20,
            HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
            Margin = new Thickness(0, 10),
        }).Frame(height: 50);
    }

    /// <summary>
    /// Test: embed a MAUI Controls Border with content.
    /// </summary>
    View EmbeddedMauiBorder()
    {
        var border = new Microsoft.Maui.Controls.Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(20) },
            Background = new Microsoft.Maui.Controls.SolidColorBrush(Color.FromArgb("#E0E0E0")),
            StrokeThickness = 0,
            Padding = new Thickness(15),
            Content = new Microsoft.Maui.Controls.Label
            {
                Text = "✅ Embedded MAUI Border with Content!",
                TextColor = Microsoft.Maui.Graphics.Colors.DarkBlue,
                FontSize = 16,
            }
        };
        return new MauiViewHost(border).Frame(height: 70);
    }

    /// <summary>
    /// Test: embed a MAUI Controls CheckBox.
    /// </summary>
    View EmbeddedCheckBox(ProjectTask task)
    {
        var grid = new Microsoft.Maui.Controls.Grid();
        grid.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Star));
        grid.ColumnSpacing = 10;
        grid.Padding = new Thickness(15);

        var cb = new Microsoft.Maui.Controls.CheckBox { IsChecked = task.IsCompleted };
        cb.CheckedChanged += (s, e) => _store.ToggleTaskComplete(task.ID);
        var lbl = new Microsoft.Maui.Controls.Label
        {
            Text = task.Title,
            VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
            TextColor = Color.FromArgb("#0D0D0D"),
            FontSize = 17,
        };

        Microsoft.Maui.Controls.Grid.SetColumn(cb, 0);
        Microsoft.Maui.Controls.Grid.SetColumn(lbl, 1);
        grid.Children.Add(cb);
        grid.Children.Add(lbl);

        return new MauiViewHost(grid).Frame(height: 50);
    }

    View ProjectCard(Project project)
    {
        return new Border
        {
            Content = new VStack(spacing: 15)
            {
                new Text(project.Icon).FontSize(20),
                new Text(project.Name.ToUpperInvariant()).FontSize(14).Color(Color.FromArgb("#919191")),
                new Text(project.Description).FontSize(17),
            }
            .Padding(new Thickness(15)),
        }
        .Frame(width: 200)
        .Background(new SolidPaint(LightSecondaryBg))
        .ClipShape(new RoundedRectangle(20))
        .OnTap(_ => Navigation?.Navigate(new ProjectDetailPage(project)));
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
                new VStack(spacing: 5)
                {
                    // Embedded MAUI Label test
                    EmbeddedMauiLabel(),

                    // Embedded MAUI Border test
                    EmbeddedMauiBorder(),

                    // Projects header
                    new Text("Projects").FontSize(22).FontWeight(FontWeight.Semibold),

                    // Horizontal project cards (pure Comet)
                    new ScrollView(Orientation.Horizontal)
                    {
                        new HStack(spacing: 15)
                        {
                            projects.Select(p => ProjectCard(p) as View).ToArray()
                        }
                        .Padding(new Thickness(30, 0))
                    }
                    .Margin(new Thickness(-30, 0)),

                    // Tasks header
                    new Text("Tasks").FontSize(22).FontWeight(FontWeight.Semibold).Frame(height: 44),

                    // Tasks with embedded MAUI CheckBox controls
                    new VStack(spacing: 10)
                    {
                        tasks.Select(t => EmbeddedCheckBox(t) as View).ToArray()
                    },
                }
                .Padding(new Thickness(15))
            }
        }
        .Title(_store.Today);
    }
}
