using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using static Comet.CometControls;

namespace Comet.Samples
{
	public class CreditCard : BindingObject
	{
		public string Number
		{
			get => GetProperty<string>();
			set => SetProperty(value);
		}
		public string Expiration
		{
			get => GetProperty<string>();
			set => SetProperty(value);
		}
		public string CVV
		{
			get => GetProperty<string>();
			set => SetProperty(value);
		}
		public string Name
		{
			get => GetProperty<string>();
			set => SetProperty(value);
		}
	}

	public class DemoCreditCardView : Component
	{
		[State]
		readonly CreditCard Card;

		readonly State<string> number = "";
		readonly State<bool> remember = false;

		Color titleColor = Color.FromArgb("#1d1d1d");
		Color ccColor = Color.FromArgb("#999999");

		public DemoCreditCardView()
		{
			Card = new CreditCard();
		}

				public override View Render() => Grid(
			columns: new object[] { 20, "*", 20 },
			rows: new object[] { "250", 20, 160, 20, 44, 20, 1, 20, 44, "*" },
			Grid(
				columns: new object[] { 30, "*", 30 },
				rows: new object[] { 30,"*",30},
                // cc background
                new ShapeView(
					new RoundedRectangle(8)
						.Fill(Color.FromArgb("#3177CB"))
						.Style(Graphics.DrawingStyle.Fill)
				).Cell(row:1, column:1),

                // the cc details
                Grid(
					columns: new object[]{ 30, 120, "*", 40, 30 },
					rows: new object[]{ 30, 30, 20, 30, 10, 20, 30, "*" },
					Text("CARD NUMBER")
						.FontSize(10)
						.Color(Colors.Silver)
						.Cell(row:2, column:1, colSpan:2),
					Text(Card.Number)
						.FontSize(14)
						.Color(Colors.Black)
						.Cell(row:3, column:1, colSpan:2),

					Text("EXPIRATION")
						.FontSize(10)
						.Color(Colors.Silver)
						.Cell(row:5, column:1),
					Text(Card.Expiration)
						.FontSize(14)
						.Color(Colors.Black)
						.Cell(row:6, column:1),

					Text("CVV")
						.FontSize(10)
						.Color(Colors.Silver)
						.Cell(row:5, column:2),
					Text(Card.CVV)
						.FontSize(14)
						.Color(Colors.Black)
						.Cell(row:6, column:2),
					HStack(
						new ShapeView(new RoundedRectangle(4.0f).Fill(Colors.Black)).Frame(40,30)
					).Cell(row: 1, column: 3)


				).Cell(row:1, column:1)

			)
			.Cell(row:0, column:0, colSpan:3)
			.Background(Color.FromArgb("#E5E9EE"))
			.Frame(height:250),
			Grid(
				columns: new object[] { "2*", 20, "*" },
				rows: new object[] { 40, 20, 40, 20, 40, 20, 44, 20, 1, 20, 44 },
				EntryContainer(Card.Number, "Enter CC Number", "\uf09d").Cell(row:0, column: 0, colSpan: 3),
				EntryContainer(Card.Expiration, "MM/YYYY", "\uf783").Cell(row:2, column: 0),
				EntryContainer(Card.CVV, "CVV", "\uf023").Cell(row:2, column: 2),
				HStack(
					Toggle((Binding<bool>)remember),
					Text("  Remember Me")
				).Cell(row:4,column:0, colSpan: 3),
				Button("Or Pay with PayPal").RoundedBorder(22, Colors.SlateGrey).Cell(row:6, column:0, colSpan:3).Color(Colors.SlateGrey),
				HRule().Cell(row:8,column:0,colSpan:3),
				Button("Purchase for $200").RoundedBorder(22, Colors.SlateGrey).Background(Colors.SlateGrey).Cell(row:10,column:0,colSpan:3).Color(Colors.White)
			).Cell(row:2, column:1)

		);

		View HRule()
		{
			return new ShapeView(
				new Rectangle()
					.Stroke(Colors.Grey, 2)
				)
				.Frame(100, 1);
		}

		Text CCText(Binding<string> val)
		{
			return Text(val)
				.Frame(height: 24)
				.FontSize(12)
				.Color(ccColor);
		}

		Text TitleText(Binding<string> val)
		{
			return Text(val)
				.FontSize(24)
				.Color(titleColor);
		}

		HStack EntryContainer(Binding<String> val, string placeholder, string icon = "")
		{
			return HStack(10,
					Text(icon)
						.Frame(width:20)
						.Margin(left:8, top:8)
						.FontFamily("Font Awesome 5 Free"),
					TextField(val, new Binding<string>(() => placeholder, null)).Margin(top:9)

			).RoundedBorder(color: Colors.Grey).FillHorizontal();
		}

		//class CCText : Text
		//{
		//    public CCText(Binding<string> val) : base(val)
		//    {
		//    }

		//}
	}


}
