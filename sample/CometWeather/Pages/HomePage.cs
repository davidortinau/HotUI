using MauiGrid = Microsoft.Maui.Controls.Grid;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiImage = Microsoft.Maui.Controls.Image;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;
using MauiBorder = Microsoft.Maui.Controls.Border;
using MauiBoxView = Microsoft.Maui.Controls.BoxView;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;

namespace CometWeather.Pages;

public class HomePageState { }

public class HomePage : Component<HomePageState>
{
    static readonly Color DarkBg     = Color.FromArgb("#081B25");
    static readonly Color CardBg     = Color.FromArgb("#0D2B3E");
    static readonly Color AccentBlue = Color.FromArgb("#1A6EBD");
    static readonly Color TextWhite  = Colors.White;
    static readonly Color TextGray   = Color.FromArgb("#8BA3B4");

    public override View Render()
    {
        var root = new MauiGrid { BackgroundColor = DarkBg };

        var scroll = new MauiScrollView
        {
            BackgroundColor = DarkBg,
            Content = BuildContent()
        };

        root.Add(scroll);
        return new MauiViewHost(root);
    }

    Microsoft.Maui.Controls.VerticalStackLayout BuildContent()
    {
        var stack = new Microsoft.Maui.Controls.VerticalStackLayout
        {
            Spacing = 0,
            Padding = new Thickness(0, 0, 0, 20),
            BackgroundColor = DarkBg,
        };

        stack.Add(BuildCurrentWeather());
        stack.Add(BuildNext24Hours());
        stack.Add(BuildDailyForecast());
        stack.Add(BuildMetrics());

        return stack;
    }

    Microsoft.Maui.Controls.View BuildCurrentWeather()
    {
        var grid = new MauiGrid
        {
            BackgroundColor = DarkBg,
            Padding = new Thickness(20, 60, 20, 20),
            RowSpacing = 8,
        };
        grid.RowDefinitions.Add(new Microsoft.Maui.Controls.RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new Microsoft.Maui.Controls.RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new Microsoft.Maui.Controls.RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new Microsoft.Maui.Controls.RowDefinition { Height = GridLength.Auto });

        // Location label
        var locationLabel = new MauiLabel
        {
            Text = "Redmond, WA",
            TextColor = TextWhite,
            FontSize = 22,
            FontAttributes = Microsoft.Maui.Controls.FontAttributes.Bold,
            HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center,
        };
        MauiGrid.SetRow(locationLabel, 0);
        grid.Add(locationLabel);

        // Large weather icon
        var weatherIcon = new MauiImage
        {
            Source = "fluent_weather_sunny_high_20_filled.png",
            HeightRequest = 120,
            WidthRequest = 120,
            HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
            Aspect = Aspect.AspectFit,
        };
        MauiGrid.SetRow(weatherIcon, 1);
        grid.Add(weatherIcon);

        // Temperature
        var tempLabel = new MauiLabel
        {
            Text = "70°F",
            TextColor = TextWhite,
            FontSize = 64,
            FontAttributes = Microsoft.Maui.Controls.FontAttributes.Bold,
            HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center,
        };
        MauiGrid.SetRow(tempLabel, 2);
        grid.Add(tempLabel);

        // Condition badge
        var conditionBorder = new MauiBorder
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
            BackgroundColor = AccentBlue,
            StrokeThickness = 0,
            Padding = new Thickness(16, 6),
            HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
            Content = new MauiLabel
            {
                Text = "Mostly Sunny",
                TextColor = TextWhite,
                FontSize = 14,
            }
        };
        MauiGrid.SetRow(conditionBorder, 3);
        grid.Add(conditionBorder);

