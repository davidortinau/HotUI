using Comet;
using CometBaristaNotes.Models;

namespace CometBaristaNotes.Components;

/// <summary>
/// Factory for creating shot record card using Comet fluent UI.
/// </summary>
public static class ShotRecordCardFactory
{
	public static View Create(ShotRecord shot, Action? onTap = null)
	{
		var beanName = shot.BeanName ?? shot.BagDisplayName ?? "Unknown Bean";

		var footer = BuildFooter(shot);

		var card = Border(
			VStack(6,
				Grid(columns: new object[] { "*", "Auto" }, rows: new object[] { "Auto" },
					HStack(6,
						Text(Icons.Coffee)
							.FontFamily(Icons.FontFamily)
							.FontSize(18)
							.Color(Theme.TextPrimary),
						Text(shot.DrinkType)
							.FontFamily(Theme.FontSemibold)
							.FontWeight(FontWeight.Bold)
							.FontSize(16)
							.Color(Theme.TextPrimary)
					)
					.Cell(row: 0, column: 0),

					MakeRatingBadge(shot)
						.Cell(row: 0, column: 1)
				),

				Text(beanName)
					.FontFamily(Theme.FontRegular)
					.FontSize(14)
					.Color(Theme.TextSecondary),

				Text(FormatRecipeLine(shot))
					.FontFamily(Theme.FontRegular)
					.FontSize(14)
					.Color(Theme.TextSecondary),

				footer
			)
		)
		.CornerRadius(Theme.RadiusCard)
		.Background(Theme.CardBackground)
		.StrokeColor(Theme.CardStroke)
		.StrokeThickness(1)
		.Padding(new Thickness(Theme.SpacingM))
		.Margin(new Thickness(Theme.SpacingM, Theme.SpacingXS));

		if (onTap != null)
			card.OnTap(_ => onTap());

		return card;
	}

	static View BuildFooter(ShotRecord shot)
	{
		var items = new List<View>
		{
			Text(FormatTimestamp(shot))
				.FontFamily(Theme.FontRegular)
				.FontSize(12)
				.Color(Theme.TextMuted),
		};

		if (shot.MadeByName != null)
			items.Add(Text($"• By: {shot.MadeByName}")
				.FontFamily(Theme.FontRegular)
				.FontSize(12)
				.Color(Theme.TextMuted));

		if (shot.MadeForName != null)
			items.Add(Text($"• For: {shot.MadeForName}")
				.FontFamily(Theme.FontRegular)
				.FontSize(12)
				.Color(Theme.TextMuted));

		var stack = HStack(4);
		foreach (var item in items)
			stack.Add(item);

		return stack;
	}

	static View MakeRatingBadge(ShotRecord shot)
	{
		if (!shot.Rating.HasValue)
			return Text("—")
				.FontFamily(Theme.FontRegular)
				.FontSize(14)
				.Color(Theme.TextMuted);

		var sentiments = new[] { Icons.SentimentVeryDissatisfied, Icons.SentimentDissatisfied, Icons.SentimentNeutral, Icons.SentimentSatisfied, Icons.SentimentVerySatisfied };
		var idx = Math.Clamp(shot.Rating.Value - 1, 0, sentiments.Length - 1);
		return Text(sentiments[idx])
			.FontFamily(Icons.FontFamily)
			.FontSize(18)
			.Color(Theme.StarFilled);
	}

	static string FormatRecipeLine(ShotRecord shot)
	{
		var doseIn = $"{shot.DoseIn:F1}g in";
		var doseOut = shot.ActualOutput.HasValue ? $"{shot.ActualOutput:F1}g out" : "—";
		var time = shot.ActualTime.HasValue ? $"({shot.ActualTime:F1}s)" : "";
		return $"{doseIn} → {doseOut} {time}".Trim();
	}

	static string FormatTimestamp(ShotRecord shot)
	{
		var diff = DateTime.Now - shot.Timestamp;
		if (diff.TotalMinutes < 1) return "Just now";
		if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
		if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
		if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
		return shot.Timestamp.ToString("MMM d");
	}
}
