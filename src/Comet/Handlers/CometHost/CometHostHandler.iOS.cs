using System;
using CoreGraphics;
using UIKit;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Comet.Handlers;

/// <summary>
/// iOS handler for CometHost. Renders the Comet View's body content (typically a MauiViewHost)
/// directly by calling GetView() then ToPlatform() on the rendered content.
/// </summary>
public partial class CometHostHandler : ViewHandler<CometHost, CometHostHandler.CometHostContainerView>
{
	public CometHostHandler() : base(CometHostMapper) { }

	protected override CometHostContainerView CreatePlatformView()
		=> new CometHostContainerView();

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

	// Fill available space
	public override Microsoft.Maui.Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
	{
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

	public class CometHostContainerView : UIView
	{
		UIView _contentView;
		IView _virtualView;

		public CometHostContainerView()
		{
			ClipsToBounds = true;
		}

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
			_contentView.SetNeedsLayout();
			_contentView.LayoutIfNeeded();
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			// Fill available space
			return size;
		}
	}
}
