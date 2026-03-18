using Comet;
using CometBaristaNotes.Models;

namespace CometBaristaNotes.Components;

/// <summary>
/// Factory for creating rating display using Comet fluent UI.
/// </summary>
public static class RatingDisplayFactory
{
	public static View Create(RatingAggregate rating)
	{
		return Border(
			HStack(12,
				MakeStatBlock("Avg", rating.RatedShots > 0 ? $"{rating.AverageRating:F1}" : "—"),
				MakeStatBlock("Shots", $"{rating.TotalShots}"),
				MakeStatBlock("Best", rating.BestRating?.ToString() ?? "—"),
				MakeStatBlock("Worst", rating.WorstRating?.ToString() ?? "—")
			)
		)
		.CornerRadius(Theme.RadiusCard)
		.Background(Theme.CardBackground)
		.StrokeColor(Theme.CardStroke)
		.StrokeThickness(1)
		.Padding(new Thickness(Theme.SpacingM));
	}

	static View MakeStatBlock(string label, string value)
	{
		return VStack(2,
			Text(value)
				.FontFamily(Theme.FontSemibold)
				.FontWeight(FontWeight.Bold)
				.FontSize(20)
				.Color(Theme.TextPrimary),
			Text(label)
				.FontFamily(Theme.FontRegular)
				.FontSize(12)
				.Color(Theme.TextMuted)
		);
	}
}
