using MauiGrid = Microsoft.Maui.Controls.Grid;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiImage = Microsoft.Maui.Controls.Image;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;
using MauiBorder = Microsoft.Maui.Controls.Border;
using MauiButton = Microsoft.Maui.Controls.Button;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;

namespace CometWeather.Pages;

public class SettingsPageState { }

public class SettingsPage : Component<SettingsPageState>
{
    static readonly Color DarkBg     = Color.FromArgb("#081B25");
    static readonly Color CardBg     = Color.FromArgb("#0D2B3E");
    static readonly Color TextWhite  = Colors.White;
    static readonly Color TextGray   = Color.FromArgb("#8BA3B4");
    static readonly Color AccentBlue = Color.FromArgb("#1A6EBD");

    // Track checkmark images for imperative updates
    readonly Dictionary<string, MauiImage> _unitsChecks = new();
    readonly Dictionary<string, MauiImage> _themeChecks = new();
    string _selectedUnits = "imperial";
    string _selectedTheme = "default";

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
            Padding = new Thickness(20, 60, 20, 40),
            BackgroundColor = DarkBg,
        };

        stack.Add(BuildProfileHeader());
        stack.Add(new Microsoft.Maui.Controls.BoxView { HeightRequest = 24, BackgroundColor = Colors.Transparent });
        stack.Add(BuildSectionLabel("Units"));
        stack.Add(new Microsoft.Maui.Controls.BoxView { HeightRequest = 8, BackgroundColor = Colors.Transparent });
        stack.Add(BuildUnitsSection());
        stack.Add(new Microsoft.Maui.Controls.BoxView { HeightRequest = 24, BackgroundColor = Colors.Transparent });
        stack.Add(BuildSectionLabel("Theme"));
        stack.Add(new Microsoft.Maui.Controls.BoxView { HeightRequest = 8, BackgroundColor = Colors.Transparent });
        stack.Add(BuildThemeSection());
        stack.Add(new Microsoft.Maui.Controls.BoxView { HeightRequest = 32, BackgroundColor = Colors.Transparent });
        stack.Add(BuildSupportLink());

        return stack;
    }

    Microsoft.Maui.Controls.View BuildProfileHeader()
    {
        var card = new MauiBorder
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
            BackgroundColor = CardBg,
            StrokeThickness = 0,
            Padding = new Thickness(16),
        };

        var row = new MauiGrid();
        row.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition { Width = GridLength.Star });
        row.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition { Width = GridLength.Auto });

        var avatarBorder = new MauiBorder
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.Ellipse(),
            BackgroundColor = AccentBlue,
            StrokeThickness = 0,
            WidthRequest = 56,
            HeightRequest = 56,
            Content = new MauiLabel
            {
                Text = "JV",
                TextColor = TextWhite,
                FontSize = 18,
                FontAttributes = Microsoft.Maui.Controls.FontAttributes.Bold,
                HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center,
                VerticalTextAlignment = Microsoft.Maui.TextAlignment.Center,
            }
        };
        MauiGrid.SetColumn(avatarBorder, 0);
        row.Add(avatarBorder);

        var nameStack = new Microsoft.Maui.Controls.VerticalStackLayout
        {
            Spacing = 2,
            Margin = new Thickness(12, 0),
            VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
        };
        nameStack.Add(new MauiLabel { Text = "Gerald Versluis", TextColor = TextWhite, FontSize = 15, FontAttributes = Microsoft.Maui.Controls.FontAttributes.Bold });
        nameStack.Add(new MauiLabel { Text = "gerald@microsoft.com", TextColor = TextGray, FontSize = 12 });
        MauiGrid.SetColumn(nameStack, 1);
        row.Add(nameStack);

        var signOutBtn = new MauiButton
        {
            Text = "Sign Out",
            TextColor = TextWhite,
            BackgroundColor = Colors.Transparent,
            BorderColor = AccentBlue,
            BorderWidth = 1,
            CornerRadius = 8,
            FontSize = 12,
            Padding = new Thickness(10, 6),
            VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
        };
        MauiGrid.SetColumn(signOutBtn, 2);
        row.Add(signOutBtn);

        card.Content = row;
        return card;
    }

    MauiLabel BuildSectionLabel(string text) =>
        new MauiLabel { Text = text, TextColor = TextGray, FontSize = 13, FontAttributes = Microsoft.Maui.Controls.FontAttributes.Bold };

    Microsoft.Maui.Controls.View BuildUnitsSection()
    {
        _unitsChecks.Clear();
        var card = new MauiBorder
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
            BackgroundColor = CardBg,
            StrokeThickness = 0,
        };

        var stack = new Microsoft.Maui.Controls.VerticalStackLayout();
        stack.Add(BuildOptionRow("Imperial", "imperial", _selectedUnits, _unitsChecks, SelectUnits));
        stack.Add(BuildDivider());
        stack.Add(BuildOptionRow("Metric", "metric", _selectedUnits, _unitsChecks, SelectUnits));
        stack.Add(BuildDivider());
        stack.Add(BuildOptionRow("Hybrid", "hybrid", _selectedUnits, _unitsChecks, SelectUnits));

        card.Content = stack;
        return card;
    }

    Microsoft.Maui.Controls.View BuildThemeSection()
    {
        _themeChecks.Clear();
        var card = new MauiBorder
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
            BackgroundColor = CardBg,
            StrokeThickness = 0,
        };

        var stack = new Microsoft.Maui.Controls.VerticalStackLayout();
        stack.Add(BuildOptionRow("Default", "default", _selectedTheme, _themeChecks, SelectTheme));
        stack.Add(BuildDivider());
        stack.Add(BuildOptionRow("Dark", "dark", _selectedTheme, _themeChecks, SelectTheme));
        stack.Add(BuildDivider());
        stack.Add(BuildOptionRow("Light", "light", _selectedTheme, _themeChecks, SelectTheme));

        card.Content = stack;
        return card;
    }

    void SelectUnits(string value)
    {
        foreach (var kv in _unitsChecks)
            kv.Value.IsVisible = kv.Key == value;
        _selectedUnits = value;
    }

    void SelectTheme(string value)
    {
        foreach (var kv in _themeChecks)
            kv.Value.IsVisible = kv.Key == value;
        _selectedTheme = value;
    }

    Microsoft.Maui.Controls.View BuildOptionRow(
        string label, string value, string selected,
        Dictionary<string, MauiImage> checks, Action<string> onSelect)
    {
        var row = new MauiGrid
        {
            Padding = new Thickness(16, 14),
        };
        row.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition { Width = GridLength.Star });
        row.ColumnDefinitions.Add(new Microsoft.Maui.Controls.ColumnDefinition { Width = GridLength.Auto });

        var lbl = new MauiLabel { Text = label, TextColor = TextWhite, FontSize = 15 };
        MauiGrid.SetColumn(lbl, 0);
        row.Add(lbl);

        var checkImg = new MauiImage
        {
            Source = "checkmark_icon.png",
            HeightRequest = 18,
            WidthRequest = 18,
            IsVisible = selected == value,
            VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center,
            Aspect = Aspect.AspectFit,
        };
        checks[value] = checkImg;
        MauiGrid.SetColumn(checkImg, 1);
        row.Add(checkImg);

        var tapGesture = new Microsoft.Maui.Controls.TapGestureRecognizer();
        tapGesture.Tapped += (s, e) => onSelect(value);
        row.GestureRecognizers.Add(tapGesture);

        return row;
    }

    Microsoft.Maui.Controls.View BuildDivider() =>
        new Microsoft.Maui.Controls.BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#1E3A50"), Margin = new Thickness(16, 0) };

    Microsoft.Maui.Controls.View BuildSupportLink()
    {
        var lbl = new MauiLabel
        {
            Text = "Support",
            TextColor = AccentBlue,
            FontSize = 15,
            HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center,
        };
        var tap = new Microsoft.Maui.Controls.TapGestureRecognizer();
        tap.Tapped += (s, e) => { /* open support URL */ };
        lbl.GestureRecognizers.Add(tap);
        return lbl;
    }
}
