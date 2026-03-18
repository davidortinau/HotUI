namespace CometBaristaNotes.Services;

public class ThemeService : IThemeService
{
	private const string PreferenceKey = "app_theme";

	public AppThemeMode CurrentMode { get; private set; } = AppThemeMode.System;

	public event Action<AppThemeMode>? ThemeChanged;

	public void SetTheme(AppThemeMode mode)
	{
		CurrentMode = mode;
		Microsoft.Maui.Storage.Preferences.Set(PreferenceKey, mode.ToString());
		ApplyTheme(mode);
		ThemeChanged?.Invoke(mode);
	}

	public void LoadSavedTheme()
	{
		var saved = Microsoft.Maui.Storage.Preferences.Get(PreferenceKey, "System");
		CurrentMode = saved switch
		{
			"Light" => AppThemeMode.Light,
			"Dark" => AppThemeMode.Dark,
			_ => AppThemeMode.System,
		};
		ApplyTheme(CurrentMode);
	}

	private static void ApplyTheme(AppThemeMode mode)
	{
		// In pure CometApp mode, Application.Current is null.
		// Theme switching is handled by the Comet view layer
		// through State<AppThemeMode> reactivity — views re-render
		// when the theme mode changes via the ThemeChanged event.
	}
}
