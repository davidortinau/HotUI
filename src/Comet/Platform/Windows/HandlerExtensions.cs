using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace Comet;

public static partial class HandlerExtensions
{
	public static void AddGesture(this IViewHandler handler, Gesture gesture)
	{
		if (handler.PlatformView is not UIElement nativeView)
			return;

		if (gesture is TapGesture tapGesture)
		{
			nativeView.Tapped += (s, e) => tapGesture.Invoke();
			gesture.PlatformGesture = nativeView;
		}
		else if (gesture is LongPressGesture longPressGesture)
		{
			nativeView.Holding += (s, e) =>
			{
				if (e.HoldingState == Microsoft.UI.Input.HoldingState.Started)
					longPressGesture.Invoke();
			};
			gesture.PlatformGesture = nativeView;
		}
		else if (gesture is PanGesture panGesture)
		{
			nativeView.ManipulationMode |= ManipulationModes.TranslateX | ManipulationModes.TranslateY;
			nativeView.ManipulationStarted += (s, e) =>
			{
				panGesture.TotalX = 0;
				panGesture.TotalY = 0;
				panGesture.Status = GestureStatus.Started;
				panGesture.Invoke();
			};
			nativeView.ManipulationDelta += (s, e) =>
			{
				panGesture.TotalX = e.Cumulative.Translation.X;
				panGesture.TotalY = e.Cumulative.Translation.Y;
				panGesture.Status = GestureStatus.Running;
				panGesture.Invoke();
			};
			nativeView.ManipulationCompleted += (s, e) =>
			{
				panGesture.TotalX = e.Cumulative.Translation.X;
				panGesture.TotalY = e.Cumulative.Translation.Y;
				panGesture.Status = GestureStatus.Completed;
				panGesture.Invoke();
			};
			gesture.PlatformGesture = nativeView;
		}
		else if (gesture is PinchGesture pinchGesture)
		{
			nativeView.ManipulationMode |= ManipulationModes.Scale;
			nativeView.ManipulationDelta += (s, e) =>
			{
				pinchGesture.Scale = e.Cumulative.Scale;
				pinchGesture.Status = GestureStatus.Running;
				pinchGesture.Invoke();
			};
			nativeView.ManipulationCompleted += (s, e) =>
			{
				pinchGesture.Scale = e.Cumulative.Scale;
				pinchGesture.Status = GestureStatus.Completed;
				pinchGesture.Invoke();
			};
			gesture.PlatformGesture = nativeView;
		}
	}

	public static void RemoveGesture(this IViewHandler handler, Gesture gesture)
	{
		// Windows gesture handlers are attached to the UIElement lifecycle
		// and will be cleaned up when the native view is disposed
	}
}
