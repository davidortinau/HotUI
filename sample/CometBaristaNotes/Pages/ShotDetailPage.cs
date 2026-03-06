using Comet;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiBorder = Microsoft.Maui.Controls.Border;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;
using MauiGrid = Microsoft.Maui.Controls.Grid;
using MauiBoxView = Microsoft.Maui.Controls.BoxView;
using MauiFontAttributes = Microsoft.Maui.Controls.FontAttributes;

namespace CometBaristaNotes.Pages;

/// <summary>
/// Displays the details of a previously logged shot.
/// Navigated to from the Activity feed when tapping a shot card.
/// </summary>
public class ShotDetailPage : Comet.View
{
	readonly int _shotId;
	ShotRecord? _shot;

	public ShotDetailPage(int shotId)
	{
		_shotId = shotId;
		_shot = InMemoryDataStore.Instance?.GetShot(shotId);
	}

	[Body]
	Comet.View body()
	{
		if (_shot == null)
		{
			var emptyStack = new VerticalStackLayout
			{
				BackgroundColor = Theme.Background,
				VerticalOptions = LayoutOptions.Fill,
			};
			emptyStack.Add(FormHelpers.MakeEmptyState(Icons.Coffee, "Shot Not Found", "This shot could not be loaded."));
			return new MauiViewHost(emptyStack);
		}

		var content = new VerticalStackLayout
		{
			Spacing = Theme.SpacingS,
			Padding = new Thickness(Theme.SpacingM),
		};

		// Header with date and drink type
		content.Add(new MauiLabel
		{
			Text = _shot.Timestamp.ToString("dddd, MMMM d 'at' h:mm tt"),
			FontFamily = Theme.FontRegular,
			FontSize = 14,
			TextColor = Theme.TextSecondary,
		});

		content.Add(new MauiLabel
		{
			Text = _shot.DrinkType,
			FontFamily = Theme.FontSemibold,
			FontSize = 28,
			TextColor = Theme.TextPrimary,
		});

		content.Add(new MauiBoxView { HeightRequest = 8, BackgroundColor = Colors.Transparent });

		// Dose card
		content.Add(BuildCard("Dose",
			BuildStatRow("In", $"{_shot.DoseIn:F1}g"),
			BuildStatRow("Out", _shot.ActualOutput.HasValue ? $"{_shot.ActualOutput.Value:F1}g" : "—"),
			BuildStatRow("Ratio", _shot.DoseIn > 0 && _shot.ActualOutput.HasValue
				? $"1:{(_shot.ActualOutput.Value / _shot.DoseIn):F1}"
				: "—")
		));

		// Time card
		content.Add(BuildCard("Extraction",
			BuildStatRow("Expected", $"{_shot.ExpectedTime:F0}s"),
			BuildStatRow("Actual", _shot.ActualTime.HasValue ? $"{_shot.ActualTime.Value:F1}s" : "—"),
			BuildStatRow("Grind", string.IsNullOrEmpty(_shot.GrindSetting) ? "—" : _shot.GrindSetting)
		));

		// Equipment card
		if (!string.IsNullOrEmpty(_shot.MachineName) || !string.IsNullOrEmpty(_shot.GrinderName))
		{
			var equipmentRows = new List<Microsoft.Maui.Controls.View>();
			if (!string.IsNullOrEmpty(_shot.MachineName))
				equipmentRows.Add(BuildStatRow("Machine", _shot.MachineName));
			if (!string.IsNullOrEmpty(_shot.GrinderName))
				equipmentRows.Add(BuildStatRow("Grinder", _shot.GrinderName));
			content.Add(BuildCard("Equipment", equipmentRows.ToArray()));
		}

		// Bean card
		if (!string.IsNullOrEmpty(_shot.BeanName))
		{
			content.Add(BuildCard("Coffee",
				BuildStatRow("Bean", _shot.BeanName),
				BuildStatRow("Bag", _shot.BagDisplayName ?? "—")
			));
		}

		// Rating
		if (_shot.Rating.HasValue && _shot.Rating.Value >= 0 && _shot.Rating.Value <= 4)
		{
			var sentiments = new[]
			{
				Icons.SentimentVeryDissatisfied,
				Icons.SentimentDissatisfied,
				Icons.SentimentNeutral,
				Icons.SentimentSatisfied,
				Icons.SentimentVerySatisfied,
			};
			var ratingRow = new HorizontalStackLayout { Spacing = Theme.SpacingS };
			for (int i = 0; i < sentiments.Length; i++)
			{
				ratingRow.Add(new MauiLabel
				{
					Text = sentiments[i],
					FontFamily = Icons.FontFamily,
					FontSize = 28,
					TextColor = i == _shot.Rating.Value ? Theme.Primary : Theme.StarEmpty,
				});
			}
			content.Add(BuildCard("Rating", ratingRow));
		}

		// Tasting notes
		if (!string.IsNullOrEmpty(_shot.TastingNotes))
		{
			content.Add(BuildCard("Tasting Notes",
				new MauiLabel
				{
					Text = _shot.TastingNotes,
					FontFamily = Theme.FontRegular,
					FontSize = 16,
					TextColor = Theme.TextPrimary,
					LineBreakMode = LineBreakMode.WordWrap,
				}
			));
		}

		// Bottom padding
		content.Add(new MauiBoxView { HeightRequest = 40, BackgroundColor = Colors.Transparent });

		var scrollView = new MauiScrollView
		{
			Content = content,
			BackgroundColor = Theme.Background,
		};

		return new MauiViewHost(scrollView);
	}

	static MauiBorder BuildCard(string title, params Microsoft.Maui.Controls.View[] children)
	{
		var stack = new VerticalStackLayout { Spacing = Theme.SpacingS, Padding = new Thickness(Theme.SpacingM) };
		stack.Add(new MauiLabel
		{
			Text = title.ToUpperInvariant(),
			FontFamily = Theme.FontSemibold,
			FontSize = 11,
			TextColor = Theme.TextSecondary,
			CharacterSpacing = 1.2,
		});
		foreach (var child in children)
			stack.Add(child);

		return new MauiBorder
		{
			Content = stack,
			BackgroundColor = Theme.Surface,
			StrokeThickness = 0,
			StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
			Margin = new Thickness(0, 4),
		};
	}

	static MauiGrid BuildStatRow(string label, string value)
	{
		var grid = new MauiGrid();
		grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(100)));
		grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

		var labelView = new MauiLabel
		{
			Text = label,
			FontFamily = Theme.FontRegular,
			FontSize = 15,
			TextColor = Theme.TextSecondary,
			VerticalOptions = LayoutOptions.Center,
		};

		var valueView = new MauiLabel
		{
			Text = value,
			FontFamily = Theme.FontSemibold,
			FontSize = 15,
			TextColor = Theme.TextPrimary,
			VerticalOptions = LayoutOptions.Center,
		};
		MauiGrid.SetColumn(valueView, 1);

		grid.Add(labelView);
		grid.Add(valueView);
		return grid;
	}
}
