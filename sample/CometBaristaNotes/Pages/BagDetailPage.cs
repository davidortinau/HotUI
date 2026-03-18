using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

namespace CometBaristaNotes.Pages;

public class BagDetailPageState
{
	public string BeanName { get; set; } = "";
	public int BeanId { get; set; }
	public string RoastDate { get; set; } = "";
	public string Notes { get; set; } = "";
	public bool IsComplete { get; set; }
	public int ShotCount { get; set; }
	public bool IsLoaded { get; set; }
	public string Error { get; set; } = "";
	public RatingAggregate Rating { get; set; } = new();
}

public class BagDetailPage : Component<BagDetailPageState>
{
	readonly int _bagId;

	public BagDetailPage(int bagId = 0) { _bagId = bagId; }

	void LoadBag()
	{
		if (_bagId <= 0)
		{
			SetState(s => s.IsLoaded = true);
			return;
		}

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		var bag = store.GetBag(_bagId);
		if (bag == null)
		{
			SetState(s =>
			{
				s.Error = "Bag not found";
				s.IsLoaded = true;
			});
			return;
		}

		var rating = store.GetBagRating(_bagId);

		SetState(s =>
		{
			s.BeanName = bag.BeanName ?? "";
			s.BeanId = bag.BeanId;
			s.RoastDate = bag.RoastDate.ToString("yyyy-MM-dd");
			s.Notes = bag.Notes ?? "";
			s.IsComplete = bag.IsComplete;
			s.ShotCount = bag.ShotCount;
			s.Rating = rating;
			s.IsLoaded = true;
		});
	}

	void Save()
	{
		SetState(s => s.Error = "");

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		if (_bagId > 0)
		{
			store.UpdateBag(new Bag
			{
				Id = _bagId,
				BeanId = State.BeanId,
				RoastDate = DateTime.TryParse(State.RoastDate, out var d) ? d : DateTime.Now,
				Notes = string.IsNullOrWhiteSpace(State.Notes) ? null : State.Notes,
				IsComplete = State.IsComplete,
				IsActive = true
			});
		}
		Navigation?.Pop();
	}

	async void DeleteBag()
	{
		var page = CometBaristaNotes.Services.PageHelper.GetCurrentPage();
		if (page == null) return;

		var message = State.ShotCount > 0
			? $"This bag has {State.ShotCount} shot(s) logged. Deleting it will hide it from all lists. Continue?"
			: "Are you sure you want to delete this bag?";

		var confirmed = await page.DisplayAlertAsync("Delete Bag", message, "Delete", "Cancel");
		if (!confirmed) return;

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		store.ArchiveBag(_bagId);
		Navigation?.Pop();
	}

	void ReactivateBag()
	{
		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		store.ReactivateBag(_bagId);
		SetState(s => s.IsComplete = false);
	}

	public override View Render()
	{
		if (!State.IsLoaded)
			LoadBag();

		if (_bagId <= 0)
		{
			return VStack(
				Text("Bag not found")
					.FontFamily(Theme.FontRegular)
					.Color(Theme.TextSecondary)
			)
			.Padding(new Thickness(Theme.SpacingM))
			.Background(Theme.Background);
		}

		var items = new List<View>();

		items.Add(FormHelpers.MakeSectionHeader("BAG DETAILS"));
		items.Add(FormHelpers.MakeReadOnlyField("Bean", State.BeanName));
		items.Add(FormHelpers.MakeReadOnlyField("Roast Date", State.RoastDate));
		items.Add(FormHelpers.MakeFormEntryWithLimit("Notes", State.Notes, "Bag notes", 500, v => SetState(s => s.Notes = v)));

		// Shot count card
		items.Add(FormHelpers.MakeCard(
			VStack(2,
				Text("Shots Logged")
					.FontFamily(Theme.FontRegular)
					.FontSize(14)
					.Color(Theme.TextSecondary),
				Text($"{State.ShotCount}")
					.FontFamily(Theme.FontSemibold)
					.FontSize(24)
					.FontWeight(FontWeight.Bold)
					.Color(Theme.TextPrimary)
			)
		));

		// Status toggle card
		items.Add(FormHelpers.MakeToggleRow(
			State.IsComplete ? "Status: Complete" : "Status: Active",
			State.IsComplete,
			v => SetState(s => s.IsComplete = v)
		));

		if (!string.IsNullOrEmpty(State.Error))
			items.Add(Text(State.Error).Color(Theme.Error).FontSize(14));

		items.Add(FormHelpers.MakePrimaryButton("Save Changes", Save));

		if (State.IsComplete)
			items.Add(FormHelpers.MakeSecondaryButton("Reactivate Bag", ReactivateBag));

		items.Add(FormHelpers.MakeDangerButton("Delete Bag", DeleteBag));

		items.Add(FormHelpers.MakeSectionHeader("RATINGS"));
		items.Add(RatingDisplayFactory.Create(State.Rating));

		var stack = VStack(Theme.SpacingS);
		foreach (var item in items) stack.Add(item);

		return ScrollView(
			stack.Padding(new Thickness(Theme.SpacingM))
		)
		.Background(Theme.Background);
	}
}
