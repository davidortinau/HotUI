using CometBaristaNotes.Components;
using CometBaristaNotes.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using UXDivers.Popups.Maui.Controls;
using UXDivers.Popups.Services;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiBoxView = Microsoft.Maui.Controls.BoxView;
using MauiButton = Microsoft.Maui.Controls.Button;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;
using MauiBorder = Microsoft.Maui.Controls.Border;

namespace CometBaristaNotes.Pages;

public class ShotFilterPopup : ActionModalPopup
{
	readonly ShotFilterCriteria _working;
	readonly List<(int Id, string Name)> _beans;
	readonly List<(int Id, string Name)> _people;
	readonly Action<ShotFilterCriteria> _onApply;
	readonly Action _onClear;

	// Track chip buttons so we can toggle visuals
	readonly Dictionary<string, MauiButton> _chipButtons = new();

	public ShotFilterPopup(
		ShotFilterCriteria current,
		List<(int Id, string Name)> beans,
		List<(int Id, string Name)> people,
		Action<ShotFilterCriteria> onApply,
		Action onClear)
	{
		_working = current.Clone();
		_beans = beans;
		_people = people;
		_onApply = onApply;
		_onClear = onClear;

		BuildContent();
	}

	void BuildContent()
	{
		var root = new VerticalStackLayout
		{
			Spacing = Theme.SpacingM,
			Padding = new Thickness(Theme.SpacingL),
		};

		// Title
		root.Add(new MauiLabel
		{
			Text = "Filter Shots",
			FontFamily = Theme.FontSemibold,
			FontSize = 20,
			TextColor = Theme.TextPrimary,
			HorizontalTextAlignment = TextAlignment.Center,
		});

		root.Add(MakeDivider());

		// Bean section
		if (_beans.Count > 0)
		{
			root.Add(MakeSectionHeader("Bean"));
			root.Add(MakeChipGroup("bean", _beans, _working.BeanIds));
		}

		// People section
		if (_people.Count > 0)
		{
			root.Add(MakeSectionHeader("Made For"));
			root.Add(MakeChipGroup("person", _people, _working.MadeForIds));
		}

		// Rating section
		root.Add(MakeSectionHeader("Rating"));
		var ratingOptions = new List<(int Id, string Name)>
		{
			(1, $"{Icons.SentimentVeryDissatisfied} 1"),
			(2, $"{Icons.SentimentDissatisfied} 2"),
			(3, $"{Icons.SentimentNeutral} 3"),
			(4, $"{Icons.SentimentSatisfied} 4"),
			(5, $"{Icons.SentimentVerySatisfied} 5"),
		};
		root.Add(MakeChipGroup("rating", ratingOptions, _working.Ratings));

		root.Add(MakeDivider());

		// Button row
		var buttonRow = new HorizontalStackLayout
		{
			Spacing = Theme.SpacingS,
			HorizontalOptions = LayoutOptions.Center,
		};

		var clearBtn = new MauiButton
		{
			Text = "Clear All",
			FontFamily = Theme.FontRegular,
			FontSize = 14,
			BackgroundColor = Colors.Transparent,
			TextColor = Theme.Error,
			BorderColor = Theme.Error,
			BorderWidth = 1,
			CornerRadius = (int)Theme.RadiusPill,
			HeightRequest = 40,
			Padding = new Thickness(Theme.SpacingM, 0),
		};
		clearBtn.Clicked += (s, e) =>
		{
			_working.Clear();
			foreach (var kvp in _chipButtons)
			{
				StyleChip(kvp.Value, false);
			}
		};

		var applyBtn = new MauiButton
		{
			Text = "Apply",
			FontFamily = Theme.FontSemibold,
			FontSize = 14,
			BackgroundColor = Theme.Primary,
			TextColor = Colors.White,
			CornerRadius = (int)Theme.RadiusPill,
			HeightRequest = 40,
			Padding = new Thickness(Theme.SpacingL, 0),
		};
		applyBtn.Clicked += async (s, e) =>
		{
			_onApply(_working);
			try { await IPopupService.Current.PopAsync(); } catch { }
		};

		buttonRow.Add(clearBtn);
		buttonRow.Add(applyBtn);
		root.Add(buttonRow);

		var scrollView = new MauiScrollView
		{
			Content = root,
			MaximumHeightRequest = 500,
		};

		var frame = new MauiBorder
		{
			Content = scrollView,
			BackgroundColor = Theme.Surface,
			Stroke = Theme.Outline,
			StrokeThickness = 1,
			StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
			{
				CornerRadius = new CornerRadius(Theme.RadiusCard),
			},
			Padding = 0,
			Margin = new Thickness(Theme.SpacingL),
		};

		Content = frame;
	}

	MauiLabel MakeSectionHeader(string text) => new()
	{
		Text = text,
		FontFamily = Theme.FontSemibold,
		FontSize = 14,
		TextColor = Theme.TextSecondary,
	};

	Microsoft.Maui.Controls.View MakeDivider() => new MauiBoxView
	{
		HeightRequest = 1,
		BackgroundColor = Theme.Outline,
		HorizontalOptions = LayoutOptions.Fill,
	};

	Microsoft.Maui.Controls.FlexLayout MakeChipGroup(string category, List<(int Id, string Name)> items, List<int> selectedIds)
	{
		var flex = new Microsoft.Maui.Controls.FlexLayout
		{
			Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
			JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start,
			AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Center,
		};

		foreach (var item in items)
		{
			var isSelected = selectedIds.Contains(item.Id);
			var chip = new MauiButton
			{
				Text = item.Name,
				FontFamily = Theme.FontRegular,
				FontSize = 13,
				CornerRadius = (int)Theme.RadiusPill,
				HeightRequest = 34,
				Padding = new Thickness(12, 0),
				Margin = new Thickness(0, 0, Theme.SpacingXS, Theme.SpacingXS),
				BorderWidth = 1,
			};

			StyleChip(chip, isSelected);

			var capturedId = item.Id;
			chip.Clicked += (s, e) =>
			{
				if (selectedIds.Contains(capturedId))
				{
					selectedIds.Remove(capturedId);
					StyleChip(chip, false);
				}
				else
				{
					selectedIds.Add(capturedId);
					StyleChip(chip, true);
				}
			};

			var key = $"{category}_{item.Id}";
			_chipButtons[key] = chip;
			flex.Add(chip);
		}

		return flex;
	}

	static void StyleChip(MauiButton chip, bool selected)
	{
		if (selected)
		{
			chip.BackgroundColor = Theme.Primary;
			chip.TextColor = Colors.White;
			chip.BorderColor = Theme.Primary;
		}
		else
		{
			chip.BackgroundColor = Theme.SurfaceElevated;
			chip.TextColor = Theme.TextPrimary;
			chip.BorderColor = Theme.Outline;
		}
	}
}
