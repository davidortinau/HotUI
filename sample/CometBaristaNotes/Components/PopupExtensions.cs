using Comet;
using Microsoft.Maui.Controls;
using UXDivers.Popups.Maui;
using UXDivers.Popups.Maui.Controls;
using UXDivers.Popups.Services;

namespace CometBaristaNotes.Components;

/// <summary>
/// Comet-friendly extensions for UXDivers.Popups.Maui.
/// Follows the same pattern as Comet's built-in DialogExtensions
/// so popup calls feel native to the MVU framework.
/// </summary>
public static class PopupExtensions
{
	/// <summary>
	/// Shows a popup from a Comet View. The popup is presented via
	/// UXDivers IPopupService and returns when dismissed.
	/// </summary>
	public static async Task ShowPopupAsync(this Comet.View view, ActionModalPopup popup)
	{
		await IPopupService.Current.PushAsync(popup);
	}

	/// <summary>
	/// Dismisses the current popup.
	/// </summary>
	public static async Task DismissPopupAsync(this Comet.View view)
	{
		try { await IPopupService.Current.PopAsync(); }
		catch { /* No popup to dismiss */ }
	}

	/// <summary>
	/// Shows a list selection popup and invokes the callback when the action button is tapped.
	/// </summary>
	public static async Task ShowListSelectionAsync<T>(
		this Comet.View view,
		string title,
		IEnumerable<T> items,
		DataTemplate itemTemplate,
		string actionText = "Done",
		Action? onAction = null)
	{
		// Dismiss any existing popup first
		try { await IPopupService.Current.PopAsync(); } catch { }

		var popup = new ListActionPopup
		{
			Title = title,
			ActionButtonText = actionText,
			ShowActionButton = true,
			ItemsSource = items is System.Collections.IList list ? list : items.ToList(),
			ItemDataTemplate = itemTemplate,
		};

		if (onAction != null)
		{
			popup.ActionButtonCommand = new Microsoft.Maui.Controls.Command(() => onAction());
		}

		await IPopupService.Current.PushAsync(popup);
	}
}
