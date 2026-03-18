#pragma warning disable CS0618 // ListView is intentionally used for visual comparison
namespace MauiControlsGallery.Pages;

public partial class ListViewPage : ContentPage
{
	public ListViewPage()
	{
		InitializeComponent();

		FoodListView.ItemsSource = new[]
		{
			new { Name = "Apple", Category = "Fruit", Emoji = "🍎" },
			new { Name = "Banana", Category = "Fruit", Emoji = "🍌" },
			new { Name = "Carrot", Category = "Vegetable", Emoji = "🥕" },
			new { Name = "Broccoli", Category = "Vegetable", Emoji = "🥦" },
			new { Name = "Salmon", Category = "Protein", Emoji = "🐟" },
			new { Name = "Chicken", Category = "Protein", Emoji = "🍗" },
			new { Name = "Rice", Category = "Grain", Emoji = "🍚" },
			new { Name = "Bread", Category = "Grain", Emoji = "🍞" },
		};

		SettingsListView.ItemsSource = new[]
		{
			new { Title = "Settings", Subtitle = "Configure your preferences" },
			new { Title = "Account", Subtitle = "Manage your account details" },
			new { Title = "Privacy", Subtitle = "Review privacy settings" },
			new { Title = "Notifications", Subtitle = "Manage notification preferences" },
		};
	}
}
