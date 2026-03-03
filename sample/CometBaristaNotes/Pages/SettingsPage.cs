using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Components;

namespace CometBaristaNotes.Pages;

public class SettingsPage : Comet.View
{
	[State] readonly State<ThemeMode> _themeMode = new(ThemeMode.Auto);

	static readonly Color Primary = Color.FromArgb("#6F4E37");

	[Body]
	Comet.View body() =>
		new Comet.ScrollView
		{
			new VStack(spacing: 16)
			{
				// Appearance
				FormHelpers.SectionHeader("APPEARANCE"),
				new HStack(spacing: 12)
				{
					ThemeButton("☀️", "Light", ThemeMode.Light),
					ThemeButton("🌙", "Dark", ThemeMode.Dark),
					ThemeButton("🔄", "Auto", ThemeMode.Auto),
				},

				// Manage
				FormHelpers.SectionHeader("MANAGE"),
				SettingsItem("Equipment", "Manage machines & grinders", () =>
					Microsoft.Maui.Controls.Shell.Current.GoToAsync("equipment")),
				SettingsItem("Beans", "Manage coffee beans", () =>
					Microsoft.Maui.Controls.Shell.Current.GoToAsync("beans")),
				SettingsItem("Profiles", "Manage user profiles", () =>
					Microsoft.Maui.Controls.Shell.Current.GoToAsync("profiles")),

				// About
				FormHelpers.SectionHeader("ABOUT"),
				FormHelpers.Card(
					new VStack(spacing: 4)
					{
						new Text("Barista Notes").FontSize(18).FontWeight(FontWeight.Bold),
						new Text("v1.0 • Comet MVU Edition").FontSize(13).Color(Colors.Gray),
						new Text("Track and perfect your espresso shots.")
							.FontSize(13).Color(Colors.Gray)
							.Padding(new Thickness(0, 8, 0, 0)),
					}
				),
			}.Padding(16)
		};

	Comet.View ThemeButton(string icon, string label, ThemeMode mode)
	{
		var isSelected = _themeMode.Value == mode;
		return new VStack(spacing: 4)
		{
			new Text(icon).FontSize(24),
			new Text(label).FontSize(12).Color(isSelected ? Primary : Colors.Gray)
		}
		.Frame(width: 80, height: 64)
		.Background(isSelected ? Primary.WithAlpha(0.15f) : Color.FromArgb("#F5F5F5"))
		.ClipShape(new RoundedRectangle(12))
		.OnTap(_ => _themeMode.Value = mode);
	}

	Comet.View SettingsItem(string title, string description, Action onTap) =>
		new HStack
		{
			new VStack(spacing: 2)
			{
				new Text(title).FontSize(16).FontWeight(FontWeight.Semibold),
				new Text(description).FontSize(13).Color(Colors.Gray),
			},
			new Spacer(),
			new Text("›").FontSize(20).Color(Colors.Gray)
		}
		.Padding(16)
		.Background(Colors.White)
		.ClipShape(new RoundedRectangle(12))
		.OnTap(_ => onTap());
}
