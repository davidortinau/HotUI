using Comet;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiBorder = Microsoft.Maui.Controls.Border;
using MauiButton = Microsoft.Maui.Controls.Button;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;
using MauiImage = Microsoft.Maui.Controls.Image;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;

namespace CometBaristaNotes.Pages;

public class ProfileFormPage : Comet.View
{
	const double AvatarSize = 100;

	readonly int _profileId;

	[State] readonly State<string> _name = new("");
	[State] readonly State<string> _avatarPath = new("");
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
		{
			_name.Value = profile.Name;
			_avatarPath.Value = profile.AvatarPath ?? "";
		}

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
				AvatarPath = string.IsNullOrEmpty(_avatarPath.Value) ? null : _avatarPath.Value,
			});
		}
		else
		{
			store.CreateProfile(new UserProfile
			{
				Name = _name.Value,
				AvatarPath = string.IsNullOrEmpty(_avatarPath.Value) ? null : _avatarPath.Value,
			});
		}

		Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
	}

	async void Delete()
	{
		if (_profileId <= 0) return;
		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		var page = Microsoft.Maui.Controls.Shell.Current?.CurrentPage;
		if (page != null)
		{
			var confirmed = await page.DisplayAlertAsync(
				"Delete Profile?",
				$"Are you sure you want to delete '{_name.Value}'? This action cannot be undone.",
				"Delete",
				"Cancel");
			if (!confirmed) return;
		}

		store.DeleteProfile(_profileId);
		await Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
	}

	async void PickPhoto()
	{
		try
		{
			var results = await Microsoft.Maui.Media.MediaPicker.Default.PickPhotosAsync();
			var result = results?.FirstOrDefault();
			if (result == null) return;

			var profilesDir = System.IO.Path.Combine(FileSystem.AppDataDirectory, "profiles");
			System.IO.Directory.CreateDirectory(profilesDir);

			var destPath = System.IO.Path.Combine(profilesDir, $"{(_profileId > 0 ? _profileId : 0)}.jpg");
			using var sourceStream = await result.OpenReadAsync();
			using var destStream = System.IO.File.Create(destPath);
			await sourceStream.CopyToAsync(destStream);

			_avatarPath.Value = destPath;
		}
		catch
		{
			// Photo pick cancelled or failed — ignore silently
		}
	}

	Microsoft.Maui.Controls.View BuildAvatar()
	{
		var container = new VerticalStackLayout
		{
			Spacing = Theme.SpacingS,
			HorizontalOptions = LayoutOptions.Center,
			Padding = new Thickness(0, Theme.SpacingS),
		};

		var hasPhoto = !string.IsNullOrEmpty(_avatarPath.Value) && System.IO.File.Exists(_avatarPath.Value);

		if (hasPhoto)
		{
			var image = new MauiImage
			{
				Source = Microsoft.Maui.Controls.ImageSource.FromFile(_avatarPath.Value),
				Aspect = Aspect.AspectFill,
				WidthRequest = AvatarSize,
				HeightRequest = AvatarSize,
			};

			var border = new MauiBorder
			{
				Content = image,
				StrokeShape = new Microsoft.Maui.Controls.Shapes.Ellipse(),
				StrokeThickness = 2,
				Stroke = new SolidColorBrush(Theme.Primary),
				WidthRequest = AvatarSize,
				HeightRequest = AvatarSize,
				HorizontalOptions = LayoutOptions.Center,
			};

			container.Add(border);
		}
		else
		{
			var icon = new MauiLabel
			{
				Text = Icons.Person,
				FontFamily = Icons.FontFamily,
				FontSize = 48,
				TextColor = Theme.TextMuted,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				WidthRequest = AvatarSize,
				HeightRequest = AvatarSize,
			};

			var border = new MauiBorder
			{
				Content = icon,
				StrokeShape = new Microsoft.Maui.Controls.Shapes.Ellipse(),
				StrokeThickness = 2,
				Stroke = new SolidColorBrush(Theme.Outline),
				BackgroundColor = Theme.SurfaceVariant,
				WidthRequest = AvatarSize,
				HeightRequest = AvatarSize,
				HorizontalOptions = LayoutOptions.Center,
			};

			container.Add(border);
		}

		// Show "Change Photo" button in edit mode (after first save)
		if (_profileId > 0)
		{
			var photoBtn = new MauiButton
			{
				Text = hasPhoto ? "Change Photo" : "Add Photo",
				FontFamily = Theme.FontSemibold,
				FontSize = 14,
				TextColor = Theme.Primary,
				BackgroundColor = Colors.Transparent,
				HeightRequest = 36,
			};
			photoBtn.Clicked += (s, e) => PickPhoto();
			container.Add(photoBtn);
		}

		return container;
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadProfile();

		var isEdit = _profileId > 0;

		var stack = new VerticalStackLayout { Spacing = Theme.SpacingS, Padding = new Thickness(Theme.SpacingM) };

		stack.Add(FormHelpers.MakeSectionHeader(isEdit ? "EDIT PROFILE" : "NEW PROFILE"));
		stack.Add(BuildAvatar());
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
