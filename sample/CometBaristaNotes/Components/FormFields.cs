using Comet;

namespace CometBaristaNotes.Components;

/// <summary>
/// Factory methods returning Comet views for form fields and UI components.
/// </summary>
public static class FormHelpers
{
	public static View MakeIcon(string glyph, double size, Color color)
	{
		return Text(glyph)
			.FontFamily(Icons.FontFamily)
			.FontSize(size)
			.Color(color)
			.HorizontalTextAlignment(TextAlignment.Center)
			.VerticalTextAlignment(TextAlignment.Center);
	}

	public static View MakeCard(View content)
	{
		return Border(content)
			.CornerRadius(Theme.RadiusCard)
			.Background(Theme.CardBackground)
			.StrokeColor(Theme.CardStroke)
			.StrokeThickness(1)
			.Padding(new Thickness(Theme.SpacingM));
	}

	public static View MakeSectionHeader(string title)
	{
		return Text(title.ToUpperInvariant())
			.FontFamily(Theme.FontSemibold)
			.FontSize(13)
			.FontWeight(FontWeight.Bold)
			.Color(Theme.TextSecondary)
			.Margin(new Thickness(0, Theme.SpacingM, 0, Theme.SpacingXS));
	}

	public static View MakeFormEntry(string label, string value, string placeholder, Action<string> onChanged)
	{
		return VStack(
			Text(label)
				.FontFamily(Theme.FontRegular)
				.FontSize(12)
				.Color(Theme.TextSecondary)
				.Margin(new Thickness(16, 0, 0, 4)),

			Border(
				TextField(value, placeholder)
					.FontSize(16)
					.Color(Theme.TextPrimary)
					.Background(Colors.Transparent)
					.Frame(height: Theme.FormFieldHeight)
					.Margin(new Thickness(16, 0))
					.OnTextChanged(onChanged)
			)
			.CornerRadius(Theme.RadiusPill)
			.Background(Theme.SurfaceVariant)
			.StrokeThickness(0)
		);
	}

	public static View MakeReadOnlyField(string label, string value)
	{
		return VStack(
			Text(label)
				.FontFamily(Theme.FontRegular)
				.FontSize(12)
				.Color(Theme.TextSecondary)
				.Margin(new Thickness(16, 0, 0, 4)),

			Border(
				Text(value)
					.FontFamily(Theme.FontSemibold)
					.FontSize(16)
					.FontWeight(FontWeight.Bold)
					.Color(Theme.TextPrimary)
					.VerticalTextAlignment(TextAlignment.Center)
					.Frame(height: Theme.FormFieldHeight)
					.Padding(new Thickness(Theme.SpacingM, 0))
			)
			.CornerRadius(Theme.RadiusPill)
			.Background(Theme.SurfaceVariant)
			.StrokeThickness(0)
		);
	}

	public static View MakePrimaryButton(string title, Action action)
	{
		return Button(title, action)
			.FontFamily(Theme.FontSemibold)
			.Background(Theme.Primary)
			.Color(Colors.White)
			.FontSize(16)
			.FontWeight(FontWeight.Bold)
			.Frame(height: Theme.ButtonHeight)
			.CornerRadius((int)Theme.RadiusPill);
	}

	public static View MakeSecondaryButton(string title, Action action)
	{
		return Button(title, action)
			.FontFamily(Theme.FontSemibold)
			.Background(Theme.SurfaceVariant)
			.Color(Theme.Primary)
			.FontSize(16)
			.FontWeight(FontWeight.Bold)
			.Frame(height: Theme.ButtonHeight)
			.CornerRadius((int)Theme.RadiusPill);
	}

	public static View MakeDangerButton(string title, Action action)
	{
		return Button(title, action)
			.FontFamily(Theme.FontSemibold)
			.Background(Theme.Error)
			.Color(Colors.White)
			.FontSize(16)
			.FontWeight(FontWeight.Bold)
			.Frame(height: Theme.ButtonHeight)
			.CornerRadius((int)Theme.RadiusPill);
	}

	public static View MakeEmptyState(string icon, string title, string description)
	{
		return VStack(12,
			Text(icon)
				.FontFamily(Icons.FontFamily)
				.FontSize(48)
				.HorizontalTextAlignment(TextAlignment.Center),

			Text(title)
				.FontFamily(Theme.FontSemibold)
				.FontSize(18)
				.FontWeight(FontWeight.Bold)
				.Color(Theme.TextPrimary)
				.HorizontalTextAlignment(TextAlignment.Center),

			Text(description)
				.FontFamily(Theme.FontRegular)
				.FontSize(14)
				.Color(Theme.TextSecondary)
				.HorizontalTextAlignment(TextAlignment.Center)
		)
		.Padding(new Thickness(Theme.SpacingXL));
	}

