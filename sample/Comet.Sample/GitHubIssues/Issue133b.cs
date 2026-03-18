using System;
using Microsoft.Maui.Graphics;
using static Comet.CometControls;

namespace Comet.Samples
{
	public class Issue133b : Component
	{
		[State]
		readonly CreditCard Card;

		readonly State<bool> remember = false;

		public Issue133b()
		{
			Card = new CreditCard();
		}


				public override View Render() => VStack(20,

			new BorderedEntry(Card.Number,"Enter CC Number", "\uf09d")
				.Margin(left:20, right: 20)

		).FillHorizontal().Alignment(Alignment.Top);

		public class BorderedEntry : HStack
		{
			public BorderedEntry(Binding<String> val, string placeholder, string icon) : base(spacing: 8)
			{
				Add(Text(icon)
					.Frame(width: 20)
					.Margin(left: 8)
					.FontFamily("Font Awesome 5 Free"));

				Add(TextField(val, new Binding<string>(() => placeholder, null)));

				this.Frame(height: 40).RoundedBorder(color: Colors.Grey);

			}
		}
	}
}
