using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Extensions.DependencyInjection;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;
using Button = Comet.Button;
using ScrollView = Comet.ScrollView;
using Picker = Comet.Picker;

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

	IEquipmentService? GetEquipmentService() =>
		ViewHandler?.MauiContext?.Services.GetService<IEquipmentService>();

	void LoadEquipment()
	{
		if (_equipmentId <= 0) { _isLoaded.Value = true; return; }

		var svc = GetEquipmentService();
		if (svc == null) return;

		var eq = svc.GetEquipment(_equipmentId);
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

		var svc = GetEquipmentService();
		if (svc == null) return;

		var typeIdx = _selectedTypeIndex.Value;
		var eqType = (typeIdx >= 0 && typeIdx < TypeValues.Length) ? TypeValues[typeIdx] : EquipmentType.Machine;

		if (_equipmentId > 0)
		{
			svc.UpdateEquipment(new Equipment
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
			svc.CreateEquipment(new Equipment
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
		var svc = GetEquipmentService();
		if (svc == null) return;

		svc.ArchiveEquipment(_equipmentId);
		Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadEquipment();

		var isEdit = _equipmentId > 0;

		return new ScrollView
		{
			new VStack(spacing: 12)
			{
				FormHelpers.SectionHeader(isEdit ? "Edit Equipment" : "New Equipment"),

				FormHelpers.FormEntry("Name *", _name, "Equipment name"),

				// Type picker
				new VStack(spacing: 4)
				{
					new Text("Type").FontSize(12).Color(Colors.Gray),
					new Picker(_selectedTypeIndex, TypeNames)
				}.Padding(12).Background(Color.FromArgb("#F5F5F5")).ClipShape(new RoundedRectangle(8)),

				FormHelpers.FormEntry("Notes", _notes, "Additional details"),

				!string.IsNullOrEmpty(_error.Value)
					? new Text(_error.Value).Color(Colors.Red).FontSize(13)
					: null,

				new Button(isEdit ? "Save Changes" : "Add Equipment", Save),

				isEdit ? new Button("Archive", Archive) : null,
			}.Padding(16)
		};
	}
}
