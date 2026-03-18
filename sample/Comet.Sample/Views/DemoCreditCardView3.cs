using System;
using Microsoft.Maui.Graphics;
using static Comet.CometControls;

namespace Comet.Samples
{
	public class DemoCreditCardView3 : Component
	{
		[State]
		readonly CreditCard Card;

		readonly State<bool> remember = false;

		public DemoCreditCardView3()
		{
			Card = new CreditCard();
		}


				public override View Render() => VStack(20,
			VStack(
				VStack(
					new ShapeView(new RoundedRectangle(4.0f)
						.Style(Graphics.DrawingStyle.Fill)
						.Fill(Colors.Grey))
						.Frame(40,30).Alignment( Alignment.Trailing)
						.Margin(top: 30, right: 30)
						.FitHorizontal(),

					Text("CARD NUMBER")
						.FontSize(10)
						.Color(Colors.Silver)
						.Margin(left: 30),

					Text(Card.Number)
						.FontSize(14)
						.Color(Colors.Black)
						.Margin(left: 30, bottom:20)
						.Frame(height:20),

					HStack(
						 Text("EXPIRATION")
							.FontSize(10)
							.Color(Colors.Silver)
							.Frame(width: 200),

						 Text("CVV")
							.FontSize(10)
							.Color(Colors.Silver)
					).Margin(left:30),

					HStack(
						Text(Card.Expiration)
							.FontSize(14)
							.Color(Colors.Black)
							.Frame(width: 200),

						Text(Card.CVV)
							.FontSize(14)
							.Color(Colors.Black)
					).Margin(left:30, bottom:30).Frame(height: 20)

				).RoundedBorder(radius: 8, color: Color.FromArgb("#3177CB"), filled: true).Margin(30)
			).Background("#f6f6f6"),

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
			),

			HStack(
				Toggle((Binding<bool>)remember),
				Text("  Remember Me")
			).Margin(left:20),

			Button("Purchase for $200")
				.RoundedBorder(22, Colors.SlateGrey)
				.Background(Colors.SlateGrey)
				.Color(Colors.White)
				.Frame(height:44)
				.Margin(left:20, right:20),

			new Separator(),

			Button("Or Pay with PayPal")
				.RoundedBorder(22, Colors.SlateGrey)
				.Color(Colors.SlateGrey)
				.Frame(height: 44)
				.Margin(left:20, right:20)


		).FillHorizontal().Alignment(Alignment.Top);

		public class Separator : ShapeView
		{
			public Separator() : base(new Rectangle().Stroke(Colors.Grey, 2))
			{
				this.Frame(height: 1);
			}
		}

		public class BorderedEntry : Component
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
