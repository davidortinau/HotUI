using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiBorder = Microsoft.Maui.Controls.Border;
using MauiGrid = Microsoft.Maui.Controls.Grid;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;
using MauiFontAttributes = Microsoft.Maui.Controls.FontAttributes;

namespace CometBaristaNotes.Components;

/// <summary>
/// Factory for creating shot record card using native MAUI controls.
/// </summary>
public static class ShotRecordCardFactory
{
	public static Microsoft.Maui.Controls.View Create(ShotRecord shot, Action? onTap = null)
	{
		var contentStack = new VerticalStackLayout { Spacing = 6 };

		// Header row: coffee icon + drink type + rating
		var headerGrid = new MauiGrid
		{
			ColumnDefinitions =
			{
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto),
			},
		};

		var headerLeft = new HorizontalStackLayout { Spacing = 6 };
		headerLeft.Add(new MauiLabel { Text = Icons.Coffee, FontFamily = Icons.FontFamily, FontSize = 18, TextColor = Theme.TextPrimary });
		headerLeft.Add(new MauiLabel { Text = shot.DrinkType, FontFamily = Theme.FontSemibold, FontSize = 16, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary });
		headerGrid.Add(headerLeft, 0, 0);

		// Rating stars
		var ratingLabel = MakeRatingBadge(shot);
		headerGrid.Add(ratingLabel, 1, 0);

		contentStack.Add(headerGrid);

		// Bean name
		var beanName = shot.BeanName ?? shot.BagDisplayName ?? "Unknown Bean";
		contentStack.Add(new MauiLabel { Text = beanName, FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary });

		// Recipe line
		contentStack.Add(new MauiLabel { Text = FormatRecipeLine(shot), FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary });

		// Footer: timestamp + user
		var footerStack = new HorizontalStackLayout { Spacing = 4 };
		footerStack.Add(new MauiLabel { Text = FormatTimestamp(shot), FontFamily = Theme.FontRegular, FontSize = 12, TextColor = Theme.TextMuted });
		if (shot.MadeByName != null)
			footerStack.Add(new MauiLabel { Text = $"• By: {shot.MadeByName}", FontFamily = Theme.FontRegular, FontSize = 12, TextColor = Theme.TextMuted });
		if (shot.MadeForName != null)
			footerStack.Add(new MauiLabel { Text = $"• For: {shot.MadeForName}", FontFamily = Theme.FontRegular, FontSize = 12, TextColor = Theme.TextMuted });
		contentStack.Add(footerStack);

		var border = new MauiBorder
		{
			Content = contentStack,
			BackgroundColor = Theme.CardBackground,
			Stroke = new SolidColorBrush(Theme.CardStroke),
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = Theme.RadiusCard },
			Padding = new Thickness(Theme.SpacingM),
			Margin = new Thickness(Theme.SpacingM, Theme.SpacingXS),
		};

		if (onTap != null)
		{
			var tap = new TapGestureRecognizer();
			tap.Tapped += (s, e) => onTap();
			border.GestureRecognizers.Add(tap);
		}

		return border;
	}

	static Microsoft.Maui.Controls.View MakeRatingBadge(ShotRecord shot)
	{
		if (!shot.Rating.HasValue)
			return new MauiLabel { Text = "—", FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextMuted };

		var sentiments = new[] { Icons.SentimentVeryDissatisfied, Icons.SentimentDissatisfied, Icons.SentimentNeutral, Icons.SentimentSatisfied, Icons.SentimentVerySatisfied };
		var idx = Math.Clamp(shot.Rating.Value - 1, 0, sentiments.Length - 1);
		return new MauiLabel { Text = sentiments[idx], FontFamily = Icons.FontFamily, FontSize = 18, TextColor = Theme.StarFilled };
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
