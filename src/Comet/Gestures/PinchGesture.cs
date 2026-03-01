using System;
namespace Comet
{
	public class PinchGesture : Gesture<PinchGesture>
	{
		public PinchGesture(Action<PinchGesture> action) : base(action) { }
		public double Scale { get; set; } = 1.0;
		public GestureStatus Status { get; set; }
	}
}