        return grid;
    }

    Microsoft.Maui.Controls.View BuildNext24Hours()
    {
        var outer = new Microsoft.Maui.Controls.VerticalStackLayout
        {
            Spacing = 10,
            Padding = new Thickness(20, 16),
            BackgroundColor = DarkBg,
        };

        outer.Add(new MauiLabel
        {
            Text = "Next 24 Hours",
            TextColor = TextWhite,
            FontSize = 16,
            FontAttributes = Microsoft.Maui.Controls.FontAttributes.Bold,
        });

        var hourlyLayout = new Microsoft.Maui.Controls.HorizontalStackLayout { Spacing = 12 };
        foreach (var h in WeatherData.Hours)
        {
            hourlyLayout.Add(BuildHourlyItem(h));
        }

        outer.Add(new MauiScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
            Content = hourlyLayout,
        });

        return outer;
    }

    Microsoft.Maui.Controls.View BuildHourlyItem(Forecast h)
    {
        var card = new MauiBorder
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) },
            BackgroundColor = CardBg,
            StrokeThickness = 0,
            Padding = new Thickness(10, 12),
            WidthRequest = 70,
        };

        var stack = new Microsoft.Maui.Controls.VerticalStackLayout { Spacing = 6, HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Center };
        stack.Add(new MauiLabel
        {
            Text = h.DateTime.ToString("h tt"),
            TextColor = TextGray,
            FontSize = 11,
            HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center,
        });
        stack.Add(new MauiImage
        {
            Source = $"{h.Day.Phrase}.png",
            HeightRequest = 28,
            WidthRequest = 28,
            HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
            Aspect = Aspect.AspectFit,
        });
        stack.Add(new MauiLabel
        {
            Text = $"{h.Temperature.Minimum.Value}°",
            TextColor = TextWhite,
            FontSize = 13,
            HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center,
        });
        card.Content = stack;
        return card;
    }

    Microsoft.Maui.Controls.View BuildDailyForecast()
    {
        var outer = new Microsoft.Maui.Controls.VerticalStackLayout
        {
            Spacing = 10,
            Padding = new Thickness(20, 16),
            BackgroundColor = DarkBg,
        };

        outer.Add(new MauiLabel
        {
            Text = "Daily Forecast",
            TextColor = TextWhite,
            FontSize = 16,
            FontAttributes = Microsoft.Maui.Controls.FontAttributes.Bold,
        });

        var dailyLayout = new Microsoft.Maui.Controls.HorizontalStackLayout { Spacing = 12 };
        foreach (var d in WeatherData.Week)
        {
            dailyLayout.Add(BuildDailyItem(d));
        }

        outer.Add(new MauiScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
            Content = dailyLayout,
        });

        return outer;
    }

    Microsoft.Maui.Controls.View BuildDailyItem(Forecast d)
    {
        var card = new MauiBorder
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) },
            BackgroundColor = CardBg,
            StrokeThickness = 0,
            Padding = new Thickness(10, 12),
            WidthRequest = 80,
        };

        var stack = new Microsoft.Maui.Controls.VerticalStackLayout { Spacing = 6, HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Center };

        stack.Add(new MauiLabel
        {
            Text = d.DateTime.ToString("ddd"),
            TextColor = TextGray,
            FontSize = 11,
            HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center,
        });
        stack.Add(new MauiImage
        {
            Source = $"{d.Day.Phrase}.png",
            HeightRequest = 28,
            WidthRequest = 28,
            HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
            Aspect = Aspect.AspectFit,
        });
        stack.Add(new MauiLabel
        {
            Text = $"{d.Temperature.Maximum.Value}°",
            TextColor = TextWhite,
            FontSize = 13,
            HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center,
        });

        // Temperature bar
        var barHeight = Math.Max(4, (d.Temperature.Maximum.Value - d.Temperature.Minimum.Value) * 2);
        stack.Add(new MauiBoxView
        {
            Color = AccentBlue,
            WidthRequest = 6,
            HeightRequest = barHeight,
            CornerRadius = 3,
            HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
        });

        stack.Add(new MauiLabel
        {
            Text = $"{d.Temperature.Minimum.Value}°",
            TextColor = TextGray,
            FontSize = 11,
            HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center,
        });

        card.Content = stack;
        return card;
    }

    Microsoft.Maui.Controls.View BuildMetrics()
    {
        var grid = new MauiGrid
        {
            Padding = new Thickness(20, 16),
            ColumnSpacing = 12,
            RowSpacing = 12,
            BackgroundColor = DarkBg,
        };
        grid.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var metrics = WeatherData.Metrics;
        int rows = (int)Math.Ceiling(metrics.Count / 3.0);
        for (int r = 0; r < rows; r++)
            grid.RowDefinitions.Add(new Microsoft.Maui.Controls.RowDefinition { Height = GridLength.Auto });

        for (int i = 0; i < metrics.Count; i++)
        {
            var card = BuildMetricCard(metrics[i]);
            MauiGrid.SetColumn(card, i % 3);
            MauiGrid.SetRow(card, i / 3);
            grid.Add(card);
        }

        return grid;
    }

    Microsoft.Maui.Controls.View BuildMetricCard(Metric m)
    {
        var card = new MauiBorder
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
            BackgroundColor = CardBg,
            StrokeThickness = 0,
            Padding = new Thickness(12),
            HeightRequest = 120,
        };

        var stack = new Microsoft.Maui.Controls.VerticalStackLayout { Spacing = 4 };
        stack.Add(new MauiImage
        {
            Source = $"{m.Icon}.png",
            HeightRequest = 24,
            WidthRequest = 24,
            HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Start,
            Aspect = Aspect.AspectFit,
        });
        stack.Add(new MauiLabel
        {
            Text = m.Value,
            TextColor = TextWhite,
            FontSize = 20,
            FontAttributes = Microsoft.Maui.Controls.FontAttributes.Bold,
        });
        stack.Add(new MauiLabel
        {
            Text = m.Title,
            TextColor = TextGray,
            FontSize = 11,
        });
        stack.Add(new MauiLabel
        {
            Text = m.WeatherStation,
            TextColor = TextGray,
            FontSize = 10,
        });

        card.Content = stack;
        return card;
    }
}
