using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;

namespace CometBaristaNotes.Components;

public class RatingDisplay : Comet.View
{
	readonly RatingAggregate _rating;

	public RatingDisplay(RatingAggregate rating)
	{
		_rating = rating;
	}

	[Body]
	Comet.View body() =>
		new HStack(spacing: 16)
		{
			StatBlock("Avg", _rating.RatedShots > 0 ? $"{_rating.AverageRating:F1}" : "—"),
			StatBlock("Shots", $"{_rating.TotalShots}"),
			StatBlock("Best", _rating.BestRating?.ToString() ?? "—"),
			StatBlock("Worst", _rating.WorstRating?.ToString() ?? "—"),
		}.Padding(12)
		 .Background(Theme.Surface)
		 .RoundedBorder(radius: Theme.RadiusCard, color: Theme.Outline, strokeSize: 1);

	static Comet.View StatBlock(string label, string value) =>
		new VStack(spacing: 2)
		{
			new Text(value).FontSize(20).FontWeight(FontWeight.Bold).Color(Theme.TextPrimary),
			new Text(label).FontSize(12).Color(Theme.TextMuted)
		};
}
