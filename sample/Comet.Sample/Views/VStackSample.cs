using System;
using System.Collections.Generic;
using static Comet.CometControls;

namespace Comet.Samples
{
	public class VStackSample : Component
	{
		readonly State<string> _textValue = "Edit Me";
		readonly State<double> _sliderValue = 50;

				public override View Render() => VStack(
			Text(() => _textValue.Value),
			TextField(_textValue, "Name"),
			SecureField(_textValue, "Name"),
			Slider((Binding<double>)_sliderValue),
			ProgressBar((Binding<double>)_sliderValue)
		).FillHorizontal();
	}

}
