using System;
using Microsoft.Maui.Graphics;
using static Comet.CometControls;

namespace Comet.Samples
{
	public class Issue133c : Component
	{
		[State]
		readonly CreditCard _card;

		public Issue133c()
		{
			_card = new CreditCard();
		}


				public override View Render() => VStack(20,

			new BorderedEntry(_card.Number,"Enter CC Number", "\uf09d")
				.Margin(left:20, right: 20)

		).FillHorizontal().Alignment(Alignment.Top);

		private class BorderedEntry : Component
		{
			private Binding<String> _val;
			private string _placeholder;
			private string _icon;

			public BorderedEntry(Binding<String> val, string placeholder, string icon)
			{
				_val = val;
				_placeholder = placeholder;
				_icon = icon;
			}

						public override View Render() => HStack(8,
					Text(_icon)
						.Frame(width: 20)
						.Margin(left: 8)
						.FontFamily("Font Awesome 5 Free"),

					TextField(_val, new Binding<string>(() => _placeholder, null))
				)
				.Frame(height: 40)
				.RoundedBorder(color: Colors.Grey);
		}
	}
}
