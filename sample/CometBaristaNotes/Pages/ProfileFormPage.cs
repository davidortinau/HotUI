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

public class ProfileFormPage : Comet.View
{
	readonly int _profileId;

	[State] readonly State<string> _name = new("");
	[State] readonly State<string> _error = new("");
	[State] readonly State<bool> _isLoaded = new(false);

	public ProfileFormPage(int profileId = 0) { _profileId = profileId; }

	IUserProfileService? GetProfileService() =>
		ViewHandler?.MauiContext?.Services.GetService<IUserProfileService>();

	void LoadProfile()
	{
		if (_profileId <= 0) { _isLoaded.Value = true; return; }

		var svc = GetProfileService();
		if (svc == null) return;

		var profile = svc.GetProfile(_profileId);
		if (profile != null)
			_name.Value = profile.Name;

		_isLoaded.Value = true;
	}

	void Save()
	{
		if (string.IsNullOrWhiteSpace(_name.Value))
		{
			_error.Value = "Please enter a profile name";
			return;
		}
		_error.Value = "";

		var svc = GetProfileService();
		if (svc == null) return;

		if (_profileId > 0)
		{
			svc.UpdateProfile(new UserProfile
			{
				Id = _profileId,
				Name = _name.Value,
			});
		}
		else
		{
			svc.CreateProfile(new UserProfile
			{
				Name = _name.Value,
			});
		}

		Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadProfile();

		var isEdit = _profileId > 0;

		return new ScrollView
		{
			new VStack(spacing: Theme.SpacingS)
			{
				FormHelpers.SectionHeader(isEdit ? "EDIT PROFILE" : "NEW PROFILE"),

				FormHelpers.FormEntry("Name *", _name, "Profile name"),

				!string.IsNullOrEmpty(_error.Value)
					? new Text(_error.Value).Color(Theme.Error).FontSize(14)
					: null,

				FormHelpers.PrimaryButton(isEdit ? "Save Changes" : "Create Profile", Save),
			}.Padding(Theme.SpacingM)
		}.Background(Theme.Background);
	}
}
