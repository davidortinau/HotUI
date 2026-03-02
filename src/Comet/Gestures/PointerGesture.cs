using System;
using Microsoft.Maui.Graphics;

namespace Comet
{
/// <summary>
/// Pointer gesture for hover/mouse interactions (desktop platforms).
/// Enhanced with position data for each pointer event.
/// </summary>
public class PointerGesture : Gesture
{
public Action<View, Point> PointerEntered { get; set; }
public Action<View, Point> PointerExited { get; set; }
public Action<View, Point> PointerMoved { get; set; }
public Action<View, Point> PointerPressed { get; set; }
public Action<View, Point> PointerReleased { get; set; }

// Backward-compatible setters (no position)
public void SetPointerEntered(Action<View> action) => PointerEntered = (v, _) => action(v);
public void SetPointerExited(Action<View> action) => PointerExited = (v, _) => action(v);
public void SetPointerMoved(Action<View> action) => PointerMoved = (v, _) => action(v);
public void SetPointerPressed(Action<View> action) => PointerPressed = (v, _) => action(v);
public void SetPointerReleased(Action<View> action) => PointerReleased = (v, _) => action(v);
}
}
