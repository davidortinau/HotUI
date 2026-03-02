using Comet.iOS;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace Comet.Handlers
{
	public partial class NavigationViewHandler : ViewHandler<NavigationView, UIView>, IPlatformViewHandler
	{
		UIViewController viewController;
		UIViewController IPlatformViewHandler.ViewController => viewController;
		protected override UIView CreatePlatformView()
		{
			var vc = new Comet.iOS.CometViewController { MauiContext = MauiContext, CurrentView = VirtualView.Content };
			var nav = VirtualView;
			if (nav.Navigation != null)
			{
				viewController = vc;
				return viewController.View;
			}
			var navigationController = new CUINavigationController();
			viewController = navigationController;

			nav.SetPerformNavigate((toView) => {
				if (toView is NavigationView newNav)
				{
					newNav.SetPerformNavigate(nav);
					newNav.SetPerformPop(nav);
				}

				toView.Navigation = nav;
				var newVc = new Comet.iOS.CometViewController { MauiContext = MauiContext, CurrentView = toView };
				navigationController.PushViewController(newVc, true);
			});
			nav.SetPerformPop(() => navigationController.PopViewController(true));
			navigationController.PushViewController(vc, true);

			// Add leading bar button (hamburger icon) if configured
			if (nav.LeadingBarAction != null)
			{
				var action = nav.LeadingBarAction;
				vc.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(
					nav.LeadingBarIcon ?? "☰",
					UIBarButtonItemStyle.Plain,
					(s, e) => action());
			}

			return navigationController.View;
		}

		protected override void ConnectHandler(UIView platformView)
		{
			base.ConnectHandler(platformView);
			ApplyNavigationBarBackground();
		}

		void ApplyNavigationBarBackground()
		{
			if (viewController is CUINavigationController navController)
			{
				var bgPaint = VirtualView?.GetBackground();
				if (bgPaint is Microsoft.Maui.Graphics.SolidPaint solid && solid.Color != null)
				{
					var uiColor = solid.Color.ToPlatform();
					var appearance = new UINavigationBarAppearance();
					appearance.ConfigureWithOpaqueBackground();
					appearance.BackgroundColor = uiColor;
					appearance.ShadowColor = UIColor.Clear;
					navController.NavigationBar.StandardAppearance = appearance;
					navController.NavigationBar.ScrollEdgeAppearance = appearance;
					navController.NavigationBar.CompactAppearance = appearance;
				}
			}
		}
	}
}
