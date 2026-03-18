using System;
using System.Collections.Generic;
using System.Text;
using static Comet.CometControls;

namespace Comet.Samples
{
	public class SkiaSample3 : Component
	{
		readonly State<double> _strokeSize = 2;
		readonly State<Color> _strokeColor = Colors.Black;

				public override View Render() => 
			VStack(
				HStack(
					Text("Stroke Width:"),
					Slider(_strokeSize,
						new Binding<double>(() => 1d, null),
						new Binding<double>(() => 10d, null)).FillHorizontal()
				).FillHorizontal(),
				
					Text("Stroke Color!:").HorizontalTextAlignment(TextAlignment.Center),
                //ScrollView(
                    HStack(
						Button("Black", () =>
						{
							_strokeColor.Value = Colors.Black;
						}).Color(Colors.Black),
						Button("Blue", () =>
						{
							_strokeColor.Value = Colors.Blue;
						}).Color(Colors.Blue),
						Button("Red", () =>
						{
							_strokeColor.Value = Colors.Red;
						}).Color(Colors.Red)
					).Alignment(Alignment.Center),
                //},
                new BindableFingerPaint(
					strokeSize:_strokeSize,
					strokeColor:_strokeColor).Frame(width:400, height:400).RoundedBorder(color:Colors.White)
		);
	}
}
