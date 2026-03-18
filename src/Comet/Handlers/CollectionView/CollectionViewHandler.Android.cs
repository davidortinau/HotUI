using System;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AView = global::Android.Views.View;
using AFrameLayout = global::Android.Widget.FrameLayout;
using AViewGroup = global::Android.Views.ViewGroup;

namespace Comet.Handlers
{
	public partial class CollectionViewHandler : ViewHandler<IListView, AFrameLayout>
	{
		AView _hostedPlatformView;

		protected override AFrameLayout CreatePlatformView() => new AFrameLayout(Context);

		public static void MapListViewProperty(IElementHandler handler, IListView virtualView)
		{
			var cvHandler = (CollectionViewHandler)handler;
			cvHandler._mauiItemsView = CreateAndConfigureMauiItemsView(virtualView);
			cvHandler.EmbedMauiItemsView();
		}

#nullable enable
		public static void MapReloadData(CollectionViewHandler handler, IListView virtualView, object? value)
#nullable restore
		{
			if (handler._mauiItemsView != null)
				RefreshItemsSource(handler._mauiItemsView, virtualView);
		}

		void EmbedMauiItemsView()
		{
			if (_mauiItemsView == null || MauiContext == null)
				return;

			if (_hostedPlatformView != null)
				PlatformView.RemoveView(_hostedPlatformView);

			try
			{
				_hostedPlatformView = _mauiItemsView.ToPlatform(MauiContext);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[CollectionViewHandler] EmbedMauiItemsView failed: {ex.Message}");
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

		protected override void DisconnectHandler(AFrameLayout platformView)
		{
			if (_hostedPlatformView != null)
			{
				platformView.RemoveView(_hostedPlatformView);
				_hostedPlatformView = null;
			}
			if (_mauiItemsView?.Handler is IElementHandler hostedHandler)
			{
				hostedHandler.DisconnectHandler();
				if (hostedHandler is IDisposable disposable)
					disposable.Dispose();
			}
			_mauiItemsView = null;
			base.DisconnectHandler(platformView);
		}

		public override Microsoft.Maui.Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var w = double.IsInfinity(widthConstraint) ? 400 : widthConstraint;
			var h = double.IsInfinity(heightConstraint) ? 800 : heightConstraint;
			return new Microsoft.Maui.Graphics.Size(w, h);
		}
	}
}
