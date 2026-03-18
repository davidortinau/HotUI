using System;
using static Comet.CometControls;

namespace Comet.Samples
{
	public class StepperSample1 : Component
	{
		readonly State<double> min = 0;
		readonly State<double> max = 10;
		readonly State<double> increment = 1;
		readonly State<double> number1 = 0;
		//private double currentValue;

				public override View Render() => VStack(
			Text($"{number1.Value}"),
			Stepper((Binding<double>)number1, (Binding<double>)max, (Binding<double>)min, (Binding<double>)increment)
		);
	}
}
