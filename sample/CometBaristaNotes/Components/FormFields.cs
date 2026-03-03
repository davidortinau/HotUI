using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace CometBaristaNotes.Components;

public static class FormHelpers
{
	static readonly Color LabelColor = Colors.Gray;
	static readonly Color CardBackground = Color.FromArgb("#F5F5F5");

	public static Comet.View FormEntry(string label, Binding<string> value, string placeholder = "")
	{
		return new VStack(spacing: 4)
		{
			new Text(label).FontSize(12).Color(LabelColor),
			new TextField(value, placeholder)
				.FontSize(16)
		}.Padding(12).Background(CardBackground).ClipShape(new RoundedRectangle(8));
	}

	public static Comet.View FormNumericEntry(string label, Binding<string> value)
	{
		return new VStack(spacing: 4)
		{
			new Text(label).FontSize(12).Color(LabelColor),
			new TextField(value, "0")
				.FontSize(16)
		}.Padding(12).Background(CardBackground).ClipShape(new RoundedRectangle(8));
	}

	public static Comet.View FormSlider(string label, Binding<double> value, double min, double max)
	{
		return new VStack(spacing: 4)
		{
			new HStack
			{
				new Text(label).FontSize(12).Color(LabelColor),
				new Spacer(),
				new Text(() => $"{value.CurrentValue:F1}").FontSize(12).Color(LabelColor)
			},
			new Comet.Slider(value: value, minimum: min, maximum: max)
		}.Padding(12).Background(CardBackground).ClipShape(new RoundedRectangle(8));
	}

	public static Comet.View SectionHeader(string title)
	{
		return new Text(title)
			.FontSize(14)
			.FontWeight(FontWeight.Semibold)
			.Color(LabelColor)
			.Padding(new Thickness(0, 16, 0, 4));
	}

	public static Comet.View Card(Comet.View content)
	{
		return new VStack
		{
			content.Padding(16)
		}.Background(Colors.White).ClipShape(new RoundedRectangle(12));
	}

	public static Comet.View EmptyState(string icon, string title, string description)
	{
		return new VStack(spacing: 12)
		{
			new Text(icon).FontSize(48),
			new Text(title).FontSize(18).FontWeight(FontWeight.Semibold),
			new Text(description).FontSize(14).Color(LabelColor)
		}.Alignment(Alignment.Center).Padding(32);
	}
}