	public static View MakeListCard(string title, string? subtitle, string? detail, Action? onTap)
	{
		var infoViews = new List<View>
		{
			Text(title)
				.FontFamily(Theme.FontSemibold)
				.FontSize(16)
				.FontWeight(FontWeight.Bold)
				.Color(Theme.TextPrimary),
		};
		if (subtitle != null)
			infoViews.Add(
				Text(subtitle)
					.FontFamily(Theme.FontRegular)
					.FontSize(14)
					.Color(Theme.TextSecondary));
		if (detail != null)
			infoViews.Add(
				Text(detail)
					.FontFamily(Theme.FontRegular)
					.FontSize(12)
					.Color(Theme.TextMuted));

		var infoStack = VStack(2);
		foreach (var v in infoViews)
			infoStack.Add(v);

		var chevron = Text(Icons.ChevronRight)
			.FontFamily(Icons.FontFamily)
			.FontSize(20)
			.Color(Theme.TextMuted)
			.VerticalTextAlignment(TextAlignment.Center)
			.Padding(new Thickness(Theme.SpacingS, 0));

		var row = HStack(Theme.SpacingS,
			infoStack.FillHorizontal(),
			chevron
		);

		View card = Border(row)
			.CornerRadius(Theme.RadiusCard)
			.Background(Theme.CardBackground)
			.StrokeColor(Theme.CardStroke)
			.StrokeThickness(1)
			.Padding(new Thickness(Theme.SpacingM));

		if (onTap != null)
			card = card.OnTap(_ => onTap());

		return card;
	}

	public static View MakeFormPicker(string label, int selectedIndex, string[] items, Action<int> onChanged)
	{
		return VStack(
			Text(label)
				.FontFamily(Theme.FontRegular)
				.FontSize(12)
				.Color(Theme.TextSecondary)
				.Margin(new Thickness(16, 0, 0, 4)),

			Border(
				Picker(selectedIndex, items)
					.Color(Theme.TextPrimary)
					.Background(Colors.Transparent)
					.Frame(height: Theme.FormFieldHeight)
					.Margin(new Thickness(16, 0))
					.OnSelectedIndexChanged(onChanged)
			)
			.CornerRadius(Theme.RadiusPill)
			.Background(Theme.SurfaceVariant)
			.StrokeThickness(0)
		);
	}

	public static View MakeFormSlider(string label, double value, double min, double max, Action<double> onChanged)
	{
		return VStack(
			Text(label)
				.FontFamily(Theme.FontRegular)
				.FontSize(12)
				.Color(Theme.TextSecondary)
				.Margin(new Thickness(16, 0, 0, 4)),

			Border(
				Slider(value, min, max)
					.MinimumTrackColor(Theme.Primary)
					.MaximumTrackColor(Theme.SurfaceVariant)
					.Margin(new Thickness(16, 0))
					.OnValueChanged(onChanged)
			)
			.CornerRadius(Theme.RadiusPill)
			.Background(Theme.SurfaceVariant)
			.StrokeThickness(0)
			.Frame(height: Theme.FormFieldHeight)
		);
	}

	public static View MakeFormEditor(string label, string value, Action<string> onChanged)
	{
		return VStack(
			Text(label)
				.FontFamily(Theme.FontRegular)
				.FontSize(12)
				.Color(Theme.TextSecondary)
				.Margin(new Thickness(16, 0, 0, 4)),

			Border(
				TextEditor(value)
					.FontSize(16)
					.Color(Theme.TextPrimary)
					.Background(Colors.Transparent)
					.Frame(height: 80)
					.Margin(new Thickness(16, 8))
					.OnTextChanged(onChanged)
			)
			.CornerRadius(Theme.RadiusEditor)
			.Background(Theme.SurfaceVariant)
			.StrokeThickness(0)
		);
	}

	public static View MakeFormEntryWithLimit(string label, string value, string placeholder, int maxLength, Action<string> onChanged)
	{
		var currentLength = value?.Length ?? 0;

		return VStack(
			Text(label)
				.FontFamily(Theme.FontRegular)
				.FontSize(12)
				.Color(Theme.TextSecondary)
				.Margin(new Thickness(16, 0, 0, 4)),

			Border(
				TextField(value, placeholder)
					.FontSize(16)
					.Color(Theme.TextPrimary)
					.Background(Theme.SurfaceVariant)
					.Frame(height: Theme.FormFieldHeight)
					.OnTextChanged(text =>
					{
						text ??= string.Empty;
						if (text.Length > maxLength)
							text = text[..maxLength];
						onChanged(text);
					})
			)
			.CornerRadius(Theme.RadiusPill)
			.Background(Theme.SurfaceVariant)
			.StrokeThickness(0),

			Text($"{currentLength}/{maxLength}")
				.FontFamily(Theme.FontRegular)
				.FontSize(12)
				.Color(currentLength >= maxLength ? Theme.Warning : Theme.TextMuted)
				.HorizontalTextAlignment(TextAlignment.End)
		);
	}

	public static View MakeToggleRow(string label, bool isOn, Action<bool> onChanged)
	{
		var grid = Grid(columns: new object[] { "*", "Auto" }, rows: new object[] { "Auto" },
			Text(label)
				.FontFamily(Theme.FontSemibold)
				.FontSize(14)
				.FontWeight(FontWeight.Bold)
				.Color(Theme.TextPrimary)
				.VerticalTextAlignment(TextAlignment.Center)
				.Cell(row: 0, column: 0),

			Toggle(isOn)
				.OnColor(Theme.Primary)
				.OnToggled(onChanged)
				.Cell(row: 0, column: 1)
		);

		return MakeCard(grid);
	}
}
