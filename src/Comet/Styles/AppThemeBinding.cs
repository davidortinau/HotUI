using System;
using Microsoft.Maui.ApplicationModel;

namespace Comet.Styles
{
	/// <summary>
	/// Provides reactive theme-aware values that respond to light/dark mode changes.
	/// Usage: view.BackgroundColor(AppThemeValue.Get(light: Colors.White, dark: Colors.Black))
	/// </summary>
	public static class AppThemeValue
	{
		/// <summary>
		/// Raised when the application theme changes.
		/// </summary>
		public static event Action ThemeChanged;

		/// <summary>
		/// Returns the appropriate value based on the current app theme.
		/// </summary>
		public static T Get<T>(T light, T dark)
		{
			var currentTheme = Theme.Current?.CurrentTheme ?? AppTheme.Light;

			if (currentTheme == AppTheme.System)
			{
				try
				{
					var requestedTheme = Microsoft.Maui.ApplicationModel.AppInfo.Current?.RequestedTheme;
					currentTheme = requestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark
						? AppTheme.Dark
						: AppTheme.Light;
				}
				catch
				{
					currentTheme = AppTheme.Light;
				}
			}

			return currentTheme == AppTheme.Dark ? dark : light;
		}

		/// <summary>
		/// Notifies listeners that the theme has changed and triggers re-evaluation.
		/// </summary>
		public static void NotifyThemeChanged()
		{
			ThemeChanged?.Invoke();
		}
	}
}
