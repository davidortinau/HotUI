using System;
using System.Collections.Generic;
using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using static Comet.CometControls;

namespace CometControlsGallery.Pages
{
	record FoodItem(string Name, string Category, string Emoji);
	record SettingsItem(string Title, string Subtitle);

	public class ListViewPageState
	{
		public string SelectedItem { get; set; } = "Selected: (none)";
	}

	public class ListViewPage : Component<ListViewPageState>
	{
		static readonly IReadOnlyList<FoodItem> FoodItems = new List<FoodItem>
		{
			new("Apple", "Fruit", "🍎"),
			new("Banana", "Fruit", "🍌"),
			new("Carrot", "Vegetable", "🥕"),
			new("Broccoli", "Vegetable", "🥦"),
			new("Salmon", "Protein", "🐟"),
			new("Chicken", "Protein", "🍗"),
			new("Rice", "Grain", "🍚"),
			new("Bread", "Grain", "🍞"),
		};

		static readonly IReadOnlyList<SettingsItem> SettingsItems = new List<SettingsItem>
		{
			new("Settings", "Configure your preferences"),
			new("Account", "Manage your account details"),
			new("Privacy", "Review privacy settings"),
			new("Notifications", "Manage notification preferences"),
		};

		public override View Render()
		{
			return GalleryPageHelpers.Scaffold("ListView",
				// ViewCell with DataTemplate
				GalleryPageHelpers.Section("ViewCell with DataTemplate",
					Border(
						new ListView<FoodItem>(() => (IReadOnlyList<FoodItem>)FoodItems)
						{
							ViewFor = item =>
								HStack(10,
									Text(item.Emoji)
										.FontSize(20)
										.Frame(width: 30, height: 30),
									VStack(2,
										Text(item.Name)
											.FontSize(14)
											.FontWeight(FontWeight.Bold),
										Text(item.Category)
											.FontSize(11)
											.Color(Colors.Gray)
									)
								)
								.Padding(new Thickness(12, 8)),
							Header = Text("Food Items")
								.FontSize(14)
								.FontWeight(FontWeight.Bold)
								.Padding(new Thickness(12, 8)),
							Footer = Text("8 items total")
								.FontSize(12)
								.Color(Colors.Gray)
								.Padding(new Thickness(12, 8)),
						}
						.OnSelected(item =>
							SetState(s => s.SelectedItem = $"Selected: {item.Name}"))
					)
					.StrokeColor(Colors.Gray)
					.StrokeThickness(1)
					.CornerRadius(8)
					.Frame(height: 350),
					Text(State.SelectedItem)
						.FontSize(14)
						.Color(Colors.DodgerBlue)
						.Padding(new Thickness(12, 8))
				),

				// TextCell ListView
				GalleryPageHelpers.Section("TextCell ListView",
					Border(
						new ListView<SettingsItem>(() => (IReadOnlyList<SettingsItem>)SettingsItems)
						{
							ViewFor = item =>
								VStack(2,
									Text(item.Title)
										.FontSize(14),
									Text(item.Subtitle)
										.FontSize(12)
										.Color(Colors.Gray)
								)
								.Padding(new Thickness(12, 8)),
						}
					)
					.StrokeColor(Colors.Gray)
					.StrokeThickness(1)
					.CornerRadius(8)
					.Frame(height: 200)
				)
			);
		}
	}
}
