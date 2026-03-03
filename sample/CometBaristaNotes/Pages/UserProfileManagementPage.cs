using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Extensions.DependencyInjection;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;
using Button = Comet.Button;
using ScrollView = Comet.ScrollView;

namespace CometBaristaNotes.Pages;

public class UserProfileManagementPage : Comet.View
{
	[State] readonly State<List<UserProfile>> _profiles = new(new());
	[State] readonly State<bool> _isLoaded = new(false);

	IUserProfileService? GetProfileService() =>
		ViewHandler?.MauiContext?.Services.GetService<IUserProfileService>();

	void LoadProfiles()
	{
		var svc = GetProfileService();
		if (svc == null) return;
		_profiles.Value = svc.GetAllProfiles();
		_isLoaded.Value = true;
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadProfiles();

		var profiles = _profiles.Value;

		if (profiles.Count == 0)
			return new VStack(spacing: 16)
			{
				FormHelpers.EmptyState("👤", "No Profiles Yet",
					"Create profiles for different users or coffee preferences"),
				new Button("+ Add Profile", () =>
				{
					Microsoft.Maui.Controls.Shell.Current.GoToAsync("profile-form?id=0");
				})
			}.Padding(24);

		return new ScrollView
		{
			new VStack(spacing: 8)
			{
				new Button("+ Add Profile", () =>
				{
					Microsoft.Maui.Controls.Shell.Current.GoToAsync("profile-form?id=0");
				}),
				profiles.Select(profile =>
					FormHelpers.Card(
						new VStack(spacing: 4)
						{
							new Text(profile.Name).FontSize(16).FontWeight(FontWeight.Semibold),
							new Text($"Created: {profile.CreatedAt:MMM d, yyyy}")
								.FontSize(12).Color(Colors.Gray),
						}
					).OnTap(_ =>
					{
						Microsoft.Maui.Controls.Shell.Current.GoToAsync($"profile-form?id={profile.Id}");
					})
				).ToArray()
			}.Padding(16)
		};
	}
}
