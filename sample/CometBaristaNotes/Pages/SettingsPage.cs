using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Components;

namespace CometBaristaNotes.Pages;

public class SettingsPage : Comet.View
{
	[State] readonly State<ThemeMode> _themeMode = new(ThemeMode.Auto);

	[Body]
	Comet.View body() =>
		new Comet.ScrollView
		{
			new VStack(spacing: Theme.SpacingS)
			{
				FormHelpers.SectionHeader("APPEARANCE"),
				new HStack(spacing: Theme.SpacingS)
				{
					ThemeButton("☀️", "Light", ThemeMode.Light),
					ThemeButton("🌙", "Dark", ThemeMode.Dark),
					ThemeButton("🔄", "Auto", ThemeMode.Auto),
				},

				FormHelpers.SectionHeader("MANAGE"),
				SettingsItem("Equipment", "Manage machines & grinders", () =>
					Microsoft.Maui.Controls.Shell.Current.GoToAsync("equipment")),
				SettingsItem("Beans", "Manage coffee beans", () =>
					Microsoft.Maui.Controls.Shell.Current.GoToAsync("beans")),
				SettingsItem("Profiles", "Manage user profiles", () =>
					Microsoft.Maui.Controls.Shell.Current.GoToAsync("profiles")),

				FormHelpers.SectionHeader("ABOUT"),
				FormHelpers.Card(
					new VStack(spacing: 4)
					{
						new Text("Barista Notes").FontSize(18).FontWeight(FontWeight.Bold).Color(Theme.TextPrimary),
						new Text("v1.0 • Comet MVU Edition").FontSize(14).Color(Theme.TextSecondary),
						new Text("Track and perfect your espresso shots.")
							.FontSize(14).Color(Theme.TextSecondary)
							.Padding(new Thickness(0, Theme.SpacingS, 0, 0)),
					}
				),
			}.Padding(Theme.SpacingM)
		}.Background(Theme.Background);

	Comet.View ThemeButton(string icon, string label, ThemeMode mode)
	{
		var isSelected = _themeMode.Value == mode;
		return new VStack(spacing: 4)
		{
			new Text(icon).FontSize(24),
			new Text(label).FontSize(12).Color(isSelected ? Theme.Primary : Theme.TextSecondary)
		}
		.Frame(width: 80, height: 64)
		.Background(isSelected ? Theme.Primary.WithAlpha(0.15f) : Theme.SurfaceVariant)
		.ClipShape(new RoundedRectangle(Theme.RadiusCard))
		.OnTap(_ => _themeMode.Value = mode);
	}

	Comet.View SettingsItem(string title, string description, Action onTap) =>
		new HStack
		{
			new VStack(spacing: 2)
			{
				new Text(title).FontSize(16).FontWeight(FontWeight.Semibold).Color(Theme.TextPrimary),
				new Text(description).FontSize(14).Color(Theme.TextSecondary),
			},
			new Spacer(),
			new Text("›").FontSize(20).Color(Theme.TextMuted)
		}
		.Padding(Theme.SpacingM)
		.Background(Theme.Surface)
		.RoundedBorder(radius: Theme.RadiusCard, color: Theme.Outline, strokeSize: 1)
		.OnTap(_ => onTap());
}
