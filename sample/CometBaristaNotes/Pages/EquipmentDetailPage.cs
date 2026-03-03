using Comet;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;

namespace CometBaristaNotes.Pages;

public class EquipmentDetailPage : Comet.View
{
	static readonly string[] TypeNames = { "Machine", "Grinder", "Tamper", "PuckScreen", "Other" };
	static readonly EquipmentType[] TypeValues =
		{ EquipmentType.Machine, EquipmentType.Grinder, EquipmentType.Tamper, EquipmentType.PuckScreen, EquipmentType.Other };

	readonly int _equipmentId;

	[State] readonly State<string> _name = new("");
	[State] readonly State<int> _selectedTypeIndex = new(0);
	[State] readonly State<string> _notes = new("");
	[State] readonly State<bool> _isLoaded = new(false);
	[State] readonly State<string> _error = new("");

	public EquipmentDetailPage(int equipmentId = 0) { _equipmentId = equipmentId; }

	void LoadEquipment()
	{
		if (_equipmentId <= 0) { _isLoaded.Value = true; return; }

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		var eq = store.GetEquipment(_equipmentId);
		if (eq == null) { _error.Value = "Equipment not found"; _isLoaded.Value = true; return; }

		_name.Value = eq.Name;
		_selectedTypeIndex.Value = Array.IndexOf(TypeValues, eq.Type);
		if (_selectedTypeIndex.Value < 0) _selectedTypeIndex.Value = 0;
		_notes.Value = eq.Notes ?? "";

		_isLoaded.Value = true;
	}

	void Save()
	{
		if (string.IsNullOrWhiteSpace(_name.Value))
		{
			_error.Value = "Equipment name is required";
			return;
		}
		_error.Value = "";

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		var typeIdx = _selectedTypeIndex.Value;
		var eqType = (typeIdx >= 0 && typeIdx < TypeValues.Length) ? TypeValues[typeIdx] : EquipmentType.Machine;

		if (_equipmentId > 0)
		{
			store.UpdateEquipment(new Equipment
			{
				Id = _equipmentId,
				Name = _name.Value,
				Type = eqType,
				Notes = string.IsNullOrWhiteSpace(_notes.Value) ? null : _notes.Value,
				IsActive = true
			});
		}
		else
		{
			store.CreateEquipment(new Equipment
			{
				Name = _name.Value,
				Type = eqType,
				Notes = string.IsNullOrWhiteSpace(_notes.Value) ? null : _notes.Value,
			});
		}

		Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
	}

	void Archive()
	{
		if (_equipmentId <= 0) return;
		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		store.ArchiveEquipment(_equipmentId);
		Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadEquipment();

		var isEdit = _equipmentId > 0;

		var stack = new VerticalStackLayout { Spacing = Theme.SpacingS, Padding = new Thickness(Theme.SpacingM) };

		stack.Add(FormHelpers.MakeSectionHeader(isEdit ? "EDIT EQUIPMENT" : "NEW EQUIPMENT"));
		stack.Add(FormHelpers.MakeFormEntry("Name *", _name.Value, "Equipment name", v => _name.Value = v));
		stack.Add(FormHelpers.MakeFormPicker("Type", _selectedTypeIndex.Value, TypeNames, v => _selectedTypeIndex.Value = v));
		stack.Add(FormHelpers.MakeFormEntry("Notes", _notes.Value, "Additional details", v => _notes.Value = v));

		if (!string.IsNullOrEmpty(_error.Value))
			stack.Add(new MauiLabel { Text = _error.Value, TextColor = Theme.Error, FontFamily = Theme.FontRegular, FontSize = 14 });

		stack.Add(FormHelpers.MakePrimaryButton(isEdit ? "Save Changes" : "Add Equipment", Save));

		if (isEdit)
			stack.Add(FormHelpers.MakeDangerButton("Archive Equipment", Archive));

		var scrollView = new MauiScrollView
		{
			Content = stack,
			BackgroundColor = Theme.Background,
		};

		return new MauiViewHost(scrollView);
	}
}
