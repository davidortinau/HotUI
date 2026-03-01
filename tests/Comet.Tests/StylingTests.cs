using System;
using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Comet.Tests
{
	public class StylingTests : TestBase
	{
		// ---- ResourceDictionary Tests ----

		[Fact]
		public void ResourceDictionaryBasic()
		{
			var rd = new ResourceDictionary();
			rd["PrimaryColor"] = Colors.Blue;
			rd["FontSize"] = 14.0;

			Assert.True(rd.TryGetResource("PrimaryColor", out var color));
			Assert.Equal(Colors.Blue, color);
			Assert.True(rd.TryGetResource("FontSize", out var size));
			Assert.Equal(14.0, size);
		}

		[Fact]
		public void ResourceDictionaryMerged()
		{
			var baseRd = new ResourceDictionary();
			baseRd["Color1"] = Colors.Red;

			var rd = new ResourceDictionary();
			rd.MergedDictionaries.Add(baseRd);

			Assert.True(rd.TryGetResource("Color1", out var color));
			Assert.Equal(Colors.Red, color);
		}

		[Fact]
		public void ResourceDictionaryOverride()
		{
			var baseRd = new ResourceDictionary();
			baseRd["Color1"] = Colors.Red;

			var rd = new ResourceDictionary();
			rd.MergedDictionaries.Add(baseRd);
			rd["Color1"] = Colors.Blue;

			Assert.True(rd.TryGetResource("Color1", out var color));
			Assert.Equal(Colors.Blue, color);
		}

		// ---- VisualStateManager Tests ----

		[Fact]
		public void VisualStateManagerStates()
		{
			Assert.Equal("Normal", VisualStateManager.NormalState);
			Assert.Equal("Disabled", VisualStateManager.DisabledState);
			Assert.Equal("Focused", VisualStateManager.FocusedState);
			Assert.Equal("PointerOver", VisualStateManager.PointerOverState);
		}

		[Fact]
		public void VisualStateGroupCreation()
		{
			var normalState = new VisualState { Name = "Normal" };
			var disabledState = new VisualState { Name = "Disabled" };
			disabledState.Setters.Add(new Setter { Property = "Opacity", Value = 0.5 });

			var group = new VisualStateGroup { Name = VisualStateManager.CommonStates };
			group.States.Add(normalState);
			group.States.Add(disabledState);

			Assert.Equal(2, group.States.Count);
			Assert.Equal("Normal", group.States[0].Name);
		}

		[Fact]
		public void GoToStateOnView()
		{
			var view = new Text("Hello");
			var normalState = new VisualState { Name = "Normal" };
			var disabledState = new VisualState { Name = "Disabled" };

			var group = new VisualStateGroup { Name = VisualStateManager.CommonStates };
			group.States.Add(normalState);
			group.States.Add(disabledState);

			view.WithVisualStateGroups(group);
			var result = VisualStateManager.GoToState(view, "Disabled");
			Assert.True(result);
		}

		// ---- Behavior Tests ----

		[Fact]
		public void BehaviorAttachDetach()
		{
			var behavior = new TestBehavior();
			var view = new Text("Hello");
			view.AddBehavior(behavior);
			Assert.Single(view.Behaviors);
			view.RemoveBehavior(behavior);
			Assert.Empty(view.Behaviors);
		}

		// ---- ValueConverter Tests ----

		[Fact]
		public void FuncConverterWorks()
		{
			var converter = new FuncConverter<int, string>(
				i => $"Value: {i}",
				s => int.Parse(s?.ToString()?.Replace("Value: ", "") ?? "0")
			);

			var result = converter.Convert(42, typeof(string), null, null);
			Assert.Equal("Value: 42", result);

			var back = converter.ConvertBack("Value: 42", typeof(int), null, null);
			Assert.Equal(42, back);
		}

		// ---- Trigger Tests ----

		[Fact]
		public void DataTriggerCreation()
		{
			var trigger = new DataTrigger
			{
				Property = "IsEnabled",
				Value = false
			};
			trigger.Setters.Add(new Setter { Property = "Opacity", Value = 0.5 });

			Assert.Equal("IsEnabled", trigger.Property);
			Assert.Single(trigger.Setters);
		}

		[Fact]
		public void EventTriggerCreation()
		{
			var triggered = false;
			var trigger = new EventTrigger
			{
				Event = "Clicked",
				Action = () => triggered = true
			};

			trigger.Action.Invoke();
			Assert.True(triggered);
		}

		class TestBehavior : Behavior<Text>
		{
			protected override void OnAttachedTo(Text view) { }
			protected override void OnDetachingFrom(Text view) { }
		}
	}
}
