using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

namespace CometBaristaNotes.Pages;

public class UserProfileManagementPageState
{
	public List<UserProfile> Profiles { get; set; } = new();
	public bool IsLoaded { get; set; }
}

public class UserProfileManagementPage : Component<UserProfileManagementPageState>
{
	void LoadProfiles()
	{
		var store = InMemoryDataStore.Instance;
		if (store == null) return;
		SetState(s =>
		{
			s.Profiles = store.GetAllProfiles();
			s.IsLoaded = true;
		});
	}

	public override View Render()
	{
		if (!State.IsLoaded)
			LoadProfiles();

		var profiles = State.Profiles;

		if (profiles.Count == 0)
		{
			return VStack(Theme.SpacingM,
				FormHelpers.MakeEmptyState(Icons.Person, "No Profiles Yet", "Create profiles for different users or coffee preferences"),
				FormHelpers.MakePrimaryButton("+ Add Profile", () => Navigation?.Navigate(new ProfileFormPage(0)))
			)
			.Padding(new Thickness(Theme.SpacingL))
			.Background(Theme.Background);
		}

		var stack = VStack(Theme.SpacingS,
			FormHelpers.MakePrimaryButton("+ Add Profile", () => Navigation?.Navigate(new ProfileFormPage(0)))
		);

		foreach (var profile in profiles)
		{
			stack.Add(FormHelpers.MakeListCard(
				profile.Name,
				$"Member since {profile.CreatedAt:MMM yyyy}",
				null,
				() => Navigation?.Navigate(new ProfileFormPage(profile.Id))
			));
		}

		return ScrollView(stack.Padding(new Thickness(Theme.SpacingM)))
			.Background(Theme.Background);
	}
}
