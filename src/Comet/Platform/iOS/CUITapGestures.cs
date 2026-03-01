using System;
using UIKit;
namespace Comet.iOS
{
	public class CUITapGesture : UITapGestureRecognizer
	{
		public CUITapGesture(TapGesture gesture) : base(() => gesture.Invoke())
		{
			gesture.PlatformGesture = this;
		}
		public override UIGestureRecognizerState State { get => base.State; set => base.State = value; }
	}

	public class CUILongPressGesture : UILongPressGestureRecognizer
	{
		readonly LongPressGesture _gesture;
		public CUILongPressGesture(LongPressGesture gesture) : base(() => gesture.Invoke())
		{
			_gesture = gesture;
			gesture.PlatformGesture = this;
			MinimumPressDuration = gesture.MinimumPressDuration;
		}
	}

	public class CUIPanGesture : UIPanGestureRecognizer
	{
		readonly PanGesture _gesture;
		public CUIPanGesture(PanGesture gesture)
		{
			_gesture = gesture;
			gesture.PlatformGesture = this;
			AddTarget(() =>
			{
				var translation = TranslationInView(View);
				_gesture.TotalX = translation.X;
				_gesture.TotalY = translation.Y;
				_gesture.Status = State switch
				{
					UIGestureRecognizerState.Began => GestureStatus.Started,
					UIGestureRecognizerState.Changed => GestureStatus.Running,
					UIGestureRecognizerState.Ended => GestureStatus.Completed,
					UIGestureRecognizerState.Cancelled => GestureStatus.Canceled,
					_ => GestureStatus.Running
				};
				_gesture.Invoke();
			});
		}
	}

	public class CUIPinchGesture : UIPinchGestureRecognizer
	{
		readonly PinchGesture _gesture;
		public CUIPinchGesture(PinchGesture gesture)
		{
			_gesture = gesture;
			gesture.PlatformGesture = this;
			AddTarget(() =>
			{
				_gesture.Scale = Scale;
				_gesture.Status = State switch
				{
					UIGestureRecognizerState.Began => GestureStatus.Started,
					UIGestureRecognizerState.Changed => GestureStatus.Running,
					UIGestureRecognizerState.Ended => GestureStatus.Completed,
					UIGestureRecognizerState.Cancelled => GestureStatus.Canceled,
					_ => GestureStatus.Running
				};
				_gesture.Invoke();
			});
		}
	}

	public class CUISwipeGesture : UISwipeGestureRecognizer
	{
		public CUISwipeGesture(SwipeGesture gesture) : base(() => gesture.Invoke())
		{
			gesture.PlatformGesture = this;
			Direction = gesture.Direction switch
			{
				SwipeDirection.Left => UISwipeGestureRecognizerDirection.Left,
				SwipeDirection.Right => UISwipeGestureRecognizerDirection.Right,
				SwipeDirection.Up => UISwipeGestureRecognizerDirection.Up,
				SwipeDirection.Down => UISwipeGestureRecognizerDirection.Down,
				_ => UISwipeGestureRecognizerDirection.Left
			};
		}
	}
}
