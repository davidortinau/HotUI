using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

namespace CometBaristaNotes.Pages;

public class EquipmentManagementPageState
{
	public List<Equipment> Equipment { get; set; } = new();
	public bool IsLoaded { get; set; }
}

public class EquipmentManagementPage : Component<EquipmentManagementPageState>
{
	void LoadEquipment()
	{
		var store = InMemoryDataStore.Instance;
		if (store == null) return;
		SetState(s =>
		{
			s.Equipment = store.GetAllEquipment();
			s.IsLoaded = true;
		});
	}

	public override View Render()
	{
		if (!State.IsLoaded)
			LoadEquipment();

		var items = State.Equipment;

		if (items.Count == 0)
		{
			return VStack(Theme.SpacingM,
				FormHelpers.MakeEmptyState(Icons.Build, "No Equipment Yet", "Add your coffee machines, grinders, and accessories"),
				FormHelpers.MakePrimaryButton("+ Add Equipment", () => Navigation?.Navigate(new EquipmentDetailPage(0)))
			)
			.Padding(new Thickness(Theme.SpacingL))
			.Background(Theme.Background);
		}

		var stack = VStack(Theme.SpacingS,
			FormHelpers.MakePrimaryButton("+ Add Equipment", () => Navigation?.Navigate(new EquipmentDetailPage(0)))
		);

		foreach (var eq in items)
		{
			stack.Add(FormHelpers.MakeListCard(
				eq.Name,
				eq.Type.ToString(),
				eq.Notes,
				() => Navigation?.Navigate(new EquipmentDetailPage(eq.Id))
			));
		}

		return ScrollView(stack.Padding(new Thickness(Theme.SpacingM)))
			.Background(Theme.Background);
	}
}
