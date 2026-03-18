using System;
using CoreGraphics;
using UIKit;
using Microsoft.Maui.HotReload;
using Microsoft.Maui;

namespace Comet.iOS
{
	public class CometView : UIView, IReloadHandler
	{
		public CometView(IMauiContext mauiContext) {
			MauiContext = mauiContext;
			BackgroundColor = UIColor.White;
		}
		public CometView(CGRect rect, IMauiContext mauiContext) : base(rect)
		{
			MauiContext = mauiContext;
			BackgroundColor = UIColor.White;
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
			base.LayoutSubviews();
			if (currentPlatformView == null)
				return;
			_view?.Measure(Bounds.Width, Bounds.Height);
			currentPlatformView.Frame = Bounds;
		}



		public void Reload() => SetView(CurrentView, true);
	}
}
