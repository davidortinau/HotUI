using MauiGrid = Microsoft.Maui.Controls.Grid;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiImage = Microsoft.Maui.Controls.Image;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;
using MauiBorder = Microsoft.Maui.Controls.Border;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;

namespace CometWeather.Pages;

public class FavoritesPageState { }

public class FavoritesPage : Component<FavoritesPageState>
{
    static readonly Color DarkBg    = Color.FromArgb("#081B25");
    static readonly Color CardBg    = Color.FromArgb("#0D2B3E");
    static readonly Color TextWhite = Colors.White;
    static readonly Color TextGray  = Color.FromArgb("#8BA3B4");
    static readonly Color AccentBlue = Color.FromArgb("#1A6EBD");

    public override View Render()
    {
        var root = new MauiGrid { BackgroundColor = DarkBg };
        root.RowDefinitions.Add(new Microsoft.Maui.Controls.RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new Microsoft.Maui.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Search header
        var searchBar = BuildSearchHeader();
        MauiGrid.SetRow(searchBar, 0);
        root.Add(searchBar);

        // Favorites grid
        var collectionGrid = BuildFavoritesGrid();
        MauiGrid.SetRow(collectionGrid, 1);
        root.Add(collectionGrid);

        return new MauiViewHost(root);
    }

    Microsoft.Maui.Controls.View BuildSearchHeader()
    {
        var header = new MauiBorder
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
            BackgroundColor = CardBg,
            StrokeThickness = 0,
            Margin = new Thickness(20, 60, 20, 10),
            Padding = new Thickness(16, 12),
        };

        var row = new Microsoft.Maui.Controls.HorizontalStackLayout { Spacing = 10 };
        row.Add(new MauiImage
        {
            Source = "search_icon.png",
            HeightRequest = 20,
            WidthRequest = 20,
            VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
            Aspect = Aspect.AspectFit,
        });
        row.Add(new MauiLabel
        {
            Text = "Search",
            TextColor = TextGray,
            FontSize = 16,
            VerticalTextAlignment = Microsoft.Maui.TextAlignment.Center,
        });

        header.Content = row;
        return header;
    }

    Microsoft.Maui.Controls.View BuildFavoritesGrid()
    {
        var scroll = new MauiScrollView
        {
            BackgroundColor = DarkBg,
        };

        var outerPadding = new Microsoft.Maui.Controls.VerticalStackLayout
        {
            Padding = new Thickness(20, 0, 20, 20),
            Spacing = 0,
        };

        // Build 2-column grid manually
        var locations = WeatherData.Locations;
        var rows = (int)Math.Ceiling(locations.Count / 2.0);

        var grid = new MauiGrid
        {
            ColumnSpacing = 12,
            RowSpacing = 12,
            BackgroundColor = DarkBg,
        };
        grid.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (int r = 0; r < rows; r++)
            grid.RowDefinitions.Add(new Microsoft.Maui.Controls.RowDefinition { Height = GridLength.Auto });

        for (int i = 0; i < locations.Count; i++)
        {
            var card = BuildLocationCard(locations[i]);
            MauiGrid.SetColumn(card, i % 2);
            MauiGrid.SetRow(card, i / 2);
            grid.Add(card);
        }

        outerPadding.Add(grid);
        scroll.Content = outerPadding;
        return scroll;
    }

    Microsoft.Maui.Controls.View BuildLocationCard(Location loc)
    {
        var card = new MauiBorder
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
            BackgroundColor = CardBg,
            StrokeThickness = 0,
            Padding = new Thickness(14),
            HeightRequest = 130,
        };

        var stack = new Microsoft.Maui.Controls.VerticalStackLayout { Spacing = 4 };

        // Icon + value row
        var topRow = new MauiGrid();
        topRow.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition { Width = GridLength.Star });
        topRow.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition { Width = GridLength.Auto });

        var icon = new MauiImage
        {
            Source = $"{loc.Icon}.png",
            HeightRequest = 32,
            WidthRequest = 32,
            VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Start,
            Aspect = Aspect.AspectFit,
        };
        MauiGrid.SetColumn(icon, 0);
        topRow.Add(icon);

        var valueLabel = new MauiLabel
        {
            Text = loc.Value,
            TextColor = TextWhite,
            FontSize = 22,
            FontAttributes = Microsoft.Maui.Controls.FontAttributes.Bold,
            HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.End,
            VerticalTextAlignment = Microsoft.Maui.TextAlignment.Start,
        };
        MauiGrid.SetColumn(valueLabel, 1);
        topRow.Add(valueLabel);

        stack.Add(topRow);

        stack.Add(new MauiLabel
        {
            Text = loc.Name,
            TextColor = TextWhite,
            FontSize = 14,
            FontAttributes = Microsoft.Maui.Controls.FontAttributes.Bold,
        });

        stack.Add(new MauiLabel
        {
            Text = loc.WeatherStation,
            TextColor = TextGray,
            FontSize = 11,
        });

        card.Content = stack;
        return card;
    }
}
