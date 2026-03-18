using Microsoft.Maui.Storage;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

namespace CometBaristaNotes.Pages;

public class ProfileFormPageState
{
	public string Name { get; set; } = "";
	public string AvatarPath { get; set; } = "";
	public string Error { get; set; } = "";
	public bool IsLoaded { get; set; }
}

public class ProfileFormPage : Component<ProfileFormPageState>
{
	const double AvatarSize = 100;

	readonly int _profileId;

	public ProfileFormPage(int profileId = 0) { _profileId = profileId; }

	void LoadProfile()
	{
		if (_profileId <= 0)
		{
			SetState(s => s.IsLoaded = true);
			return;
		}

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		var profile = store.GetProfile(_profileId);
		SetState(s =>
		{
			if (profile != null)
			{
				s.Name = profile.Name;
				s.AvatarPath = profile.AvatarPath ?? "";
			}
			s.IsLoaded = true;
		});
	}

	void Save()
	{
		if (string.IsNullOrWhiteSpace(State.Name))
		{
			SetState(s => s.Error = "Please enter a profile name");
			return;
		}
		SetState(s => s.Error = "");

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		if (_profileId > 0)
		{
			store.UpdateProfile(new UserProfile
			{
				Id = _profileId,
				Name = State.Name,
				AvatarPath = string.IsNullOrEmpty(State.AvatarPath) ? null : State.AvatarPath,
			});
		}
		else
		{
			store.CreateProfile(new UserProfile
			{
				Name = State.Name,
				AvatarPath = string.IsNullOrEmpty(State.AvatarPath) ? null : State.AvatarPath,
			});
		}

		Navigation?.Pop();
	}

	async void Delete()
	{
		if (_profileId <= 0) return;
		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		var page = CometBaristaNotes.Services.PageHelper.GetCurrentPage();
		if (page != null)
		{
			var confirmed = await page.DisplayAlertAsync(
				"Delete Profile?",
				$"Are you sure you want to delete '{State.Name}'? This action cannot be undone.",
				"Delete",
				"Cancel");
			if (!confirmed) return;
		}

		store.DeleteProfile(_profileId);
		Navigation?.Pop();
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

			SetState(s => s.AvatarPath = destPath);
		}
		catch
		{
			// Photo pick cancelled or failed — ignore silently
		}
	}

	View BuildAvatar()
	{
		var hasPhoto = !string.IsNullOrEmpty(State.AvatarPath) && System.IO.File.Exists(State.AvatarPath);

		var items = new List<View>();

		if (hasPhoto)
		{
			items.Add(
				Border(
					Image(State.AvatarPath)
						.Aspect(Aspect.AspectFill)
						.Frame(width: (float)AvatarSize, height: (float)AvatarSize)
				)
				.CornerRadius((float)(AvatarSize / 2))
				.StrokeColor(Theme.Primary)
				.StrokeThickness(2)
				.Frame(width: (float)AvatarSize, height: (float)AvatarSize)
			);
		}
		else
		{
			items.Add(
				Border(
					Text(Icons.Person)
						.FontFamily(Icons.FontFamily)
						.FontSize(48)
						.Color(Theme.TextMuted)
						.HorizontalTextAlignment(TextAlignment.Center)
						.VerticalTextAlignment(TextAlignment.Center)
						.Frame(width: (float)AvatarSize, height: (float)AvatarSize)
				)
				.CornerRadius((float)(AvatarSize / 2))
				.StrokeColor(Theme.Outline)
				.StrokeThickness(2)
				.Background(Theme.SurfaceVariant)
				.Frame(width: (float)AvatarSize, height: (float)AvatarSize)
			);
		}

		if (_profileId > 0)
		{
			items.Add(
				Button(hasPhoto ? "Change Photo" : "Add Photo", PickPhoto)
					.FontFamily(Theme.FontSemibold)
					.FontSize(14)
					.Color(Theme.Primary)
					.Background(Colors.Transparent)
					.Frame(height: 36)
			);
		}

		var stack = VStack(Theme.SpacingS);
		foreach (var item in items)
			stack.Add(item);

		return stack.Padding(new Thickness(0, Theme.SpacingS));
	}

	public override View Render()
	{
		if (!State.IsLoaded)
			LoadProfile();

		var isEdit = _profileId > 0;

		var items = new List<View>
		{
			FormHelpers.MakeSectionHeader(isEdit ? "EDIT PROFILE" : "NEW PROFILE"),
			BuildAvatar(),
			FormHelpers.MakeFormEntry("Name *", State.Name, "Profile name", v => SetState(s => s.Name = v)),
		};

		if (!string.IsNullOrEmpty(State.Error))
			items.Add(Text(State.Error).Color(Theme.Error).FontFamily(Theme.FontRegular).FontSize(14));

		items.Add(FormHelpers.MakePrimaryButton(isEdit ? "Save Changes" : "Create Profile", Save));

		if (isEdit)
			items.Add(FormHelpers.MakeDangerButton("Delete Profile", Delete));

		var stack = VStack(Theme.SpacingS);
		foreach (var item in items)
			stack.Add(item);

		return ScrollView(
			stack.Padding(new Thickness(Theme.SpacingM))
		)
		.Background(Theme.Background);
	}
}
