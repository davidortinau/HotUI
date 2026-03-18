using System;
using Microsoft.Maui.Graphics;
using static Comet.CometControls;

namespace Comet.Samples
{
	public class Issue133 : Component
	{
		[State]
		readonly CreditCard Card;

		readonly State<bool> remember = false;

		public Issue133()
		{
			Card = new CreditCard();
		}


				public override View Render() => VStack(20,

			new BorderedEntry(Card.Number,"Enter CC Number", "\uf09d")
				.Margin(left:20, right: 20),

			HStack(20,
				new BorderedEntry(Card.Expiration, "MM/YYYY", "\uf783")
					.Frame(height: 40, width: 200)
					.Margin(left:20),

				Spacer(),

				new BorderedEntry(Card.CVV, "CVV", "\uf023")
					.Frame( height: 40, width: 100)
					.Margin(right:20)
			)


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
