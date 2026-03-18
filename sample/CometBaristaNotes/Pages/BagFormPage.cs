using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

namespace CometBaristaNotes.Pages;

public class BagFormPageState
{
	public string RoastDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
	public string Notes { get; set; } = "";
	public string Error { get; set; } = "";
	public string BeanName { get; set; } = "";
	public bool IsLoaded { get; set; }
}

public class BagFormPage : Component<BagFormPageState>
{
	readonly int _beanId;

	public BagFormPage(int beanId = 0) { _beanId = beanId; }

	void LoadBeanName()
	{
		var store = InMemoryDataStore.Instance;
		if (store != null)
		{
			var bean = store.GetBean(_beanId);
			SetState(s => s.BeanName = bean?.Name ?? "Unknown Bean");
		}
		SetState(s => s.IsLoaded = true);
	}

	void Save()
	{
		if (!DateTime.TryParse(State.RoastDate, out var roastDate))
		{
			SetState(s => s.Error = "Please enter a valid date (yyyy-MM-dd)");
			return;
		}

		if (roastDate.Date > DateTime.Now.Date)
		{
			SetState(s => s.Error = "Roast date cannot be in the future");
			return;
		}

		SetState(s => s.Error = "");

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		store.CreateBag(new Bag
		{
			BeanId = _beanId,
			RoastDate = roastDate,
			Notes = string.IsNullOrWhiteSpace(State.Notes) ? null : State.Notes,
		});

		Navigation?.Pop();
	}

	public override View Render()
	{
		if (!State.IsLoaded)
			LoadBeanName();

		var errorView = !string.IsNullOrEmpty(State.Error)
			? Text(State.Error).Color(Theme.Error).FontFamily(Theme.FontRegular).FontSize(14)
			: null;

		return ScrollView(
			VStack(Theme.SpacingS,
				FormHelpers.MakeSectionHeader("ADD BAG"),
				FormHelpers.MakeReadOnlyField("Bean", State.BeanName),
				FormHelpers.MakeFormEntry("Roast Date", State.RoastDate, "yyyy-MM-dd", v => SetState(s => s.RoastDate = v)),
				FormHelpers.MakeFormEntryWithLimit("Notes (optional)", State.Notes, "e.g., From Trader Joe's, Gift from friend", 500, v => SetState(s => s.Notes = v)),
				errorView,
				FormHelpers.MakePrimaryButton("Add Bag", Save)
			)
			.Padding(new Thickness(Theme.SpacingM))
		)
		.Background(Theme.Background);
	}
}
