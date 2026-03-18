using System;
using System.Collections.Generic;
using Comet;
using Comet.Styles;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using static Comet.CometControls;

namespace CometControlsGallery.Pages
{
	internal static class GalleryPageHelpers
	{
		static readonly Color PageBackground = Colors.White;

		public static View Scaffold(string title, params View[] sections) =>
			ScrollView(
				VStack(16, sections)
					.Padding(new Thickness(24))
			)
			.Background(PageBackground)
			.Title(title);

		public static View Section(string title, params View[] content)
		{
			var views = new List<View> { SectionHeader(title) };
			views.AddRange(content);
			views.Add(Separator());
			return VStack(10, views.ToArray());
		}

		public static View Section(string title, string _description, params View[] content)
		{
			return Section(title, content);
		}

		public static View SectionHeader(string title) =>
			Text(title)
				.FontSize(16)
				.FontWeight(FontWeight.Bold)
				.Color(Colors.CornflowerBlue);

		public static View Separator() =>
			new ShapeView(new Rectangle())
				.Background(Colors.Grey)
				.Frame(height: 1)
				.Opacity(0.3f);

		public static View BodyText(string value) =>
			Text(value)
				.FontSize(14);

		public static View Caption(string value) =>
			Text(value)
				.FontSize(12)
				.Color(Colors.Grey);

		public static View NavButton(string label, Action action) =>
			Button(label, action)
				.ButtonStyle(ButtonStyles.Outlined);

		public static View ColorBlock(string label, Color background, Color textColor, float height = 56) =>
			Border(
				Text(label)
					.FontWeight(FontWeight.Bold)
					.Color(textColor)
			)
			.Background(background)
			.CornerRadius(16)
			.Frame(height: height)
			.Padding(new Thickness(16, 12));
	}
}
