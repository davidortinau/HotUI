using Comet;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;
using MauiBoxView = Microsoft.Maui.Controls.BoxView;
using MauiFontAttributes = Microsoft.Maui.Controls.FontAttributes;

namespace CometBaristaNotes.Pages;

public class ActivityFeedPage : Comet.View
{
	[State] readonly State<List<ShotRecord>> _shots = new();
	[State] readonly State<bool> _isLoading = new(true);

	[Body]
	Comet.View body()
	{
		if (_isLoading.Value)
		{
			LoadShots();
		}

		var shots = _shots.Value ?? new List<ShotRecord>();

		if (shots.Count == 0 && !_isLoading.Value)
		{
			var emptyStack = new VerticalStackLayout
			{
				BackgroundColor = Theme.Background,
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill,
			};
			emptyStack.Add(FormHelpers.MakeEmptyState(Icons.Coffee, "No Shots Yet", "Log your first espresso shot to see it here."));
			return new MauiViewHost(emptyStack);
		}

		var contentStack = new VerticalStackLayout { Spacing = 0 };

		// Spacer
		contentStack.Add(new MauiBoxView { HeightRequest = 20, BackgroundColor = Colors.Transparent });

		// Shot count header
		contentStack.Add(new MauiLabel
		{
			Text = $"{shots.Count} shots logged",
			FontFamily = Theme.FontSemibold,
			FontSize = 14,
			FontAttributes = MauiFontAttributes.Bold,
			TextColor = Theme.TextSecondary,
			Margin = new Thickness(Theme.SpacingM, Theme.SpacingS),
		});

		// Shot cards
		foreach (var shot in shots)
		{
			contentStack.Add(ShotRecordCardFactory.Create(shot, () =>
			{
				// Could navigate to shot detail
			}));
		}

		var scrollView = new MauiScrollView
		{
			Content = contentStack,
			BackgroundColor = Theme.Background,
		};

		return new MauiViewHost(scrollView);
	}

	void LoadShots()
	{
		var store = InMemoryDataStore.Instance;
		if (store != null)
		{
			_shots.Value = store.GetAllShots();
		}
		_isLoading.Value = false;
	}
}
