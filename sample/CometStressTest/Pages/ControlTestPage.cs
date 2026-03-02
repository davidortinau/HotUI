namespace CometStressTest.Pages;

public class ControlTestPage : View
{
	readonly State<string> name = new State<string>("");
	readonly State<string> password = new State<string>("");
	readonly State<string> searchText = new State<string>("");
	readonly State<bool> toggleValue = new State<bool>(false);
	readonly State<double> sliderValue = new State<double>(0.5);
	readonly State<double> stepperValue = new State<double>(5);
	readonly State<int> clickCount = new State<int>(0);
	readonly State<DateTime> selectedDate = new State<DateTime>(DateTime.Today);

	[Body]
	View body() => new ScrollView
	{
		new VStack(spacing: 12)
		{
			new Text("🎮 Control Stress Test")
				.FontSize(22),

			// TextField
			new Text("TextField:").FontSize(14).Color(Colors.Gray),
			new TextField(name, "Enter your name..."),
			new Text(() => $"Hello {name.Value}!")
				.FontSize(16),

			// SecureField
			new Text("SecureField:").FontSize(14).Color(Colors.Gray),
			new SecureField(password, "Enter password..."),
			new Text(() => $"Password length: {password.Value?.Length ?? 0}")
				.FontSize(14),

			// SearchBar
			new Text("SearchBar:").FontSize(14).Color(Colors.Gray),
			new SearchBar(searchText),
			new Text(() => $"Searching: \"{searchText.Value}\"")
				.FontSize(14),

			// Toggle
			new Text("Toggle:").FontSize(14).Color(Colors.Gray),
			new HStack(spacing: 10)
			{
				new Toggle(toggleValue),
				new Text(() => toggleValue.Value ? "ON" : "OFF")
					.FontSize(16),
			},

			// Slider + ProgressBar
			new Text("Slider + ProgressBar:").FontSize(14).Color(Colors.Gray),
			new Slider(sliderValue, 0, 1),
			new ProgressBar(sliderValue),
			new Text(() => $"Value: {sliderValue.Value:F2}")
				.FontSize(14),

			// Stepper
			new Text("Stepper:").FontSize(14).Color(Colors.Gray),
			new HStack(spacing: 10)
			{
				new Stepper(stepperValue, 0, 20, 1),
				new Text(() => $"Steps: {stepperValue.Value}")
					.FontSize(16),
			},

			// DatePicker
			new Text("DatePicker:").FontSize(14).Color(Colors.Gray),
			new DatePicker(selectedDate),
			new Text(() => $"Selected: {selectedDate.Value:yyyy-MM-dd}")
				.FontSize(14),

			// Button with click counter
			new Text("Button:").FontSize(14).Color(Colors.Gray),
			new Button($"Clicked {clickCount.Value} times", () =>
			{
				clickCount.Value++;
			}),
			new Text(() => $"Total clicks: {clickCount.Value}")
				.FontSize(14),

			new Spacer(),
		}.Padding(16),
	};
}
