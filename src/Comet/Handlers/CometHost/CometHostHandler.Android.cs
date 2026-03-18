using System;
using Android.Views;
using Android.Widget;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Comet.Handlers;

/// <summary>
/// Android handler for CometHost. Creates a container FrameLayout that hosts
/// the Comet View's rendered body content (typically a MauiViewHost).
/// </summary>
public partial class CometHostHandler : ViewHandler<CometHost, CometHostHandler.CometHostContainerView>
{
	public CometHostHandler() : base(CometHostMapper) { }

	protected override CometHostContainerView CreatePlatformView()
		=> new CometHostContainerView(Context);

	protected override void ConnectHandler(CometHostContainerView platformView)
	{
		base.ConnectHandler(platformView);
		UpdateCometView();
	}

	protected override void DisconnectHandler(CometHostContainerView platformView)
	{
		platformView.ClearContent();
		base.DisconnectHandler(platformView);
	}

	public override Microsoft.Maui.Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
	{
		if (VirtualView is IContentView contentView)
		{
			var size = contentView.CrossPlatformMeasure(widthConstraint, heightConstraint);
			if (size.Width > 0 && size.Height > 0)
				return size;
		}
		var w = double.IsInfinity(widthConstraint) ? 400 : widthConstraint;
		var h = double.IsInfinity(heightConstraint) ? 800 : heightConstraint;
		return new Microsoft.Maui.Graphics.Size(w, h);
	}

	void UpdateCometView()
	{
		if (VirtualView?.CometView == null || MauiContext == null)
			return;

		try
		{
			var cometView = VirtualView.CometView;
			
			// Get the render view (body content) to avoid CometViewHandler handler circularity
			var renderView = cometView.GetView();
			IView viewToRender = (renderView != null && renderView != cometView) ? renderView : cometView;
			
			var platformView = viewToRender.ToPlatform(MauiContext);
			if (platformView != null)
				PlatformView.SetContent(platformView, viewToRender);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[CometHostHandler] UpdateCometView failed: {ex.Message}");
		}
	}

	public class CometHostContainerView : FrameLayout
	{
		global::Android.Views.View _contentView;
		IView _virtualView;

		public CometHostContainerView(global::Android.Content.Context context) : base(context) { }

		public void SetContent(global::Android.Views.View platformView, IView virtualView)
		{
			if (_contentView != null)
				RemoveView(_contentView);
			_contentView = platformView;
			_virtualView = virtualView;
			if (_contentView != null)
			{
				AddView(_contentView, new FrameLayout.LayoutParams(
					ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
			}
		}

		public void ClearContent()
		{
			if (_contentView != null)
				RemoveView(_contentView);
			_contentView = null;
			_virtualView = null;
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);
			if (_contentView == null) return;

			var width = right - left;
			var height = bottom - top;
			if (width > 0 && height > 0 && _virtualView != null)
			{
				var density = Context?.Resources?.DisplayMetrics?.Density ?? 1;
				var widthDp = width / density;
				var heightDp = height / density;
				_virtualView.Measure(widthDp, heightDp);
				_virtualView.Arrange(new Microsoft.Maui.Graphics.Rect(0, 0, widthDp, heightDp));
			}
		}
	}
}
