using System;

namespace Comet
{
/// <summary>
/// Drag gesture recognizer for drag source support.
/// </summary>
public class DragGesture : Gesture
{
public Func<View, object> DragStarting { get; set; }
public Action<View> DropCompleted { get; set; }
public bool CanDrag { get; set; } = true;
}

/// <summary>
/// Drop gesture recognizer for drop target support.
/// </summary>
public class DropGesture : Gesture
{
public Action<View, object> Drop { get; set; }
public Func<View, object, bool> DragOver { get; set; }
public bool AllowDrop { get; set; } = true;
}
}
