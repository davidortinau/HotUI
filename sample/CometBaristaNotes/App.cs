using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace CometBaristaNotes;

public class BaristaNotesApp : Application
{
	protected override Window CreateWindow(IActivationState? activationState)
	{
		ApplySavedTheme();
		return new Window(new BaristaShell());
	}

	static void ApplySavedTheme()
	{
		var saved = Preferences.Get("ThemeMode", "Auto");
		if (Application.Current is not null)
		{
			Application.Current.UserAppTheme = saved switch
			{
				"Light" => AppTheme.Light,
				"Dark" => AppTheme.Dark,
				_ => AppTheme.Unspecified,
			};
		}
	}
}
