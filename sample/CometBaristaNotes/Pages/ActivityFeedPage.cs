using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

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
			return FormHelpers.EmptyState("☕", "No Shots Yet",
				"Log your first espresso shot to see it here.")
				.Background(Theme.Background);
		}

		return new Comet.ScrollView
		{
			new VStack(spacing: Theme.SpacingS)
			{
				new Text($"{shots.Count} shots")
					.FontSize(14).FontWeight(FontWeight.Semibold).Color(Theme.TextSecondary)
					.Padding(new Thickness(0, Theme.SpacingS, 0, 0)),
				shots.Select(shot =>
					new ShotRecordCard(shot, () =>
					{
					}) as Comet.View
				).ToArray()
			}.Padding(Theme.SpacingM)
		}.Background(Theme.Background);
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
