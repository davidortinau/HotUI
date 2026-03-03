using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Comet.Benchmarks
{
	/// <summary>
	/// Benchmarks: Rapid-fire state changes simulating animation-like workloads.
	/// Tests throughput and coalescing behavior under high-frequency updates.
	/// </summary>
	[MemoryDiagnoser]
	[SimpleJob(warmupCount: 3, iterationCount: 10)]
	public class RapidUpdateBenchmarks
	{
		[GlobalSetup]
		public void Setup() => BenchmarkUI.Init();

		// --- Counter thrashing: increment as fast as possible ---

		[Params(100, 1000, 5000)]
		public int Iterations;

		[Benchmark(Description = "XAML: Rapid counter updates")]
		public void XamlRapidCounter()
		{
			var label = new Microsoft.Maui.Controls.Label();

			for (int i = 0; i < Iterations; i++)
				label.Text = i.ToString();
		}

		[Benchmark(Description = "MVU: Rapid counter updates")]
		public void MvuRapidCounter()
		{
			var view = new RapidCounterCometView();
			BenchmarkUI.InitializeHandlers(view);

			for (int i = 0; i < Iterations; i++)
				view._value.Value = i;
		}

		// --- Multi-property animation: update X, Y, Opacity, Scale ---

		[Benchmark(Description = "XAML: Multi-property animation (4 props)")]
		public void XamlMultiPropAnimation()
		{
			var label = new Microsoft.Maui.Controls.Label();

			for (int i = 0; i < Iterations; i++)
			{
				label.TranslationX = i * 0.1;
				label.TranslationY = i * 0.2;
				label.Opacity = (i % 100) / 100.0;
				label.Scale = 1.0 + (i % 50) * 0.01;
			}
		}

		[Benchmark(Description = "MVU: Multi-property animation (4 states)")]
		public void MvuMultiPropAnimation()
		{
			var view = new AnimationCometView();
			BenchmarkUI.InitializeHandlers(view);

			for (int i = 0; i < Iterations; i++)
			{
				view._x.Value = (float)(i * 0.1);
				view._y.Value = (float)(i * 0.2);
				view._opacity.Value = (float)((i % 100) / 100.0);
				view._scale.Value = (float)(1.0 + (i % 50) * 0.01);
			}
		}

		// --- String-heavy updates (allocation pressure) ---

		[Benchmark(Description = "XAML: String-heavy updates")]
		public void XamlStringUpdates()
		{
			var label = new Microsoft.Maui.Controls.Label();

			for (int i = 0; i < Iterations; i++)
				label.Text = $"The quick brown fox jumps over the lazy dog — iteration {i} of {Iterations} with timestamp {DateTime.UtcNow.Ticks}";
		}

		[Benchmark(Description = "MVU: String-heavy updates")]
		public void MvuStringUpdates()
		{
			var view = new StringHeavyCometView();
			BenchmarkUI.InitializeHandlers(view);

			for (int i = 0; i < Iterations; i++)
				view._content.Value = $"The quick brown fox jumps over the lazy dog — iteration {i} of {Iterations} with timestamp {DateTime.UtcNow.Ticks}";
		}
	}

	// --- Helper types ---

	public class RapidCounterVM : INotifyPropertyChanged
	{
		int _value;
		public int Value
		{
			get => _value;
			set { _value = value; OnPropertyChanged(); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
		void OnPropertyChanged([CallerMemberName] string? n = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
	}

	public class RapidCounterCometView : Comet.View
	{
		public readonly State<int> _value = new State<int>(0);
		public RapidCounterCometView()
		{
			Body = () => new Text(() => $"Value: {_value.Value}");
		}
	}

	public class AnimationVM : INotifyPropertyChanged
	{
		double _x, _y, _opacity = 1, _scale = 1;
		public double X { get => _x; set { _x = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); } }
		public double Y { get => _y; set { _y = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); } }
		public double Opacity { get => _opacity; set { _opacity = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); } }
		public double Scale { get => _scale; set { _scale = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); } }
		public string DisplayText => $"X:{_x:F1} Y:{_y:F1} O:{_opacity:F2} S:{_scale:F2}";

		public event PropertyChangedEventHandler? PropertyChanged;
		void OnPropertyChanged([CallerMemberName] string? n = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
	}

	public class AnimationCometView : Comet.View
	{
		public readonly State<float> _x = new State<float>(0);
		public readonly State<float> _y = new State<float>(0);
		public readonly State<float> _opacity = new State<float>(1);
		public readonly State<float> _scale = new State<float>(1);

		public AnimationCometView()
		{
			Body = () => new Text(() => $"X:{_x.Value:F1} Y:{_y.Value:F1} O:{_opacity.Value:F2} S:{_scale.Value:F2}");
		}
	}

	public class StringHeavyVM : INotifyPropertyChanged
	{
		string _content = "";
		public string Content
		{
			get => _content;
			set { _content = value; OnPropertyChanged(); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
		void OnPropertyChanged([CallerMemberName] string? n = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
	}

	public class StringHeavyCometView : Comet.View
	{
		public readonly State<string> _content = new State<string>("");
		public StringHeavyCometView()
		{
			Body = () => new Text(() => _content.Value);
		}
	}
}
