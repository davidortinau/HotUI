using Microsoft.Extensions.DependencyInjection;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

namespace CometBaristaNotes.Pages;

public class SettingsPageState
{
	public AppThemeMode ThemeMode { get; set; }
}

public class SettingsPage : Component<SettingsPageState>
{
	readonly IThemeService _themeService;

	public SettingsPage()
	{
		_themeService = IPlatformApplication.Current!.Services.GetRequiredService<IThemeService>();
		_themeService.LoadSavedTheme();
	}

	public override View Render()
	{
		if (State.ThemeMode == default && _themeService.CurrentMode != default)
			SetState(s => s.ThemeMode = _themeService.CurrentMode);

		return ScrollView(
			VStack(Theme.SpacingM,
				FormHelpers.MakeSectionHeader("APPEARANCE"),
				BuildAppearanceButtons(),
				FormHelpers.MakeSectionHeader("MANAGE"),
				BuildManageItem("Equipment", "Manage machines, grinders", () =>
					Navigation?.Navigate(new EquipmentManagementPage())),
				BuildManageItem("Beans", "Manage coffee beans", () =>
					Navigation?.Navigate(new BeanManagementPage())),
				BuildManageItem("User Profiles", "Manage household members", () =>
					Navigation?.Navigate(new UserProfileManagementPage())),
				FormHelpers.MakeSectionHeader("ABOUT"),
				BuildAboutCard()
			)
			.Padding(new Thickness(Theme.SpacingM))
		)
		.Background(Theme.Background);
	}

	View BuildAppearanceButtons() =>
		HStack(Theme.SpacingS,
			BuildThemeButton(Icons.LightMode, "Light", AppThemeMode.Light),
			BuildThemeButton(Icons.DarkMode, "Dark", AppThemeMode.Dark),
			BuildThemeButton(Icons.BrightnessAuto, "Auto", AppThemeMode.System)
		);

	View BuildThemeButton(string icon, string label, AppThemeMode mode)
	{
		var isSelected = State.ThemeMode == mode;
		return Border(
			VStack(4,
				Text(icon)
					.FontFamily(Icons.FontFamily)
					.FontSize(24)
					.HorizontalTextAlignment(TextAlignment.Center),
				Text(label)
					.FontFamily(Theme.FontRegular)
					.FontSize(12)
					.Color(isSelected ? Theme.Primary : Theme.TextSecondary)
					.HorizontalTextAlignment(TextAlignment.Center)
			)
		)
		.CornerRadius(Theme.RadiusCard)
		.Background(isSelected ? Theme.Primary.WithAlpha(0.15f) : Theme.CardBackground)
		.StrokeColor(isSelected ? Theme.Primary : Theme.CardStroke)
		.StrokeThickness(1)
		.Frame(width: 100, height: 64)
		.Padding(new Thickness(8))
		.OnTap(_ => {
			SetState(s => s.ThemeMode = mode);
			_themeService.SetTheme(mode);
		});
	}

	View BuildManageItem(string title, string description, Action onTap) =>
		FormHelpers.MakeListCard(title, description, null, onTap);

	View BuildAboutCard() =>
		FormHelpers.MakeCard(
			VStack(Theme.SpacingXS,
				Text("BaristaNotes")
					.FontFamily(Theme.FontSemibold)
					.FontSize(18)
					.FontWeight(FontWeight.Bold)
					.Color(Theme.TextPrimary),
				Text("Version 1.0")
					.FontFamily(Theme.FontRegular)
					.FontSize(14)
					.Color(Theme.TextSecondary),
				Text("Track your espresso journey")
					.FontFamily(Theme.FontRegular)
					.FontSize(14)
					.Color(Theme.TextSecondary)
					.Margin(new Thickness(0, Theme.SpacingXS, 0, 0))
			)
		);
}
