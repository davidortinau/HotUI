using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using static Comet.CometControls;

namespace Comet.Samples
{
	public class TextFieldSample4 : Component
	{
		class MyBindingObject : BindingObject
		{
			public string Text
			{
				get => GetProperty<string>();
				set => SetProperty(value);
			}
		}

		[State] private readonly MyBindingObject _state = new MyBindingObject { Text = "Edit Me" };

				public override View Render() => VStack(
			TextField(_state.Text, "Name"),
			HStack(
				Text("Current Value:")
					.Color(Colors.Grey),
				Text(_state.Text),
				Spacer()
			)
		).FillHorizontal();
	}

}
