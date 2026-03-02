using Microsoft.Maui.Dispatching;

namespace CometStressTest.Pages;

public class StateTestPage : View
{
	readonly State<int> timerCount = new State<int>(0);
	readonly State<int> rapidCount = new State<int>(0);
	readonly State<string> firstName = new State<string>("John");
	readonly State<string> lastName = new State<string>("Doe");
	readonly State<double> multiplier = new State<double>(1.0);
	readonly State<bool> timerRunning = new State<bool>(false);

	IDispatcherTimer? _timer;

	[Body]
	View body() => new ScrollView
	{
		new VStack(spacing: 12)
		{
			new Text("🧪 State Management Stress Test")
				.FontSize(22),

			// Multiple state variables
			new Text("Multi-State Binding:").FontSize(16).Color(Colors.Gray),
			new TextField(firstName, "First name"),
			new TextField(lastName, "Last name"),
			new Text(() => $"Full name: {firstName.Value} {lastName.Value}")
				.FontSize(16),
			new Text(() => $"Initials: {(firstName.Value?.Length > 0 ? firstName.Value[0].ToString() : "")}{(lastName.Value?.Length > 0 ? lastName.Value[0].ToString() : "")}")
				.FontSize(14)
				.Color(Colors.DarkBlue),

			// Computed values
			new Text("Computed Values:").FontSize(16).Color(Colors.Gray),
			new Slider(multiplier, 0, 10),
			new Text(() => $"Multiplier: {multiplier.Value:F1}")
				.FontSize(14),
			new Text(() => $"Name length × multiplier = {(firstName.Value?.Length ?? 0 + lastName.Value?.Length ?? 0) * multiplier.Value:F1}")
				.FontSize(14),

			// Timer
			new Text("Timer Test:").FontSize(16).Color(Colors.Gray),
			new Text(() => $"Timer: {timerCount.Value}s")
				.FontSize(24),
			new HStack(spacing: 10)
			{
				new Button(() => timerRunning.Value ? "Stop Timer" : "Start Timer", () =>
				{
					if (timerRunning.Value)
						StopTimer();
					else
						StartTimer();
				}),
				new Button("Reset", () =>
				{
					StopTimer();
					timerCount.Value = 0;
				}),
			},

			// Rapid state updates
			new Text("Thread-Safety Test:").FontSize(16).Color(Colors.Gray),
			new Text(() => $"Rapid count: {rapidCount.Value}")
				.FontSize(18),
			new Button("Fire 100 Rapid Updates", () =>
			{
				for (int i = 0; i < 100; i++)
				{
					rapidCount.Value++;
				}
			}),

			// Reset all
			new Button("Reset All State", () =>
			{
				StopTimer();
				timerCount.Value = 0;
				rapidCount.Value = 0;
				firstName.Value = "John";
				lastName.Value = "Doe";
				multiplier.Value = 1.0;
			}),

			new Spacer(),
		}.Padding(16),
	};

	void StartTimer()
	{
		if (_timer != null) return;

		var dispatcher = Dispatcher.GetForCurrentThread();
		if (dispatcher == null) return;

		_timer = dispatcher.CreateTimer();
		_timer.Interval = TimeSpan.FromSeconds(1);
		_timer.Tick += (s, e) => timerCount.Value++;
		_timer.Start();
		timerRunning.Value = true;
	}

	void StopTimer()
	{
		_timer?.Stop();
		_timer = null;
		timerRunning.Value = false;
	}
}
