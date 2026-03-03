using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiBorder = Microsoft.Maui.Controls.Border;
using MauiEntry = Microsoft.Maui.Controls.Entry;
using MauiButton = Microsoft.Maui.Controls.Button;
using MauiSlider = Microsoft.Maui.Controls.Slider;
using MauiPicker = Microsoft.Maui.Controls.Picker;
using MauiEditor = Microsoft.Maui.Controls.Editor;
using MauiGrid = Microsoft.Maui.Controls.Grid;
using MauiBoxView = Microsoft.Maui.Controls.BoxView;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;
using MauiFontAttributes = Microsoft.Maui.Controls.FontAttributes;

namespace CometBaristaNotes.Components;

/// <summary>
/// Factory methods returning native MAUI controls for proper rendering in Shell.
/// </summary>
public static class FormHelpers
{
	public static MauiLabel MakeIcon(string glyph, double size, Color color)
	{
		return new MauiLabel
		{
			Text = glyph,
			FontFamily = Icons.FontFamily,
			FontSize = size,
			TextColor = color,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
		};
	}

	public static Microsoft.Maui.Controls.View MakeCard(Microsoft.Maui.Controls.View content)
	{
		return new MauiBorder
		{
			Content = content,
			BackgroundColor = Theme.CardBackground,
			Stroke = new SolidColorBrush(Theme.CardStroke),
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = Theme.RadiusCard },
			Padding = new Thickness(Theme.SpacingM),
		};
	}

	public static Microsoft.Maui.Controls.View MakeSectionHeader(string title)
	{
		return new MauiLabel
		{
			Text = title.ToUpperInvariant(),
			FontFamily = Theme.FontSemibold,
			FontSize = 13,
			FontAttributes = MauiFontAttributes.Bold,
			TextColor = Theme.TextSecondary,
			Margin = new Thickness(0, Theme.SpacingM, 0, Theme.SpacingXS),
		};
	}

	public static Microsoft.Maui.Controls.View MakeFormEntry(string label, string value, string placeholder, Action<string> onChanged)
	{
		var stack = new VerticalStackLayout { Spacing = 4 };
		stack.Add(new MauiLabel { Text = label, FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary });

		var entry = new MauiEntry
		{
			Text = value,
			Placeholder = placeholder,
			FontSize = 16,
			TextColor = Theme.TextPrimary,
			BackgroundColor = Theme.SurfaceVariant,
			HeightRequest = Theme.FormFieldHeight,
		};
		entry.TextChanged += (s, e) => onChanged(e.NewTextValue ?? "");

		var border = new MauiBorder
		{
			Content = entry,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = Theme.RadiusPill },
			BackgroundColor = Theme.SurfaceVariant,
		};

		stack.Add(border);
		return stack;
	}

	public static Microsoft.Maui.Controls.View MakeReadOnlyField(string label, string value)
	{
		var stack = new VerticalStackLayout { Spacing = 4 };
		stack.Add(new MauiLabel { Text = label, FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary });

		var valueLabel = new MauiLabel
		{
			Text = value,
			FontFamily = Theme.FontSemibold,
			FontSize = 16,
			FontAttributes = MauiFontAttributes.Bold,
			TextColor = Theme.TextPrimary,
			VerticalTextAlignment = TextAlignment.Center,
			HeightRequest = Theme.FormFieldHeight,
			Padding = new Thickness(Theme.SpacingM, 0),
		};

		var border = new MauiBorder
		{
			Content = valueLabel,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = Theme.RadiusPill },
			BackgroundColor = Theme.SurfaceVariant,
		};

		stack.Add(border);
		return stack;
	}

	public static Microsoft.Maui.Controls.View MakePrimaryButton(string title, Action action)
	{
		var btn = new MauiButton
		{
			Text = title,
			FontFamily = Theme.FontSemibold,
			BackgroundColor = Theme.Primary,
			TextColor = Colors.White,
			FontSize = 16,
			FontAttributes = MauiFontAttributes.Bold,
			HeightRequest = Theme.ButtonHeight,
			CornerRadius = (int)Theme.RadiusPill,
		};
		btn.Clicked += (s, e) => action();
		return btn;
	}

	public static Microsoft.Maui.Controls.View MakeSecondaryButton(string title, Action action)
	{
		var btn = new MauiButton
		{
			Text = title,
			FontFamily = Theme.FontSemibold,
			BackgroundColor = Theme.SurfaceVariant,
			TextColor = Theme.Primary,
			FontSize = 16,
			FontAttributes = MauiFontAttributes.Bold,
			HeightRequest = Theme.ButtonHeight,
			CornerRadius = (int)Theme.RadiusPill,
		};
		btn.Clicked += (s, e) => action();
		return btn;
	}

	public static Microsoft.Maui.Controls.View MakeDangerButton(string title, Action action)
	{
		var btn = new MauiButton
		{
			Text = title,
			FontFamily = Theme.FontSemibold,
			BackgroundColor = Theme.Error,
			TextColor = Colors.White,
			FontSize = 16,
			FontAttributes = MauiFontAttributes.Bold,
			HeightRequest = Theme.ButtonHeight,
			CornerRadius = (int)Theme.RadiusPill,
		};
		btn.Clicked += (s, e) => action();
		return btn;
	}

	public static Microsoft.Maui.Controls.View MakeEmptyState(string icon, string title, string description)
	{
		var stack = new VerticalStackLayout
		{
			Spacing = 12,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Padding = new Thickness(Theme.SpacingXL),
		};
		stack.Add(new MauiLabel { Text = icon, FontFamily = Icons.FontFamily, FontSize = 48, HorizontalTextAlignment = TextAlignment.Center });
		stack.Add(new MauiLabel { Text = title, FontFamily = Theme.FontSemibold, FontSize = 18, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary, HorizontalTextAlignment = TextAlignment.Center });
		stack.Add(new MauiLabel { Text = description, FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center });
		return stack;
	}

	public static Microsoft.Maui.Controls.View MakeListCard(string title, string? subtitle, string? detail, Action? onTap)
	{
		var grid = new MauiGrid
		{
			ColumnDefinitions =
			{
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto),
			},
		};

		var infoStack = new VerticalStackLayout { Spacing = 2 };
		infoStack.Add(new MauiLabel { Text = title, FontFamily = Theme.FontSemibold, FontSize = 16, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary });
		if (subtitle != null)
			infoStack.Add(new MauiLabel { Text = subtitle, FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary });
		if (detail != null)
			infoStack.Add(new MauiLabel { Text = detail, FontFamily = Theme.FontRegular, FontSize = 12, TextColor = Theme.TextMuted });

		grid.Add(infoStack, 0, 0);
		grid.Add(new MauiLabel { Text = Icons.ChevronRight, FontFamily = Icons.FontFamily, FontSize = 20, TextColor = Theme.TextMuted, VerticalTextAlignment = TextAlignment.Center, Padding = new Thickness(Theme.SpacingS, 0) }, 1, 0);

		var border = new MauiBorder
		{
			Content = grid,
			BackgroundColor = Theme.CardBackground,
			Stroke = new SolidColorBrush(Theme.CardStroke),
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = Theme.RadiusCard },
			Padding = new Thickness(Theme.SpacingM),
		};

		if (onTap != null)
		{
			var tap = new TapGestureRecognizer();
			tap.Tapped += (s, e) => onTap();
			border.GestureRecognizers.Add(tap);
		}

		return border;
	}

	public static Microsoft.Maui.Controls.View MakeFormPicker(string label, int selectedIndex, string[] items, Action<int> onChanged)
	{
		var stack = new VerticalStackLayout { Spacing = 4 };
		stack.Add(new MauiLabel { Text = label, FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary });

		var picker = new MauiPicker
		{
			TextColor = Theme.TextPrimary,
			BackgroundColor = Theme.SurfaceVariant,
			HeightRequest = Theme.FormFieldHeight,
		};
		foreach (var item in items) picker.Items.Add(item);
		if (selectedIndex >= 0 && selectedIndex < items.Length) picker.SelectedIndex = selectedIndex;
		picker.SelectedIndexChanged += (s, e) => onChanged(picker.SelectedIndex);

		var border = new MauiBorder
		{
			Content = picker,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = Theme.RadiusPill },
			BackgroundColor = Theme.SurfaceVariant,
		};

		stack.Add(border);
		return stack;
	}

	public static Microsoft.Maui.Controls.View MakeFormSlider(string label, double value, double min, double max, Action<double> onChanged)
	{
		var headerStack = new HorizontalStackLayout();
		headerStack.Add(new MauiLabel { Text = label, FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary, HorizontalOptions = LayoutOptions.Start });
		var valueLabel = new MauiLabel { Text = $"{value:F1}", FontFamily = Theme.FontSemibold, FontSize = 14, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary, HorizontalOptions = LayoutOptions.End };
		headerStack.Add(valueLabel);

		var slider = new MauiSlider { Minimum = min, Maximum = max, Value = value, MinimumTrackColor = Theme.Primary, MaximumTrackColor = Theme.SurfaceVariant };
		slider.ValueChanged += (s, e) =>
		{
			valueLabel.Text = $"{e.NewValue:F1}";
			onChanged(e.NewValue);
		};

		var contentStack = new VerticalStackLayout { Spacing = 4 };

		var headerGrid = new MauiGrid();
		headerGrid.Add(new MauiLabel { Text = label, FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary, HorizontalOptions = LayoutOptions.Start });
		headerGrid.Add(valueLabel);
		valueLabel.HorizontalOptions = LayoutOptions.End;

		contentStack.Add(headerGrid);
		contentStack.Add(slider);

		return MakeCard(contentStack);
	}

	public static Microsoft.Maui.Controls.View MakeFormEditor(string label, string value, Action<string> onChanged)
	{
		var stack = new VerticalStackLayout { Spacing = 4 };
		stack.Add(new MauiLabel { Text = label, FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary });

		var editor = new MauiEditor
		{
			Text = value,
			FontSize = 16,
			TextColor = Theme.TextPrimary,
			BackgroundColor = Theme.SurfaceVariant,
			HeightRequest = 80,
		};
		editor.TextChanged += (s, e) => onChanged(e.NewTextValue ?? "");

		var border = new MauiBorder
		{
			Content = editor,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = Theme.RadiusEditor },
			BackgroundColor = Theme.SurfaceVariant,
		};

		stack.Add(border);
		return stack;
	}

	public static Microsoft.Maui.Controls.View MakeToggleRow(string label, bool isOn, Action<bool> onChanged)
	{
		var grid = new MauiGrid
		{
			ColumnDefinitions =
			{
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto),
			},
		};

		grid.Add(new MauiLabel { Text = label, FontFamily = Theme.FontSemibold, FontSize = 14, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary, VerticalTextAlignment = TextAlignment.Center }, 0, 0);

		var toggle = new Microsoft.Maui.Controls.Switch { IsToggled = isOn, OnColor = Theme.Primary };
		toggle.Toggled += (s, e) => onChanged(e.Value);
		grid.Add(toggle, 1, 0);

		return MakeCard(grid);
	}
}
