using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

namespace CometBaristaNotes.Pages;

public class BeanDetailPageState
{
	public string Name { get; set; } = "";
	public string Roaster { get; set; } = "";
	public string Origin { get; set; } = "";
	public string Notes { get; set; } = "";
	public bool IsLoaded { get; set; }
	public string Error { get; set; } = "";
	public List<Bag> Bags { get; set; } = new();
	public RatingAggregate Rating { get; set; } = new();
	public List<ShotRecord> AllShots { get; set; } = new();
	public int VisibleShotCount { get; set; } = 10;
}

public class BeanDetailPage : Component<BeanDetailPageState>
{
	readonly int _beanId;
	const int ShotsPageSize = 10;

	public BeanDetailPage(int beanId = 0) { _beanId = beanId; }

	void LoadBean()
	{
		if (_beanId <= 0) { SetState(s => s.IsLoaded = true); return; }

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		var bean = store.GetBean(_beanId);
		if (bean == null)
		{
			SetState(s => { s.Error = "Bean not found"; s.IsLoaded = true; });
			return;
		}

		SetState(s =>
		{
			s.Name = bean.Name;
			s.Roaster = bean.Roaster ?? "";
			s.Origin = bean.Origin ?? "";
			s.Notes = bean.Notes ?? "";
			s.Bags = store.GetBagsForBean(_beanId);
			s.Rating = store.GetBeanRating(_beanId);
			s.AllShots = store.GetShotsByBean(_beanId);
			s.IsLoaded = true;
		});
	}

	void Save()
	{
		if (string.IsNullOrWhiteSpace(State.Name))
		{
			SetState(s => s.Error = "Bean name is required");
			return;
		}
		SetState(s => s.Error = "");

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		if (_beanId > 0)
		{
			store.UpdateBean(new Bean
			{
				Id = _beanId,
				Name = State.Name,
				Roaster = string.IsNullOrWhiteSpace(State.Roaster) ? null : State.Roaster,
				Origin = string.IsNullOrWhiteSpace(State.Origin) ? null : State.Origin,
				Notes = string.IsNullOrWhiteSpace(State.Notes) ? null : State.Notes,
				IsActive = true
			});
		}
		else
		{
			store.CreateBean(new Bean
			{
				Name = State.Name,
				Roaster = string.IsNullOrWhiteSpace(State.Roaster) ? null : State.Roaster,
				Origin = string.IsNullOrWhiteSpace(State.Origin) ? null : State.Origin,
				Notes = string.IsNullOrWhiteSpace(State.Notes) ? null : State.Notes,
			});
		}

		Navigation?.Pop();
	}

	async void DeleteBean()
	{
		var page = Services.PageHelper.GetCurrentPage();
		if (page == null) return;

		var confirmed = await page.DisplayAlertAsync(
			"Delete Bean",
			$"Are you sure you want to delete \"{State.Name}\"? This will also archive all associated bags.",
			"Delete", "Cancel");

		if (!confirmed) return;

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		store.ArchiveBean(_beanId);
		Navigation?.Pop();
	}

	public override View Render()
	{
		if (!State.IsLoaded)
			LoadBean();

		var isEdit = _beanId > 0;

		var items = new List<View>();

		items.Add(FormHelpers.MakeSectionHeader(isEdit ? "EDIT BEAN" : "NEW BEAN"));
		items.Add(FormHelpers.MakeFormEntry("Name *", State.Name, "Bean name", v => SetState(s => s.Name = v)));
		items.Add(FormHelpers.MakeFormEntry("Roaster", State.Roaster, "Roaster name", v => SetState(s => s.Roaster = v)));
		items.Add(FormHelpers.MakeFormEntry("Origin", State.Origin, "Country or region", v => SetState(s => s.Origin = v)));
		items.Add(FormHelpers.MakeFormEntry("Notes", State.Notes, "Tasting notes, processing, etc.", v => SetState(s => s.Notes = v)));

		if (!string.IsNullOrEmpty(State.Error))
			items.Add(Text(State.Error).Color(Theme.Error).FontFamily(Theme.FontRegular).FontSize(14));

		items.Add(FormHelpers.MakePrimaryButton(isEdit ? "Save Changes" : "Create Bean", Save));

		if (isEdit)
		{
			items.Add(FormHelpers.MakeDangerButton("Delete Bean", DeleteBean));
			items.Add(FormHelpers.MakeSectionHeader("RATINGS"));
			items.Add(RatingDisplayFactory.Create(State.Rating));
			items.Add(BuildRatingDistribution());

			items.Add(FormHelpers.MakeSectionHeader("BAGS"));
			if (State.Bags.Count == 0)
				items.Add(Text("No bags added yet").FontFamily(Theme.FontRegular).FontSize(14).Color(Theme.TextSecondary));

			items.Add(FormHelpers.MakeSecondaryButton("+ Add Bag", () =>
			{
				Navigation?.Navigate(new BagFormPage(_beanId));
			}));

			foreach (var bag in State.Bags)
			{
				items.Add(BuildBagCard(bag));
			}

			// Shot History
			items.Add(FormHelpers.MakeSectionHeader("SHOT HISTORY"));
			var shots = State.AllShots;
			if (shots.Count == 0)
			{
				items.Add(Text("No shots recorded yet").FontFamily(Theme.FontRegular).FontSize(14).Color(Theme.TextSecondary));
			}
			else
			{
				var visible = shots.Take(State.VisibleShotCount).ToList();
				foreach (var shot in visible)
				{
					var shotId = shot.Id;
					items.Add(ShotRecordCardFactory.Create(shot, () =>
					{
						Navigation?.Navigate(new ShotLoggingPage(shotId));
					}));
				}

				if (State.VisibleShotCount < shots.Count)
				{
					items.Add(FormHelpers.MakeSecondaryButton(
						$"Load More ({shots.Count - State.VisibleShotCount} remaining)",
						() => { SetState(s => s.VisibleShotCount += ShotsPageSize); }));
				}
			}
		}

		var stack = VStack(Theme.SpacingS);
		foreach (var item in items) stack.Add(item);

		return ScrollView(
			stack.Padding(new Thickness(Theme.SpacingM))
		)
		.Background(Theme.Background);
	}

