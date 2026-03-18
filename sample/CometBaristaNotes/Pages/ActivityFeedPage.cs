using CometBaristaNotes.Components;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;

namespace CometBaristaNotes.Pages;

public class ActivityFeedState
{
	public int RefreshVersion { get; set; }
}

public class ActivityFeedPage : Component<ActivityFeedState>
{
	const int MaxShots = 25;

	readonly IDataStore _store;
	readonly IDataChangeNotifier? _notifier;

	public ActivityFeedPage()
	{
		_store = IPlatformApplication.Current?.Services.GetService<IDataStore>() ?? (IDataStore)InMemoryDataStore.Instance;
		_notifier = IPlatformApplication.Current?.Services.GetService<IDataChangeNotifier>();
		if (_notifier != null)
		{
			_notifier.DataChanged += OnDataChanged;
		}
	}

	public override View Render()
	{
		_ = State.RefreshVersion;

		var shots = _store.GetAllShots().Take(MaxShots).ToList();
		if (shots.Count == 0)
		{
			return VStack(
				FormHelpers.MakeEmptyState(Icons.Feed, "No shots yet", "Log a new shot to seed the activity feed.")
			)
			.Background(Theme.Background)
			.FillHorizontal()
			.Title("Activity");
		}

		var content = VStack(Theme.SpacingS,
				BuildSummaryCard(shots),
				FormHelpers.MakeSectionHeader("Latest shots")
			);

		foreach (var shot in shots)
		{
			content.Add(BuildShotCard(shot));
		}

		return ScrollView(
				content.Padding(new Thickness(Theme.SpacingM))
			)
		.Background(Theme.Background)
		.Title("Activity");
	}

	protected override void OnWillUnmount()
	{
		if (_notifier != null)
		{
			_notifier.DataChanged -= OnDataChanged;
		}

		base.OnWillUnmount();
	}

	void OnDataChanged(string entityType, int entityId, DataChangeType changeType)
	{
		if (entityType != "Shot")
		{
			return;
		}

		MainThread.BeginInvokeOnMainThread(() =>
			SetState(state => state.RefreshVersion++));
	}

	View BuildSummaryCard(IReadOnlyList<ShotRecord> shots)
	{
		var latestShot = shots[0];
		var detail = $"{shots.Count} recent shots • latest {FormatTimestamp(latestShot.Timestamp)}";

		return FormHelpers.MakeCard(
			VStack(Theme.SpacingS,
				Text("Activity feed")
					.FontFamily(Theme.FontSemibold)
					.FontSize(18)
					.FontWeight(FontWeight.Bold)
					.Color(Theme.TextPrimary),

				Text(detail)
					.FontFamily(Theme.FontRegular)
					.FontSize(13)
					.Color(Theme.TextSecondary)
			));
	}

	View BuildShotCard(ShotRecord shot)
	{
		var title = shot.BeanName ?? shot.BagDisplayName ?? shot.DrinkType;
		var subtitle = $"{shot.DrinkType} • {FormatTimestamp(shot.Timestamp)}";
		var detail = $"{shot.DoseIn:0.#}g in • {(shot.ActualOutput ?? shot.ExpectedOutput):0.#}g out";

		return FormHelpers.MakeListCard(
			title,
			subtitle,
			detail,
			() => Navigation?.Navigate(new ShotLoggingPage(shot.Id)));
	}

	static string FormatTimestamp(DateTime timestamp)
	{
		var diff = DateTime.Now - timestamp;
		if (diff.TotalMinutes < 1)
		{
			return "just now";
		}

		if (diff.TotalMinutes < 60)
		{
			return $"{(int)diff.TotalMinutes}m ago";
		}

		if (diff.TotalHours < 24)
		{
			return $"{(int)diff.TotalHours}h ago";
		}

		if (diff.TotalDays < 7)
		{
			return $"{(int)diff.TotalDays}d ago";
		}

		return timestamp.ToString("MMM d");
	}
}
