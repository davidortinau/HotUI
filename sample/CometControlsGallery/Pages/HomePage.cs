using System;
using System.Runtime.InteropServices;
using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using static Comet.CometControls;

namespace CometControlsGallery.Pages
{
	public class HomePage : View
	{
		[Body]
		View body() =>
			ScrollView(
				VStack(16,
					Text("\ud83c\udf4e Comet on Mac Catalyst")
						.FontSize(32)
						.FontWeight(FontWeight.Bold)
						.HorizontalTextAlignment(TextAlignment.Center),
					Text("Rendered natively with AppKit")
						.FontSize(16)
						.Color(Colors.Grey)
						.HorizontalTextAlignment(TextAlignment.Center),
					Text("This sample app demonstrates Comet on Mac Catalyst \u2014 " +
						"a minimal MVU framework that uses .NET MAUI core " +
						"to map native controls. No MAUI Controls required!")
						.FontSize(14),
					Border(
						VStack(8,
							Text("Platform Details")
								.FontSize(18)
								.FontWeight(FontWeight.Bold),
							Text("\u2022 Comet control handlers mapped to UIKit")
								.FontSize(14),
							Text("\u2022 Native rendering")
								.FontSize(14),
							Text("\u2022 WebKit for BlazorWebView")
								.FontSize(14),
							Text("\u2022 CoreGraphics-backed ICanvas for GraphicsView")
								.FontSize(14),
							Text("\u2022 .NET 10 / MAUI 10")
								.FontSize(14),
							Text($"\u2022 Runtime: {RuntimeInformation.FrameworkDescription}")
								.FontSize(14)
								.Color(Colors.Grey),
							Text($"\u2022 OS: {RuntimeInformation.OSDescription}")
								.FontSize(14)
								.Color(Colors.Grey)
						)
						.Padding(new Thickness(16))
					)
					.CornerRadius(8)
					.StrokeColor(Colors.DodgerBlue)
					.StrokeThickness(1),
					Text("Use the menu on the left to explore different control demos.")
						.FontSize(14)
						.Color(Colors.Grey)
						.HorizontalTextAlignment(TextAlignment.Center)
				)
				.Padding(new Thickness(24))
			)
			.Title("Home");
	}
}
