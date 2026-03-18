using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

namespace CometBaristaNotes.Pages;

public class EquipmentDetailPageState
{
	public string Name { get; set; } = "";
	public int SelectedTypeIndex { get; set; }
	public string Notes { get; set; } = "";
	public bool IsLoaded { get; set; }
	public string Error { get; set; } = "";
}

public class EquipmentDetailPage : Component<EquipmentDetailPageState>
{
	static readonly string[] TypeNames = { "Machine", "Grinder", "Tamper", "PuckScreen", "Other" };
	static readonly EquipmentType[] TypeValues =
		{ EquipmentType.Machine, EquipmentType.Grinder, EquipmentType.Tamper, EquipmentType.PuckScreen, EquipmentType.Other };

	readonly int _equipmentId;

	public EquipmentDetailPage(int equipmentId = 0) { _equipmentId = equipmentId; }

	void LoadEquipment()
	{
		if (_equipmentId <= 0) { SetState(s => s.IsLoaded = true); return; }

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		var eq = store.GetEquipment(_equipmentId);
		if (eq == null)
		{
			SetState(s =>
			{
				s.Error = "Equipment not found";
				s.IsLoaded = true;
			});
			return;
		}

		var typeIndex = Array.IndexOf(TypeValues, eq.Type);
		if (typeIndex < 0) typeIndex = 0;

		SetState(s =>
		{
			s.Name = eq.Name;
			s.SelectedTypeIndex = typeIndex;
			s.Notes = eq.Notes ?? "";
			s.IsLoaded = true;
		});
	}

	void Save()
	{
		if (string.IsNullOrWhiteSpace(State.Name))
		{
			SetState(s => s.Error = "Equipment name is required");
			return;
		}
		SetState(s => s.Error = "");

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		var typeIdx = State.SelectedTypeIndex;
		var eqType = (typeIdx >= 0 && typeIdx < TypeValues.Length) ? TypeValues[typeIdx] : EquipmentType.Machine;

		if (_equipmentId > 0)
		{
			store.UpdateEquipment(new Equipment
			{
				Id = _equipmentId,
				Name = State.Name,
				Type = eqType,
				Notes = string.IsNullOrWhiteSpace(State.Notes) ? null : State.Notes,
				IsActive = true
			});
		}
		else
		{
			store.CreateEquipment(new Equipment
			{
				Name = State.Name,
				Type = eqType,
				Notes = string.IsNullOrWhiteSpace(State.Notes) ? null : State.Notes,
			});
		}

		Navigation?.Pop();
	}

	async void Archive()
	{
		if (_equipmentId <= 0) return;
		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		var page = Services.PageHelper.GetCurrentPage();
		if (page == null) return;

		var confirm = await page.DisplayAlertAsync(
			"Archive Equipment?",
			$"Are you sure you want to archive '{State.Name}'? This action cannot be undone.",
			"Archive", "Cancel");
		if (!confirm) return;

		store.ArchiveEquipment(_equipmentId);
		Navigation?.Pop();
	}

	public override View Render()
	{
		if (!State.IsLoaded)
			LoadEquipment();

		var isEdit = _equipmentId > 0;

		var items = new List<View>
		{
			FormHelpers.MakeSectionHeader(isEdit ? "EDIT EQUIPMENT" : "NEW EQUIPMENT"),
			FormHelpers.MakeFormEntry("Name *", State.Name, "Equipment name", v => SetState(s => s.Name = v)),
			FormHelpers.MakeFormPicker("Type", State.SelectedTypeIndex, TypeNames, v => SetState(s => s.SelectedTypeIndex = v)),
			FormHelpers.MakeFormEntry("Notes", State.Notes, "Additional details", v => SetState(s => s.Notes = v)),
		};

		if (!string.IsNullOrEmpty(State.Error))
			items.Add(Text(State.Error).Color(Theme.Error).FontFamily(Theme.FontRegular).FontSize(14));

		items.Add(FormHelpers.MakePrimaryButton(isEdit ? "Save Changes" : "Add Equipment", Save));

		if (isEdit)
			items.Add(FormHelpers.MakeDangerButton("Archive Equipment", Archive));

		var stack = VStack(Theme.SpacingS);
		foreach (var item in items)
			stack.Add(item);

		return ScrollView(
			stack.Padding(new Thickness(Theme.SpacingM))
		)
		.Background(Theme.Background);
	}
}
