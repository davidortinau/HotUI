using System;
namespace Comet
{
	public class PanGesture : Gesture<PanGesture>
	{
		public PanGesture(Action<PanGesture> action) : base(action) { }
		public double TotalX { get; set; }
		public double TotalY { get; set; }
		public GestureStatus Status { get; set; }
	}
}
