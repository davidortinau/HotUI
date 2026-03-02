using System;
using CoreGraphics;
using UIKit;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Comet.Handlers
{
	public partial class CollectionViewHandler : ViewHandler<IListView, CollectionViewHandler.CollectionViewContainer>
	{
		protected override CollectionViewContainer CreatePlatformView() => new CollectionViewContainer();

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

			try
			{
				var platformView = _mauiCollectionView.ToPlatform(MauiContext);
				PlatformView.SetContent(platformView, _mauiCollectionView);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[CollectionViewHandler] EmbedMauiCollectionView failed: {ex.Message}");
			}
		}

		protected override void DisconnectHandler(CollectionViewContainer platformView)
		{
			platformView.ClearContent();
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
			// Use platform view's intrinsic size when constraints are unconstrained
			if (_mauiCollectionView != null)
			{
				var platformView = _mauiCollectionView.Handler?.PlatformView as UIView;
				if (platformView != null)
				{
					var fitting = platformView.SizeThatFits(new CoreGraphics.CGSize(
						double.IsInfinity(widthConstraint) ? double.MaxValue : widthConstraint,
						double.IsInfinity(heightConstraint) ? double.MaxValue : heightConstraint));
					var w = double.IsInfinity(widthConstraint) ? fitting.Width : widthConstraint;
					var h = double.IsInfinity(heightConstraint) ? fitting.Height : heightConstraint;
					if (w > 0 && h > 0)
						return new Microsoft.Maui.Graphics.Size(w, h);
				}
			}
			return new Microsoft.Maui.Graphics.Size(
				double.IsInfinity(widthConstraint) ? 400 : widthConstraint,
				double.IsInfinity(heightConstraint) ? 600 : heightConstraint);
		}

		public class CollectionViewContainer : UIView
		{
			UIView _contentView;
			IView _virtualView;

			public CollectionViewContainer() { ClipsToBounds = true; }

			public void SetContent(UIView platformView, IView virtualView)
			{
				_contentView?.RemoveFromSuperview();
				_contentView = platformView;
				_virtualView = virtualView;
				if (_contentView != null)
				{
					_contentView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
					AddSubview(_contentView);
					SetNeedsLayout();
				}
			}

			public void ClearContent()
			{
				_contentView?.RemoveFromSuperview();
				_contentView = null;
				_virtualView = null;
			}

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				if (_contentView == null || Bounds.Width <= 0 || Bounds.Height <= 0)
					return;

				_virtualView?.Measure(Bounds.Width, Bounds.Height);
				_virtualView?.Arrange(new Microsoft.Maui.Graphics.Rect(0, 0, Bounds.Width, Bounds.Height));
				_contentView.Frame = Bounds;
			}

			public override CGSize SizeThatFits(CGSize size) => size;
		}
	}
}
