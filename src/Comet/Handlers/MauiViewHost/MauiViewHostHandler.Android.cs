using System;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AView = global::Android.Views.View;
using AFrameLayout = global::Android.Widget.FrameLayout;
using AViewGroup = global::Android.Views.ViewGroup;

namespace Comet.Handlers
{
	/// <summary>
	/// Android handler for MauiViewHost. Creates a container FrameLayout that hosts
	/// the MAUI Controls platform view directly.
	/// </summary>
	public partial class MauiViewHostHandler : ViewHandler<MauiViewHost, AFrameLayout>
	{
		public static IPropertyMapper<MauiViewHost, MauiViewHostHandler> Mapper =
			new PropertyMapper<MauiViewHost, MauiViewHostHandler>(ViewHandler.ViewMapper);

		public MauiViewHostHandler() : base(Mapper) { }

		private AView _hostedPlatformView;

		protected override AFrameLayout CreatePlatformView()
			=> new AFrameLayout(Context);

		protected override void ConnectHandler(AFrameLayout platformView)
		{
			base.ConnectHandler(platformView);
			UpdateHostedView();
		}

		protected override void DisconnectHandler(AFrameLayout platformView)
		{
			if (VirtualView?.HostedView?.Handler is IElementHandler hostedHandler)
				hostedHandler.DisconnectHandler();
			if (_hostedPlatformView != null)
			{
				platformView.RemoveView(_hostedPlatformView);
				_hostedPlatformView = null;
			}
			base.DisconnectHandler(platformView);
		}

		void UpdateHostedView()
		{
			if (VirtualView?.HostedView == null || MauiContext == null)
				return;

			if (_hostedPlatformView != null)
				PlatformView.RemoveView(_hostedPlatformView);

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
			{
				PlatformView.AddView(_hostedPlatformView,
					new AFrameLayout.LayoutParams(
						AViewGroup.LayoutParams.MatchParent,
						AViewGroup.LayoutParams.MatchParent));
			}
		}
	}
}
