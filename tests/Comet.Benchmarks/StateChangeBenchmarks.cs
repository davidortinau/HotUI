using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Comet.Benchmarks
{
	/// <summary>
	/// Benchmarks: Time from state mutation to view update completion.
	/// XAML: PropertyChanged → binding engine → control property update
	/// MVU: State<T>.Value set → Body() rebuild → diff → handler update
	/// </summary>
	[MemoryDiagnoser]
	[SimpleJob(warmupCount: 3, iterationCount: 10)]
	public class StateChangeBenchmarks
	{
		[GlobalSetup]
		public void Setup() => BenchmarkUI.Init();

		[Params(1, 10, 50)]
		public int UpdateCount;

		// --- Single property change ---

		[Benchmark(Description = "XAML: Single property update")]
		public void XamlSinglePropertyChange()
		{
			var label = new Microsoft.Maui.Controls.Label();

			for (int i = 0; i < UpdateCount; i++)
				label.Text = i.ToString();
		}

		[Benchmark(Description = "MVU: Single state update")]
		public void MvuSingleStateChange()
		{
			var view = new SingleStateCometView();
			BenchmarkUI.InitializeHandlers(view);

			for (int i = 0; i < UpdateCount; i++)
				view._counter.Value = i;
		}

		// --- Multiple independent property changes ---

		[Benchmark(Description = "XAML: N independent property changes")]
		public void XamlMultiPropertyChange()
		{
			var stack = new VerticalStackLayout();
			var labels = new Microsoft.Maui.Controls.Label[UpdateCount];
			for (int i = 0; i < UpdateCount; i++)
			{
				labels[i] = new Microsoft.Maui.Controls.Label { Text = $"Value {i}" };
				stack.Children.Add(labels[i]);
			}

			for (int i = 0; i < UpdateCount; i++)
				labels[i].Text = $"Updated {i}";
		}

		[Benchmark(Description = "MVU: N independent state changes")]
		public void MvuMultiStateChange()
		{
			var view = new MultiStateCometView(UpdateCount);
			BenchmarkUI.InitializeHandlers(view);

			for (int i = 0; i < UpdateCount; i++)
				view.SetValue(i, $"Updated {i}");
		}

		// --- Change with no visual effect (same value) ---

		[Benchmark(Description = "XAML: No-op property change (same value)")]
		public void XamlNoOpChange()
		{
			var label = new Microsoft.Maui.Controls.Label { Text = "42" };

			for (int i = 0; i < UpdateCount; i++)
				label.Text = "42"; // Same value
		}

		[Benchmark(Description = "MVU: No-op state change (same value)")]
		public void MvuNoOpChange()
		{
			var view = new SingleStateCometView();
			BenchmarkUI.InitializeHandlers(view);
			view._counter.Value = 42;

			for (int i = 0; i < UpdateCount; i++)
				view._counter.Value = 42; // Same value
		}

		// --- Selective vs full rebuild: change 1 of N ---

		[Benchmark(Description = "XAML: Change 1 of 100 properties")]
		public void XamlSelectiveUpdate()
		{
			var stack = new VerticalStackLayout();
			var labels = new Microsoft.Maui.Controls.Label[100];
			for (int i = 0; i < 100; i++)
			{
				labels[i] = new Microsoft.Maui.Controls.Label { Text = $"Value {i}" };
				stack.Children.Add(labels[i]);
			}

			for (int iter = 0; iter < UpdateCount; iter++)
				labels[0].Text = $"Changed {iter}";
		}

		[Benchmark(Description = "MVU: Change 1 of 100 states (full rebuild)")]
		public void MvuSelectiveUpdate()
		{
			var view = new MultiStateCometView(100);
			BenchmarkUI.InitializeHandlers(view);

			for (int iter = 0; iter < UpdateCount; iter++)
				view.SetValue(0, $"Changed {iter}");
		}
	}

	// --- Helper types ---

	public class SinglePropViewModel : INotifyPropertyChanged
	{
		int _counter;
		public int Counter
		{
			get => _counter;
			set { _counter = value; OnPropertyChanged(); }
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string? name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

	public class SingleStateCometView : Comet.View
	{
		public readonly State<int> _counter = new State<int>(0);

		public SingleStateCometView()
		{
			Body = () => new Text(() => $"Count: {_counter.Value}");
		}
	}

	public class MultiPropViewModel : INotifyPropertyChanged
	{
		public string[] Values { get; }

		public MultiPropViewModel(int count)
		{
			Values = new string[count];
			for (int i = 0; i < count; i++)
				Values[i] = $"Value {i}";
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public void RaiseAllChanged()
		{
			for (int i = 0; i < Values.Length; i++)
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Values[{i}]"));
		}

		public void RaiseChanged(int index)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Values[{index}]"));
	}

	public class MultiStateCometView : Comet.View
	{
		readonly State<string>[] _states;

		public MultiStateCometView(int count)
		{
			_states = new State<string>[count];
			for (int i = 0; i < count; i++)
				_states[i] = new State<string>($"Value {i}");

			Body = () =>
			{
				var children = new Comet.View[_states.Length];
				for (int i = 0; i < _states.Length; i++)
				{
					var idx = i;
					children[i] = new Text(() => _states[idx].Value);
				}
				return LayoutHelper.ToVStack(children);
			};
		}

		public void SetValue(int index, string value)
			=> _states[index].Value = value;
	}
}
