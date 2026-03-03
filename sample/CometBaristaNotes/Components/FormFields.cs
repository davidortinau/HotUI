using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace CometBaristaNotes.Components;

public static class FormHelpers
{
	public static Comet.View FormEntry(string label, Binding<string> value, string placeholder = "")
	{
		return new VStack(spacing: 4)
		{
			new Text(label).FontSize(14).Color(Theme.TextSecondary),
			new TextField(value, placeholder)
				.FontSize(16)
				.Frame(height: Theme.FormFieldHeight)
				.Background(Theme.SurfaceVariant)
				.ClipShape(new RoundedRectangle(Theme.RadiusPill))
				.Padding(new Thickness(Theme.SpacingM, 0))
		};
	}

	public static Comet.View FormNumericEntry(string label, Binding<string> value)
	{
		return new VStack(spacing: 4)
		{
			new Text(label).FontSize(14).Color(Theme.TextSecondary),
			new TextField(value, "0")
				.FontSize(16)
				.Frame(height: Theme.FormFieldHeight)
				.Background(Theme.SurfaceVariant)
				.ClipShape(new RoundedRectangle(Theme.RadiusPill))
				.Padding(new Thickness(Theme.SpacingM, 0))
		};
	}

	public static Comet.View FormSlider(string label, Binding<double> value, double min, double max)
	{
		return new VStack(spacing: 4)
		{
			new HStack
			{
				new Text(label).FontSize(14).Color(Theme.TextSecondary),
				new Spacer(),
				new Text(() => $"{value.CurrentValue:F1}").FontSize(14).Color(Theme.TextPrimary).FontWeight(FontWeight.Bold)
			},
			new Comet.Slider(value: value, minimum: min, maximum: max)
		}.Padding(Theme.SpacingM).Background(Theme.SurfaceVariant).ClipShape(new RoundedRectangle(Theme.RadiusPill));
	}

	public static Comet.View SectionHeader(string title)
	{
		return new Text(title.ToUpperInvariant())
			.FontSize(14)
			.FontWeight(FontWeight.Semibold)
			.Color(Theme.TextSecondary)
			.Padding(new Thickness(0, Theme.SpacingM, 0, Theme.SpacingXS));
	}

	public static Comet.View Card(Comet.View content)
	{
		return new VStack
		{
			content.Padding(12)
		}.Background(Theme.Surface)
		 .RoundedBorder(radius: Theme.RadiusCard, color: Theme.Outline, strokeSize: 1);
	}

	public static Comet.View EmptyState(string icon, string title, string description)
	{
		return new VStack(spacing: 12)
		{
			new Text(icon).FontSize(48),
			new Text(title).FontSize(18).FontWeight(FontWeight.Semibold).Color(Theme.TextPrimary),
			new Text(description).FontSize(14).Color(Theme.TextSecondary)
		}.Alignment(Alignment.Center).Padding(Theme.SpacingXL);
	}

	public static Comet.View PrimaryButton(string title, Action action)
	{
		return new Comet.Button(title, action)
			.Frame(height: Theme.ButtonHeight)
			.Background(Theme.Primary)
			.Color(Colors.White)
			.FontSize(16).FontWeight(FontWeight.Semibold)
			.ClipShape(new RoundedRectangle(Theme.RadiusPill));
	}

	public static Comet.View FormPicker(string label, Binding<int> selectedIndex, string[] items)
	{
		return new VStack(spacing: 4)
		{
			new Text(label).FontSize(14).Color(Theme.TextSecondary),
			new Comet.Picker(selectedIndex, items)
				.Frame(height: Theme.FormFieldHeight)
				.Background(Theme.SurfaceVariant)
				.ClipShape(new RoundedRectangle(Theme.RadiusPill))
		};
	}

	public static Comet.View ReadOnlyField(string label, string value)
	{
		return new VStack(spacing: 4)
		{
			new Text(label).FontSize(14).Color(Theme.TextSecondary),
			new Text(value).FontSize(16).FontWeight(FontWeight.Semibold).Color(Theme.TextPrimary)
				.Frame(height: Theme.FormFieldHeight)
				.Padding(new Thickness(Theme.SpacingM, 0))
				.Background(Theme.SurfaceVariant)
				.ClipShape(new RoundedRectangle(Theme.RadiusPill))
		};
	}
}
