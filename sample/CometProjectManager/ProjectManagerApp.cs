using CometProjectManager.Pages;
using Syncfusion.Maui.Toolkit.Hosting;

namespace CometProjectManager;

public class ProjectManagerApp : CometApp
{
	// Support launch argument --page=X for screenshot testing
	static string? _forcePage = null;
	public static void SetForcePage(string page) => _forcePage = page;

	readonly State<string> _currentPage = "Dashboard";

	void ShowFlyoutMenu()
	{
		var menuView = new FlyoutMenuView(
			_currentPage.Value,
			(selected) =>
			{
				_currentPage.Value = selected;
				ModalView.Dismiss();
			});
		ModalView.Present(menuView);
	}

	[Body]
	View body()
	{
		if (_forcePage != null)
		{
			var store = DataStore.Instance;
			var firstProject = store.Projects.Value?.FirstOrDefault();
			var firstTask = store.AllTasks.Value?.FirstOrDefault();
			return _forcePage switch
			{
				"dashboard" => new DashboardPage(),
				"projects" => new ProjectListPage(),
				"manage" => new ManageMetaPage(),
				"projectdetail" => new ProjectDetailPage(firstProject ?? new CometProjectManager.Models.Project()),
				"taskdetail" => new TaskDetailPage(firstTask, firstTask?.ProjectID ?? 1),
				_ => new DashboardPage(),
			};
		}

		return _currentPage.Value switch
		{
			"Projects" => new ProjectListPage(ShowFlyoutMenu),
			"Manage Meta" => new ManageMetaPage(ShowFlyoutMenu),
			_ => new DashboardPage(ShowFlyoutMenu),
		};
	}
}

/// <summary>
/// Flyout menu view matching MAUI Shell flyout appearance
/// </summary>
public class FlyoutMenuView : View
{
	readonly string _selectedPage;
	readonly Action<string> _onSelect;

	public FlyoutMenuView(string selectedPage, Action<string> onSelect)
	{
		_selectedPage = selectedPage;
		_onSelect = onSelect;
	}

	static readonly (string Title, string Icon)[] MenuItems = new[]
	{
		("Dashboard", "\uf246"),
		("Projects", "\uf4e4"),
		("Manage Meta", "\uf6fa"),
	};

	[Body]
	View body()
	{
		var stack = new Microsoft.Maui.Controls.VerticalStackLayout
		{
			Spacing = 0,
			BackgroundColor = Microsoft.Maui.Graphics.Colors.White,
			WidthRequest = 300,
			VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Fill,
		};

		var header = new Microsoft.Maui.Controls.Label
		{
			Text = "Project Manager",
			FontSize = 20,
			FontFamily = "SegoeSemibold",
			Padding = new Microsoft.Maui.Thickness(20, 50, 20, 20),
		};
		stack.Add(header);

		stack.Add(new Microsoft.Maui.Controls.BoxView
		{
			HeightRequest = 1,
			Color = Microsoft.Maui.Graphics.Color.FromArgb("#E0E0E0"),
		});

		foreach (var (title, icon) in MenuItems)
		{
			var isSelected = title == _selectedPage;
			var row = new Microsoft.Maui.Controls.Grid
			{
				Padding = new Microsoft.Maui.Thickness(20, 14),
				ColumnDefinitions = new Microsoft.Maui.Controls.ColumnDefinitionCollection
				{
					new Microsoft.Maui.Controls.ColumnDefinition(new Microsoft.Maui.GridLength(36)),
					new Microsoft.Maui.Controls.ColumnDefinition(Microsoft.Maui.GridLength.Star),
				},
				BackgroundColor = isSelected
					? Microsoft.Maui.Graphics.Color.FromArgb("#E8E8E8")
					: Microsoft.Maui.Graphics.Colors.Transparent,
			};

			var iconLabel = new Microsoft.Maui.Controls.Label
			{
				Text = icon,
				FontFamily = Fonts.FluentUI.FontFamily,
				FontSize = 22,
				VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
			};
			row.Add(iconLabel);

			var titleLabel = new Microsoft.Maui.Controls.Label
			{
				Text = title,
				FontSize = 16,
				VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
				FontAttributes = isSelected ? Microsoft.Maui.Controls.FontAttributes.Bold : Microsoft.Maui.Controls.FontAttributes.None,
			};
			Microsoft.Maui.Controls.Grid.SetColumn(titleLabel, 1);
			row.Add(titleLabel);

			var t = title;
			var tap = new Microsoft.Maui.Controls.TapGestureRecognizer();
			tap.Tapped += (s, e) => _onSelect(t);
			row.GestureRecognizers.Add(tap);
			stack.Add(row);
		}

		return new MauiViewHost(stack);
	}
}

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var args = System.Environment.GetCommandLineArgs();
		foreach (var arg in args)
		{
			if (arg.StartsWith("--page=", System.StringComparison.OrdinalIgnoreCase))
			{
				ProjectManagerApp.SetForcePage(arg.Substring(7).ToLowerInvariant());
			}
		}

		var builder = MauiApp.CreateBuilder();
		builder.UseCometApp<ProjectManagerApp>()
			.ConfigureSyncfusionToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
				fonts.AddFont("FluentSystemIcons-Regular.ttf", Fonts.FluentUI.FontFamily);
			});
		return builder.Build();
	}
}
