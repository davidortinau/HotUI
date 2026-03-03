using Comet;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;

namespace CometBaristaNotes.Pages;

public class UserProfileManagementPage : Comet.View
{
	[State] readonly State<List<UserProfile>> _profiles = new(new());
	[State] readonly State<bool> _isLoaded = new(false);

	void LoadProfiles()
	{
		var store = InMemoryDataStore.Instance;
		if (store == null) return;
		_profiles.Value = store.GetAllProfiles();
		_isLoaded.Value = true;
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadProfiles();

		var profiles = _profiles.Value;

		if (profiles.Count == 0)
		{
			var emptyStack = new VerticalStackLayout
			{
				Spacing = Theme.SpacingM,
				Padding = new Thickness(Theme.SpacingL),
				BackgroundColor = Theme.Background,
				VerticalOptions = LayoutOptions.Fill,
			};
			emptyStack.Add(FormHelpers.MakeEmptyState("👤", "No Profiles Yet", "Create profiles for different users or coffee preferences"));
			emptyStack.Add(FormHelpers.MakePrimaryButton("+ Add Profile", () =>
			{
				Microsoft.Maui.Controls.Shell.Current.GoToAsync("profile-form?id=0");
			}));
			return new MauiViewHost(emptyStack);
		}

		var stack = new VerticalStackLayout { Spacing = Theme.SpacingS, Padding = new Thickness(Theme.SpacingM) };

		stack.Add(FormHelpers.MakePrimaryButton("+ Add Profile", () =>
		{
			Microsoft.Maui.Controls.Shell.Current.GoToAsync("profile-form?id=0");
		}));

		foreach (var profile in profiles)
		{
			stack.Add(FormHelpers.MakeListCard(
				profile.Name,
				$"Member since {profile.CreatedAt:MMM yyyy}",
				null,
				() => Microsoft.Maui.Controls.Shell.Current.GoToAsync($"profile-form?id={profile.Id}")
			));
		}

		var scrollView = new MauiScrollView
		{
			Content = stack,
			BackgroundColor = Theme.Background,
		};

		return new MauiViewHost(scrollView);
	}
}
