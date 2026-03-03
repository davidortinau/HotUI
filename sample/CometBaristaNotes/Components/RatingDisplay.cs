using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiBorder = Microsoft.Maui.Controls.Border;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;
using MauiFontAttributes = Microsoft.Maui.Controls.FontAttributes;

namespace CometBaristaNotes.Components;

/// <summary>
/// Factory for creating rating display using native MAUI controls.
/// </summary>
public static class RatingDisplayFactory
{
	public static Microsoft.Maui.Controls.View Create(RatingAggregate rating)
	{
		var stack = new HorizontalStackLayout { Spacing = 12 };

		stack.Add(MakeStatBlock("Avg", rating.RatedShots > 0 ? $"{rating.AverageRating:F1}" : "—"));
		stack.Add(MakeStatBlock("Shots", $"{rating.TotalShots}"));
		stack.Add(MakeStatBlock("Best", rating.BestRating?.ToString() ?? "—"));
		stack.Add(MakeStatBlock("Worst", rating.WorstRating?.ToString() ?? "—"));

		return new MauiBorder
		{
			Content = stack,
			BackgroundColor = Theme.CardBackground,
			Stroke = new SolidColorBrush(Theme.CardStroke),
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = Theme.RadiusCard },
			Padding = new Thickness(Theme.SpacingM),
		};
	}

	static Microsoft.Maui.Controls.View MakeStatBlock(string label, string value)
	{
		var stack = new VerticalStackLayout { Spacing = 2 };
		stack.Add(new MauiLabel { Text = value, FontSize = 20, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary });
		stack.Add(new MauiLabel { Text = label, FontSize = 12, TextColor = Theme.TextMuted });
		return stack;
	}
}
