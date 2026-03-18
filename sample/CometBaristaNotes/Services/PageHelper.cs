namespace CometBaristaNotes.Services;

/// <summary>
/// Provides access to the current MAUI Page for displaying alerts and action sheets
/// in a pure CometApp context where Application.Current is null.
/// </summary>
public static class PageHelper
{
	/// <summary>
	/// Gets the current visible Page from the CometApp window hierarchy.
	/// Falls back to Application.Current for hybrid scenarios.
	/// </summary>
	public static Microsoft.Maui.Controls.Page? GetCurrentPage()
	{
		// Try IPlatformApplication to find the IApplication and its windows
		var app = IPlatformApplication.Current;
		if (app != null)
		{
			var application = app.Services.GetService<Microsoft.Maui.IApplication>();
			if (application != null)
			{
				foreach (var window in application.Windows)
				{
					if (window is Microsoft.Maui.Controls.Window mauiWindow)
						return mauiWindow.Page;
				}
			}
		}

		// Fallback to Application.Current (hybrid mode)
		return Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
	}

	/// <summary>
	/// Dispatch an action on the main/UI thread.
	/// </summary>
	public static void DispatchOnMainThread(Action action)
	{
		Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(action);
	}
}
