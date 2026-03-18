using System;
using CoreGraphics;
using UIKit;
using Microsoft.Maui.HotReload;
using Microsoft.Maui;

namespace Comet.iOS
{
	public class CometView : UIView, IReloadHandler
	{
		bool _inLayout;

		public CometView(IMauiContext mauiContext) {
			MauiContext = mauiContext;
			BackgroundColor = UIColor.SystemBackground;
		}
		public CometView(CGRect rect, IMauiContext mauiContext) : base(rect)
		{
			MauiContext = mauiContext;
			BackgroundColor = UIColor.SystemBackground;
		}
		IView _view;
		public IView CurrentView
		{
			get => _view;
			set => SetView(value);
		}
		public IMauiContext MauiContext { get; internal set; }

		UIView currentPlatformView;
		IViewHandler currentHandler;
		void SetView(IView view, bool forceRefresh = false)
		{
			if (view == _view && !forceRefresh)
				return;
			//reuse the handlers!
			if(view is View v && _view is View pv &&
				v.GetContentTypeHashCode() == pv.GetContentTypeHashCode()
				&& currentHandler != null)
			{
				_view = view;
				v.ViewHandler = currentHandler;
				if (_view is IHotReloadableView ihr1)
				{
					ihr1.ReloadHandler = this;
					MauiHotReloadHelper.AddActiveView(ihr1);
				}
				return;
			}

			_view = view;

			if (_view is IHotReloadableView ihr)
			{
				ihr.ReloadHandler = this;
				MauiHotReloadHelper.AddActiveView(ihr);
			}
			// Resolve views with a Body (e.g. Component<T>) to their concrete view tree
			// before calling ToPlatform, to avoid circular CometViewHandler→CometView loop.
			var viewToRender = _view;
			if (viewToRender is View cometView && cometView.Body != null)
				viewToRender = cometView.GetView();
			var newPlatformView = viewToRender?.ToPlatform(MauiContext);
			currentHandler = _view?.Handler;
			if (currentPlatformView == newPlatformView)
				return;
			currentPlatformView?.RemoveFromSuperview();
			if (newPlatformView != this && newPlatformView != null)
				AddSubview(currentPlatformView = newPlatformView);
		}


		public override void LayoutSubviews()
		{
			if (_inLayout)
				return;
			_inLayout = true;
			try
			{
				base.LayoutSubviews();
				if (currentPlatformView == null || Bounds.Width <= 0 || Bounds.Height <= 0)
					return;

				// Invalidate measurement so the view tree remeasures with
				// new constraints (critical for device rotation).
				if (_view is View cometView)
					cometView.MeasurementValid = false;

				_view?.Measure(Bounds.Width, Bounds.Height);
				_view?.Arrange(new Microsoft.Maui.Graphics.Rect(0, 0, Bounds.Width, Bounds.Height));
				currentPlatformView.Frame = Bounds;
				currentPlatformView.SetNeedsLayout();
				currentPlatformView.LayoutIfNeeded();
			}
			finally
			{
				_inLayout = false;
			}
		}



		public void Reload() => SetView(CurrentView, true);
	}
}
