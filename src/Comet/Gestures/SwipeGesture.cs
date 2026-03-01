using System;
namespace Comet
{
	public class SwipeGesture : Gesture<SwipeGesture>
	{
		public SwipeGesture(Action<SwipeGesture> action) : base(action) { }
		public SwipeDirection Direction { get; set; }
	}

	public enum SwipeDirection
	{
		Left,
		Right,
		Up,
		Down
	}
}
