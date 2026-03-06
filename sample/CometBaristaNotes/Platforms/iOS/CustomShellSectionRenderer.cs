using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Platform;
using UIKit;
using Page = Microsoft.Maui.Controls.Page;

namespace CometBaristaNotes.Platforms.iOS;

/// <summary>
/// Custom ShellSectionRenderer that enables PrefersLargeTitles on iOS.
/// Uses a transparent nav bar so the page background shows through seamlessly.
/// Sets NavigationItem.Title directly so the inline (collapsed) title appears.
/// Reapplies appearance in ViewDidLayoutSubviews to override MAUI's Shell tracker.
/// </summary>
public class CustomShellSectionRenderer : ShellSectionRenderer
{
	private bool _prefersLargeTitles;

	public CustomShellSectionRenderer(IShellContext context) : base(context)
	{
	}

	public bool PrefersLargeTitles
	{
		get => _prefersLargeTitles;
		set
		{
			_prefersLargeTitles = value;
			UpdatePrefersLargeTitles();
		}
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		UpdatePrefersLargeTitles();
		ApplyTransparentNavBar();
	}

	public override void ViewWillAppear(bool animated)
	{
		base.ViewWillAppear(animated);
		ApplyTransparentNavBar();
	}

	public override void ViewDidLayoutSubviews()
	{
		base.ViewDidLayoutSubviews();
		// This runs AFTER MAUI's Shell handler applies its appearance.
		// Reapply our transparent appearance to override MAUI's settings.
		ApplyTransparentNavBar();
	}

	public override void PushViewController(UIViewController viewController, bool animated)
	{
		ConfigureViewControllerForLargeTitles(viewController);
		base.PushViewController(viewController, animated);
	}

	public override void SetViewControllers(UIViewController[] viewControllers, bool animated)
	{
		foreach (var vc in viewControllers)
		{
			ConfigureViewControllerForLargeTitles(vc);
		}
		base.SetViewControllers(viewControllers, animated);
	}

	private void ApplyTransparentNavBar()
	{
		if (NavigationBar is null)
			return;

		var textColor = CometBaristaNotes.Components.Theme.TextPrimary.ToPlatform();
		var titleAttrs = new UIStringAttributes { ForegroundColor = textColor };

		// Transparent background — page BackgroundColor shows through
		var transparent = new UINavigationBarAppearance();
		transparent.ConfigureWithTransparentBackground();
		transparent.BackgroundColor = UIColor.Clear;
		transparent.ShadowColor = UIColor.Clear;
		transparent.TitleTextAttributes = titleAttrs;
		transparent.LargeTitleTextAttributes = titleAttrs;

		NavigationBar.StandardAppearance = transparent;
		NavigationBar.ScrollEdgeAppearance = transparent;
		NavigationBar.CompactAppearance = transparent;

		if (OperatingSystem.IsIOSVersionAtLeast(15))
		{
			NavigationBar.CompactScrollEdgeAppearance = transparent;
		}

		NavigationBar.TintColor = textColor;
		NavigationBar.Translucent = true;
	}

	private void UpdatePrefersLargeTitles()
	{
		if (NavigationBar is null)
			return;

		if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11))
		{
			NavigationBar.PrefersLargeTitles = _prefersLargeTitles;

			if (ViewControllers != null)
			{
				foreach (var vc in ViewControllers)
				{
					ConfigureViewControllerForLargeTitles(vc);
				}
			}
		}
	}

	private void ConfigureViewControllerForLargeTitles(UIViewController viewController)
	{
		if (!_prefersLargeTitles)
			return;

		if (!OperatingSystem.IsIOSVersionAtLeast(11) && !OperatingSystem.IsMacCatalystVersionAtLeast(11))
			return;

		var page = GetPageFromViewController(viewController);

		if (page != null)
		{
			var largeTitleDisplayMode = page.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>().LargeTitleDisplay();

			viewController.NavigationItem.LargeTitleDisplayMode = largeTitleDisplayMode switch
			{
				LargeTitleDisplayMode.Always => UINavigationItemLargeTitleDisplayMode.Always,
				LargeTitleDisplayMode.Never => UINavigationItemLargeTitleDisplayMode.Never,
				_ => UINavigationItemLargeTitleDisplayMode.Automatic
			};

			if (!string.IsNullOrEmpty(page.Title))
			{
				viewController.NavigationItem.Title = page.Title;
			}

			page.PropertyChanged -= OnPagePropertyChanged;
			page.PropertyChanged += OnPagePropertyChanged;
		}
		else
		{
			viewController.NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
		}
	}

	private void OnPagePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName != Page.TitleProperty.PropertyName)
			return;

		if (sender is not Page page)
			return;

		if (page.Handler is IPlatformViewHandler handler && handler.ViewController != null)
		{
			handler.ViewController.NavigationItem.Title = page.Title;
		}
	}

	private Page? GetPageFromViewController(UIViewController viewController)
	{
		if (viewController is IPlatformViewHandler handler && handler.VirtualView is Page page)
		{
			return page;
		}

		return null;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && ViewControllers != null)
		{
			foreach (var vc in ViewControllers)
			{
				var page = GetPageFromViewController(vc);
				if (page != null)
				{
					page.PropertyChanged -= OnPagePropertyChanged;
				}
			}
		}
		base.Dispose(disposing);
	}
}
