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
{
Console.WriteLine("[MVH] CreatePlatformView");
return new MauiViewHostContainerView();
}

protected override void ConnectHandler(MauiViewHostContainerView platformView)
{
base.ConnectHandler(platformView);
Console.WriteLine($"[MVH] ConnectHandler hosted={VirtualView?.HostedView?.GetType().FullName}");
UpdateHostedView();
}

protected override void DisconnectHandler(MauiViewHostContainerView platformView)
{
platformView.ClearHostedView();
base.DisconnectHandler(platformView);
}

void UpdateHostedView()
{
if (VirtualView?.HostedView == null || MauiContext == null)
{
Console.WriteLine($"[MVH] UpdateHostedView SKIP hosted={VirtualView?.HostedView} ctx={MauiContext}");
return;
}

try
{
var hostedPlatformView = VirtualView.HostedView.ToPlatform(MauiContext);
Console.WriteLine($"[MVH] ToPlatform OK: {hostedPlatformView?.GetType().Name} frame={hostedPlatformView?.Frame}");
PlatformView.SetHostedView(hostedPlatformView);
}
catch (Exception ex)
{
Console.WriteLine($"[MVH] ToPlatform EXCEPTION: {ex}");
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
Console.WriteLine($"[MVH-Container] SetHostedView {_hostedView.GetType().Name}");
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
if (_hostedView != null)
{
_hostedView.Frame = Bounds;
Console.WriteLine($"[MVH-Container] LayoutSubviews bounds={Bounds} hostedFrame={_hostedView.Frame}");
}
}

public override CGSize SizeThatFits(CGSize size)
{
if (_hostedView != null)
{
var result = _hostedView.SizeThatFits(size);
Console.WriteLine($"[MVH-Container] SizeThatFits({size}) => {result}");
return result;
}
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
