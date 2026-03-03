using Comet;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;

namespace CometBaristaNotes.Pages;

public class ProfileFormPage : Comet.View
{
	readonly int _profileId;

	[State] readonly State<string> _name = new("");
	[State] readonly State<string> _error = new("");
	[State] readonly State<bool> _isLoaded = new(false);

	public ProfileFormPage(int profileId = 0) { _profileId = profileId; }

	void LoadProfile()
	{
		if (_profileId <= 0) { _isLoaded.Value = true; return; }

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		var profile = store.GetProfile(_profileId);
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

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		if (_profileId > 0)
		{
			store.UpdateProfile(new UserProfile
			{
				Id = _profileId,
				Name = _name.Value,
			});
		}
		else
		{
			store.CreateProfile(new UserProfile
			{
				Name = _name.Value,
			});
		}

		Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
	}

	void Delete()
	{
		if (_profileId <= 0) return;
		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		store.DeleteProfile(_profileId);
		Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadProfile();

		var isEdit = _profileId > 0;

		var stack = new VerticalStackLayout { Spacing = Theme.SpacingS, Padding = new Thickness(Theme.SpacingM) };

		stack.Add(FormHelpers.MakeSectionHeader(isEdit ? "EDIT PROFILE" : "NEW PROFILE"));
		stack.Add(FormHelpers.MakeFormEntry("Name *", _name.Value, "Profile name", v => _name.Value = v));

		if (!string.IsNullOrEmpty(_error.Value))
			stack.Add(new MauiLabel { Text = _error.Value, TextColor = Theme.Error, FontFamily = Theme.FontRegular, FontSize = 14 });

		stack.Add(FormHelpers.MakePrimaryButton(isEdit ? "Save Changes" : "Create Profile", Save));

		if (isEdit)
			stack.Add(FormHelpers.MakeDangerButton("Delete Profile", Delete));

		var scrollView = new MauiScrollView
		{
			Content = stack,
			BackgroundColor = Theme.Background,
		};

		return new MauiViewHost(scrollView);
	}
}