	View BuildRatingDistribution()
	{
		var shots = State.AllShots;
		var sentiments = new[] { Icons.SentimentVeryDissatisfied, Icons.SentimentDissatisfied, Icons.SentimentNeutral, Icons.SentimentSatisfied, Icons.SentimentVerySatisfied };
		var sentimentColors = new[] { Theme.Error, Theme.Warning, Theme.TextMuted, Theme.Success, Theme.StarFilled };

		var counts = new int[5];
		foreach (var shot in shots)
		{
			if (shot.Rating.HasValue)
			{
				var idx = Math.Clamp(shot.Rating.Value - 1, 0, 4);
				counts[idx]++;
			}
		}
		var maxCount = counts.Max();

		var container = VStack(Theme.SpacingXS);

		for (var i = 4; i >= 0; i--)
		{
			var barFraction = maxCount > 0 ? (double)counts[i] / maxCount : 0;

			container.Add(
				Grid(columns: new object[] { 28, "*", 30 }, rows: new object[] { "Auto" },
					Text(sentiments[i])
						.FontFamily(Icons.FontFamily)
						.FontSize(18)
						.Color(sentimentColors[i])
						.HorizontalTextAlignment(TextAlignment.Center)
						.VerticalTextAlignment(TextAlignment.Center)
						.Cell(row: 0, column: 0),

					BuildBarOverlay(barFraction, sentimentColors[i])
						.Cell(row: 0, column: 1),

					Text(counts[i].ToString())
						.FontFamily(Theme.FontRegular)
						.FontSize(12)
						.Color(Theme.TextSecondary)
						.HorizontalTextAlignment(TextAlignment.End)
						.VerticalTextAlignment(TextAlignment.Center)
						.Cell(row: 0, column: 2)
				)
				.ColumnSpacing(Theme.SpacingS)
				.Frame(height: 24)
			);
		}

		return Border(
			container
		)
		.CornerRadius(Theme.RadiusCard)
		.Background(Theme.CardBackground)
		.StrokeColor(Theme.CardStroke)
		.StrokeThickness(1)
		.Padding(new Thickness(Theme.SpacingM))
		.Margin(new Thickness(0, Theme.SpacingXS, 0, 0));
	}

	View BuildBarOverlay(double fraction, Color fillColor)
	{
		return ProgressBar(fraction)
			.ProgressColor(fillColor)
			.TrackColor(Theme.SurfaceVariant)
			.Frame(height: 12);
	}

	View BuildBagCard(Bag bag)
	{
		var statsItems = new List<View>();
		statsItems.Add(Text($"{bag.ShotCount} shots").FontFamily(Theme.FontRegular).FontSize(12).Color(Theme.TextMuted));

		if (bag.AverageRating.HasValue)
			statsItems.Add(Text($"{Icons.SentimentVerySatisfied} {bag.AverageRating.Value:F1}").FontFamily(Icons.FontFamily).FontSize(12).Color(Theme.StarFilled));
		else
			statsItems.Add(Text("No ratings").FontFamily(Theme.FontRegular).FontSize(12).Color(Theme.TextMuted));

		statsItems.Add(Text(bag.IsComplete ? "Complete" : "Active").FontFamily(Theme.FontRegular).FontSize(12).Color(bag.IsComplete ? Theme.Success : Theme.Primary));

		var statsStack = HStack(12);
		foreach (var s in statsItems) statsStack.Add(s);

		var infoItems = new List<View>();
		infoItems.Add(Text($"Roasted {bag.RoastDate:MMM d, yyyy}").FontFamily(Theme.FontSemibold).FontSize(14).FontWeight(FontWeight.Bold).Color(Theme.TextPrimary));
		if (bag.Notes != null)
			infoItems.Add(Text(bag.Notes).FontFamily(Theme.FontRegular).FontSize(12).Color(Theme.TextSecondary));
		infoItems.Add(statsStack);

		var infoStack = VStack(4);
		foreach (var item in infoItems) infoStack.Add(item);

		return Border(
			Grid(columns: new object[] { "*", "Auto" }, rows: new object[] { "Auto" },
				infoStack.Cell(row: 0, column: 0),
				Text(Icons.ChevronRight)
					.FontFamily(Icons.FontFamily)
					.FontSize(20)
					.Color(Theme.TextMuted)
					.VerticalTextAlignment(TextAlignment.Center)
					.Cell(row: 0, column: 1)
			)
		)
		.CornerRadius(Theme.RadiusCard)
		.Background(Theme.CardBackground)
		.StrokeColor(Theme.CardStroke)
		.StrokeThickness(1)
		.Padding(new Thickness(Theme.SpacingM))
		.OnTap(_ => Navigation?.Navigate(new BagDetailPage(bag.Id)));
	}
}
