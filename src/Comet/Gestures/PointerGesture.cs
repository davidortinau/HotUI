using System;

namespace Comet
{
/// <summary>
/// Pointer gesture for hover/mouse interactions (desktop platforms).
/// </summary>
public class PointerGesture : Gesture
{
public Action<View> PointerEntered { get; set; }
public Action<View> PointerExited { get; set; }
public Action<View> PointerMoved { get; set; }
public Action<View> PointerPressed { get; set; }
public Action<View> PointerReleased { get; set; }
}
}
