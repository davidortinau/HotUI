using System;
using CoreGraphics;
using UIKit;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Comet.Handlers
{
	/// <summary>
	/// iOS handler for MauiViewHost. Creates a container UIView that hosts
	/// the MAUI Controls platform view directly, bypassing CometView.
	/// </summary>
	public partial class MauiViewHostHandler : ViewHandler<MauiViewHost, UIView>
	{
		public static IPropertyMapper<MauiViewHost, MauiViewHostHandler> Mapper =
			new PropertyMapper<MauiViewHost, MauiViewHostHandler>(ViewHandler.ViewMapper);

		public MauiViewHostHandler() : base(Mapper) { }

		private UIView _hostedPlatformView;

		protected override UIView CreatePlatformView()
			=> new MauiViewHostContainerView();

		protected override void ConnectHandler(UIView platformView)
		{
			base.ConnectHandler(platformView);
			UpdateHostedView();
		}

		protected override void DisconnectHandler(UIView platformView)
		{
			_hostedPlatformView?.RemoveFromSuperview();
			_hostedPlatformView = null;
			base.DisconnectHandler(platformView);
		}

		void UpdateHostedView()
		{
			if (VirtualView?.HostedView == null || MauiContext == null)
				return;

			_hostedPlatformView?.RemoveFromSuperview();

			try
			{
				_hostedPlatformView = VirtualView.HostedView.ToPlatform(MauiContext);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[MauiViewHostHandler] ToPlatform failed: {ex.Message}");
				return;
			}

			if (_hostedPlatformView != null)
				PlatformView.AddSubview(_hostedPlatformView);
		}

		class MauiViewHostContainerView : UIView
		{
			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				foreach (var sub in Subviews)
					sub.Frame = Bounds;
			}

			public override CGSize SizeThatFits(CGSize size)
			{
				if (Subviews.Length > 0)
					return Subviews[0].SizeThatFits(size);
				return base.SizeThatFits(size);
			}
		}
	}
}
