using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui;
using UIKit;
namespace Comet.iOS
{
	public class CUITabView : UIView
	{
		public IMauiContext Context { get; set; }
		UITabBarController tabViewController = new UITabBarController();
		public CUITabView()
		{
			tabViewController.View.BackgroundColor = UIColor.SystemBackground;
			Add(tabViewController.View);

			var appearance = new UITabBarAppearance();
			appearance.ConfigureWithDefaultBackground();
			tabViewController.TabBar.StandardAppearance = appearance;
			tabViewController.TabBar.ScrollEdgeAppearance = appearance;
		}
		public void Setup(IList<View> views)
		{
			if (views == null)
			{
				tabViewController.ViewControllers = null;
				return;
			}
			var controllers = views.Select(x
				=> new Tuple<View, UIViewController>(x, new CometViewController { MauiContext = Context, CurrentView = x })).ToList();
			foreach (var pair in controllers)
			{
				var title = pair.Item1.GetEnvironment<string>(EnvironmentKeys.TabView.Title);
				var imagePath = pair.Item1.GetEnvironment<string>(EnvironmentKeys.TabView.Image);
				UIImage image = null;
				if (!string.IsNullOrWhiteSpace(imagePath))
				{
					// Try SF Symbols first, then fall back to bundle
					image = UIImage.GetSystemImage(imagePath) ?? UIImage.FromBundle(imagePath);
				}
				pair.Item2.TabBarItem = new UITabBarItem()
				{
					Title = title ?? "",
					Image = image,
				};
			};

			tabViewController.ViewControllers = controllers.Select(x => x.Item2).ToArray();
		}

		public override void MovedToSuperview()
		{
			base.MovedToSuperview();
			var vc = this.GetViewController();
			vc?.AddChildViewController(tabViewController);
		}
		public override void RemoveFromSuperview()
		{
			tabViewController.RemoveFromParentViewController();
			base.RemoveFromSuperview();
		}
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			tabViewController.View.Frame = this.Bounds;
		}
	}
}
