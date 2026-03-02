using System;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;

namespace Comet.Handlers
{
	public partial class CollectionViewHandler : ViewHandler<IListView, WGrid>
	{
		WFrameworkElement _hostedPlatformView;

		protected override WGrid CreatePlatformView() => new WGrid();

		public static void MapListViewProperty(IElementHandler handler, IListView virtualView)
		{
			var cvHandler = (CollectionViewHandler)handler;
			cvHandler._mauiCollectionView = CreateAndConfigureMauiCollectionView(virtualView);
			cvHandler.EmbedMauiCollectionView();
		}

#nullable enable
		public static void MapReloadData(CollectionViewHandler handler, IListView virtualView, object? value)
#nullable restore
		{
			if (handler._mauiCollectionView != null)
				RefreshItemsSource(handler._mauiCollectionView, virtualView);
		}

		void EmbedMauiCollectionView()
		{
			if (_mauiCollectionView == null || MauiContext == null)
				return;

			if (_hostedPlatformView != null)
				PlatformView.Children.Remove(_hostedPlatformView);

			try
			{
				_hostedPlatformView = _mauiCollectionView.ToPlatform(MauiContext) as WFrameworkElement;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[CollectionViewHandler] EmbedMauiCollectionView failed: {ex.Message}");
				return;
			}

			if (_hostedPlatformView != null)
			{
				_hostedPlatformView.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
				_hostedPlatformView.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;
				PlatformView.Children.Add(_hostedPlatformView);
			}
		}

		protected override void DisconnectHandler(WGrid platformView)
		{
			if (_hostedPlatformView != null)
			{
				platformView.Children.Remove(_hostedPlatformView);
				_hostedPlatformView = null;
			}
			if (_mauiCollectionView?.Handler is IElementHandler hostedHandler)
			{
				hostedHandler.DisconnectHandler();
				if (hostedHandler is IDisposable disposable)
					disposable.Dispose();
			}
			_mauiCollectionView = null;
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
