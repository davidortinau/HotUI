using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using static Comet.CometControls;

namespace Comet.Samples
{
	public class BasicTestView : Component
	{
		class MyBindingObject : BindingObject
		{
			public bool CanEdit
			{
				get => GetProperty<bool>();
				set => SetProperty(value);
			}

			public string Text
			{
				get => GetProperty<string>();
				set => SetProperty(value);
			}
		}

		[State]
		readonly MyBindingObject state;

		readonly State<int> clickCount = new State<int>(1);

		readonly State<bool> bar = new State<bool>();

		public BasicTestView()
		{
			state = new MyBindingObject
			{
				Text = "Bar",
				CanEdit = true,
			};
			
		}

		public override View Render() =>
			VStack(
				(state.CanEdit
					? (View) TextField(state.Text)
					: Text(() => $"{state.Text}: multiText")), // Text will warn you. This should be done by TextBinding
                Text(state.Text),
				HStack(
					Button("Toggle Entry/Label",
						() => state.CanEdit = !state.CanEdit)
						.Background(Colors.Salmon),
					Button("Update Text",
						() => state.Text = $"Click Count: {clickCount.Value++}" )
				)
			);
	}
}
