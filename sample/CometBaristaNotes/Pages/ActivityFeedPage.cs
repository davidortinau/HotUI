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
				"Log your first espresso shot to see it here.");
		}

		return new Comet.ScrollView
		{
			new VStack(spacing: 12)
			{
				new Text($"{shots.Count} shots")
					.FontSize(13).Color(Colors.Gray)
					.Padding(new Thickness(16, 8, 16, 0)),
				shots.Select(shot =>
					new ShotRecordCard(shot, () =>
					{
						// Tap navigates; future: shot detail
					}) as Comet.View
				).ToArray()
			}.Padding(16)
		};
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
