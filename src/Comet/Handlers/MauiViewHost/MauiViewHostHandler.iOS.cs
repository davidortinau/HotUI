using System;
using CoreGraphics;
using UIKit;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Comet.Handlers
{
public partial class MauiViewHostHandler : ViewHandler<MauiViewHost, MauiViewHostHandler.MauiViewHostContainerView>
{
public static IPropertyMapper<MauiViewHost, MauiViewHostHandler> Mapper =
new PropertyMapper<MauiViewHost, MauiViewHostHandler>(ViewHandler.ViewMapper);

public MauiViewHostHandler() : base(Mapper) { }

protected override MauiViewHostContainerView CreatePlatformView()
=> new MauiViewHostContainerView();

protected override void ConnectHandler(MauiViewHostContainerView platformView)
{
base.ConnectHandler(platformView);
UpdateHostedView();
}

protected override void DisconnectHandler(MauiViewHostContainerView platformView)
{
if (VirtualView?.HostedView?.Handler is IElementHandler hostedHandler)
hostedHandler.DisconnectHandler();
platformView.ClearHostedView();
base.DisconnectHandler(platformView);
}

void UpdateHostedView()
{
if (VirtualView?.HostedView == null || MauiContext == null)
return;

try
{
var hostedPlatformView = VirtualView.HostedView.ToPlatform(MauiContext);
PlatformView.SetHostedView(hostedPlatformView);
}
catch (Exception ex)
{
System.Diagnostics.Debug.WriteLine($"[MauiViewHostHandler] ToPlatform failed: {ex.Message}");
}
}

public class MauiViewHostContainerView : UIView
{
UIView _hostedView;

public void SetHostedView(UIView view)
{
_hostedView?.RemoveFromSuperview();
_hostedView = view;
if (_hostedView != null)
{
AddSubview(_hostedView);
SetNeedsLayout();
}
}

public void ClearHostedView()
{
_hostedView?.RemoveFromSuperview();
_hostedView = null;
}

public override void LayoutSubviews()
{
base.LayoutSubviews();
if (_hostedView != null && Bounds.Width > 0 && Bounds.Height > 0)
{
_hostedView.Frame = Bounds;
_hostedView.SetNeedsLayout();
_hostedView.LayoutIfNeeded();
}
}

public override CGSize SizeThatFits(CGSize size)
{
if (_hostedView != null)
return _hostedView.SizeThatFits(size);
return base.SizeThatFits(size);
}

public override CGSize IntrinsicContentSize
{
get
{
if (_hostedView != null)
return _hostedView.IntrinsicContentSize;
return base.IntrinsicContentSize;
}
}
}
}
}
