using System;
using System.Collections.Generic;
using System.Text;
using static Comet.CometControls;
namespace Comet.Samples
{
	public class SkiaSample4 : Component
	{
		readonly State<double> _strokeSize = 2;
		readonly State<Color> _strokeColor = Colors.White ;

				public override View Render()
		{
			var fingerPaint = new BindableFingerPaint(
				strokeSize: _strokeSize,
				strokeColor: _strokeColor);

			return VStack(
				VStack(
					HStack(
						Text("Stroke Width:"),
						Slider(_strokeSize,
							new Binding<double>(() => 1d, null),
							new Binding<double>(() => 10d, null)).FillHorizontal()
					),
					HStack(
						Text("Stroke Color:"),
						TextField(new Binding<string>(() => _strokeColor.Value.ToArgbHex(),(s) => _strokeColor.Value = Color.FromArgb(s)))
					),
					Button("Reset", () => fingerPaint.Reset()),
					fingerPaint.Frame(height: 400)
				)
			);
		}
	}
}
