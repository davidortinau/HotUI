using CometProjectManager.Pages;
using Syncfusion.Maui.Toolkit.Hosting;

namespace CometProjectManager;

public class ProjectManagerApp : CometApp
{
	[Body]
	View body()
	{
		return new ScrollView
		{
			new VStack(spacing: 15)
			{
				// 1. Pure Comet text
				new Text("Pure Comet Controls").FontSize(28).FontWeight(FontWeight.Bold).Color(Colors.Black),
				new Text("These are standard Comet MVU controls:").FontSize(16).Color(Colors.DarkGray),

				// 2. Embedded MAUI Label via MauiViewHost
				new Text("Embedded MAUI Controls via MauiViewHost:").FontSize(16).Color(Colors.DarkGray),
				new MauiViewHost(new Microsoft.Maui.Controls.Label
				{
					Text = "✅ MAUI Label (Purple)",
					TextColor = Microsoft.Maui.Graphics.Colors.Purple,
					FontSize = 22,
					HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
				}).Frame(height: 40),

				// 3. Embedded MAUI Border with content
				new MauiViewHost(CreateMauiBorder()).Frame(height: 80),

				// 4. Embedded MAUI Grid with CheckBox + Label
				new MauiViewHost(CreateMauiCheckboxRow("Task 1: Build Comet")).Frame(height: 50),
				new MauiViewHost(CreateMauiCheckboxRow("Task 2: Test MauiViewHost")).Frame(height: 50),

				// 5. Pure Comet Button
				new Button("Pure Comet Button", () => { }).Color(Colors.White).Background(new SolidPaint(Color.FromArgb("#512BD4"))).Frame(height: 44),

				// 6. Embedded MAUI Button
				new MauiViewHost(new Microsoft.Maui.Controls.Button
				{
					Text = "MAUI Button",
					BackgroundColor = Color.FromArgb("#2B0B98"),
					TextColor = Microsoft.Maui.Graphics.Colors.White,
					CornerRadius = 8,
				}).Frame(height: 50),
			}
			.Padding(new Thickness(20, 60, 20, 20))
		};
	}

	static Microsoft.Maui.Controls.Border CreateMauiBorder()
	{
		return new Microsoft.Maui.Controls.Border
		{
			StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
			Background = new Microsoft.Maui.Controls.SolidColorBrush(Color.FromArgb("#E8DEF8")),
			StrokeThickness = 0,
			Padding = new Thickness(16),
			Content = new Microsoft.Maui.Controls.VerticalStackLayout
			{
				Children =
				{
					new Microsoft.Maui.Controls.Label
					{
						Text = "✅ MAUI Border with Content",
						TextColor = Color.FromArgb("#1D1B20"),
						FontSize = 18,
						FontAttributes = Microsoft.Maui.Controls.FontAttributes.Bold,
					},
					new Microsoft.Maui.Controls.Label
					{
						Text = "Nested MAUI controls inside Comet!",
						TextColor = Color.FromArgb("#49454F"),
						FontSize = 14,
					}
				}
			}
		};
	}

	static Microsoft.Maui.Controls.Grid CreateMauiCheckboxRow(string text)
	{
		var grid = new Microsoft.Maui.Controls.Grid();
		grid.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Auto));
		grid.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Star));
		grid.ColumnSpacing = 10;

		var cb = new Microsoft.Maui.Controls.CheckBox { Color = Color.FromArgb("#512BD4") };
		var lbl = new Microsoft.Maui.Controls.Label
		{
			Text = text,
			VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
			FontSize = 17,
		};

		Microsoft.Maui.Controls.Grid.SetColumn(cb, 0);
		Microsoft.Maui.Controls.Grid.SetColumn(lbl, 1);
		grid.Children.Add(cb);
		grid.Children.Add(lbl);
		return grid;
	}
}

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
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
