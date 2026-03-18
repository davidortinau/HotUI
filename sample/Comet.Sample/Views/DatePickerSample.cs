using System;
using static Comet.CometControls;

namespace Comet.Samples
{
	public class DatePickerSample : Component
	{
		readonly State<DateTime?> currentDate = DateTime.Today;
		public DatePickerSample()
		{
			currentDate.PropertyChanged += CurrentDate_PropertyChanged;
		}

				public override View Render() => VStack(
			DatePicker((Binding<DateTime?>)currentDate,
				minimumDate: new Binding<DateTime?>(() => new DateTime(2015, 10, 1), null),
				maximumDate: new Binding<DateTime?>(() => new DateTime(2018, 10, 01), null)).Format("dd/MM/yyyy")
			.Frame(width: 200)
		);



		private void CurrentDate_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			Console.WriteLine((sender as State<DateTime?>)?.Value);
		}
	}
}
