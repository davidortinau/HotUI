using Comet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiBorder = Microsoft.Maui.Controls.Border;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;
using MauiFontAttributes = Microsoft.Maui.Controls.FontAttributes;

namespace CometBaristaNotes.Pages;

public class SettingsPage : Comet.View
{
	readonly IThemeService _themeService;
	[State] readonly State<AppThemeMode> _themeMode;

	public SettingsPage()
	{
		_themeService = IPlatformApplication.Current!.Services.GetRequiredService<IThemeService>();
		_themeService.LoadSavedTheme();
		_themeMode = new(_themeService.CurrentMode);
	}

	[Body]
	Comet.View body()
	{
		var stack = new VerticalStackLayout { Spacing = Theme.SpacingM, Padding = new Thickness(Theme.SpacingM) };

		// Appearance section
		stack.Add(FormHelpers.MakeSectionHeader("APPEARANCE"));
		stack.Add(BuildAppearanceButtons());

		// Manage section
		stack.Add(FormHelpers.MakeSectionHeader("MANAGE"));
		stack.Add(BuildManageItem("Equipment", "Manage machines, grinders", () =>
			Microsoft.Maui.Controls.Shell.Current.GoToAsync("equipment")));
		stack.Add(BuildManageItem("Beans", "Manage coffee beans", () =>
			Microsoft.Maui.Controls.Shell.Current.GoToAsync("beans")));
		stack.Add(BuildManageItem("User Profiles", "Manage household members", () =>
			Microsoft.Maui.Controls.Shell.Current.GoToAsync("profiles")));

		// About section
		stack.Add(FormHelpers.MakeSectionHeader("ABOUT"));
		stack.Add(BuildAboutCard());

		var scrollView = new MauiScrollView
		{
			Content = stack,
			BackgroundColor = Theme.Background,
		};

		return new MauiViewHost(scrollView);
	}

	Microsoft.Maui.Controls.View BuildAppearanceButtons()
	{
		var hStack = new HorizontalStackLayout { Spacing = Theme.SpacingS };
		hStack.Add(BuildThemeButton(Icons.LightMode, "Light", AppThemeMode.Light));
		hStack.Add(BuildThemeButton(Icons.DarkMode, "Dark", AppThemeMode.Dark));
		hStack.Add(BuildThemeButton(Icons.BrightnessAuto, "Auto", AppThemeMode.System));
		return hStack;
	}

	Microsoft.Maui.Controls.View BuildThemeButton(string icon, string label, AppThemeMode mode)
	{
		var isSelected = _themeMode.Value == mode;

		var contentStack = new VerticalStackLayout
		{
			Spacing = 4,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};
		contentStack.Add(new MauiLabel { Text = icon, FontFamily = Icons.FontFamily, FontSize = 24, HorizontalTextAlignment = TextAlignment.Center });
		contentStack.Add(new MauiLabel { Text = label, FontFamily = Theme.FontRegular, FontSize = 12, TextColor = isSelected ? Theme.Primary : Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center });

		var border = new MauiBorder
		{
			Content = contentStack,
			BackgroundColor = isSelected ? Theme.Primary.WithAlpha(0.15f) : Theme.CardBackground,
			Stroke = new SolidColorBrush(isSelected ? Theme.Primary : Theme.CardStroke),
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = Theme.RadiusCard },
			HeightRequest = 64,
			WidthRequest = 100,
			Padding = new Thickness(8),
		};

		var tap = new TapGestureRecognizer();
		tap.Tapped += (s, e) =>
		{
			_themeMode.Value = mode;
			_themeService.SetTheme(mode);
		};
		border.GestureRecognizers.Add(tap);

		return border;
	}

	Microsoft.Maui.Controls.View BuildManageItem(string title, string description, Action onTap)
	{
		return FormHelpers.MakeListCard(title, description, null, onTap);
	}

	Microsoft.Maui.Controls.View BuildAboutCard()
	{
		var stack = new VerticalStackLayout { Spacing = Theme.SpacingXS };
		stack.Add(new MauiLabel { Text = "BaristaNotes", FontFamily = Theme.FontSemibold, FontSize = 18, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary });
		stack.Add(new MauiLabel { Text = "Version 1.0", FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary });
		stack.Add(new MauiLabel { Text = "Track your espresso journey", FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary, Margin = new Thickness(0, Theme.SpacingXS, 0, 0) });

		return FormHelpers.MakeCard(stack);
	}
}
