using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

namespace CometBaristaNotes.Pages;

public class ShotLoggingPage : Comet.View
{
	[State] readonly State<int> _selectedBagIndex = new(-1);
	[State] readonly State<string> _doseIn = new("18");
	[State] readonly State<string> _grindSetting = new("15");
	[State] readonly State<string> _expectedTime = new("28");
	[State] readonly State<string> _expectedOutput = new("36");
	[State] readonly State<string> _actualTime = new("");
	[State] readonly State<string> _actualOutput = new("");
	[State] readonly State<double> _rating = new(3);
	[State] readonly State<string> _tastingNotes = new("");
	[State] readonly State<int> _drinkTypeIndex = new(0);
	[State] readonly State<int> _machineIndex = new(0);
	[State] readonly State<int> _grinderIndex = new(0);
	[State] readonly State<int> _madeByIndex = new(0);
	[State] readonly State<int> _madeForIndex = new(0);
	[State] readonly State<bool> _saved = new(false);

	static readonly string[] DrinkTypes = { "Espresso", "Ristretto", "Lungo", "Doppio", "Americano" };

	[Body]
	Comet.View body()
	{
		if (_saved.Value)
		{
			return new VStack(spacing: 16)
			{
				new Text("✅").FontSize(48),
				new Text("Shot Logged!").FontSize(24).FontWeight(FontWeight.Bold).Color(Theme.TextPrimary),
				new Text("Your espresso shot has been recorded.").FontSize(14).Color(Theme.TextSecondary),
				FormHelpers.PrimaryButton("Log Another", ResetForm)
					.Padding(new Thickness(0, 16, 0, 0))
			}.Alignment(Alignment.Center).Padding(Theme.SpacingXL).Background(Theme.Background);
		}

		var store = InMemoryDataStore.Instance;
		var bags = store?.GetAllBags().Where(b => !b.IsComplete).ToList() ?? new();
		var machines = store?.GetByType(EquipmentType.Machine) ?? new();
		var grinders = store?.GetByType(EquipmentType.Grinder) ?? new();
		var profiles = store?.GetAllProfiles() ?? new();

		var bagNames = bags.Select(b => b.BeanName ?? $"Bag #{b.Id}").ToArray();
		var machineNames = new[] { "None" }.Concat(machines.Select(m => m.Name)).ToArray();
		var grinderNames = new[] { "None" }.Concat(grinders.Select(g => g.Name)).ToArray();
		var profileNames = new[] { "None" }.Concat(profiles.Select(p => p.Name)).ToArray();

		return new Comet.ScrollView
		{
			new VStack(spacing: Theme.SpacingS)
			{
				FormHelpers.SectionHeader("COFFEE"),
				FormHelpers.FormPicker("Bag", _selectedBagIndex, bagNames),
				FormHelpers.FormPicker("Drink Type", _drinkTypeIndex, DrinkTypes),

				FormHelpers.SectionHeader("RECIPE"),
				new HStack(spacing: Theme.SpacingS)
				{
					FormHelpers.FormNumericEntry("Dose (g)", _doseIn),
					FormHelpers.FormNumericEntry("Grind", _grindSetting),
				},
				new HStack(spacing: Theme.SpacingS)
				{
					FormHelpers.FormNumericEntry("Target Time (s)", _expectedTime),
					FormHelpers.FormNumericEntry("Target Output (g)", _expectedOutput),
				},

				FormHelpers.SectionHeader("EQUIPMENT"),
				FormHelpers.FormPicker("Machine", _machineIndex, machineNames),
				FormHelpers.FormPicker("Grinder", _grinderIndex, grinderNames),

				FormHelpers.SectionHeader("RESULTS"),
				new HStack(spacing: Theme.SpacingS)
				{
					FormHelpers.FormNumericEntry("Actual Time (s)", _actualTime),
					FormHelpers.FormNumericEntry("Actual Output (g)", _actualOutput),
				},

				FormHelpers.SectionHeader("RATING"),
				FormHelpers.FormSlider("Rating", _rating, 1, 5),
				new Text(() => RatingStars((int)Math.Round(_rating.Value)))
					.FontSize(24).Color(Theme.Warning),

				FormHelpers.SectionHeader("NOTES"),
				new TextEditor(_tastingNotes)
					.Frame(height: 100)
					.Background(Theme.SurfaceVariant)
					.ClipShape(new RoundedRectangle(Theme.RadiusEditor)),

				FormHelpers.SectionHeader("PEOPLE"),
				new HStack(spacing: Theme.SpacingS)
				{
					FormHelpers.FormPicker("Made By", _madeByIndex, profileNames),
					FormHelpers.FormPicker("Made For", _madeForIndex, profileNames),
				},

				FormHelpers.PrimaryButton("Log Shot", () => SaveShot(bags, machines, grinders, profiles))
					.Padding(new Thickness(0, Theme.SpacingS, 0, Theme.SpacingXL)),
			}.Padding(Theme.SpacingM)
		}.Background(Theme.Background);
	}

	void SaveShot(List<Bag> bags, List<Equipment> machines, List<Equipment> grinders, List<UserProfile> profiles)
	{
		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		var bagIdx = _selectedBagIndex.Value;
		if (bagIdx < 0 || bagIdx >= bags.Count) return;

		var machineIdx = _machineIndex.Value - 1;
		var grinderIdx = _grinderIndex.Value - 1;
		var madeByIdx = _madeByIndex.Value - 1;
		var madeForIdx = _madeForIndex.Value - 1;

		var shot = new ShotRecord
		{
			BagId = bags[bagIdx].Id,
			DoseIn = decimal.TryParse(_doseIn.Value, out var d) ? d : 18m,
			GrindSetting = _grindSetting.Value ?? "15",
			ExpectedTime = decimal.TryParse(_expectedTime.Value, out var et) ? et : 28m,
			ExpectedOutput = decimal.TryParse(_expectedOutput.Value, out var eo) ? eo : 36m,
			ActualTime = decimal.TryParse(_actualTime.Value, out var at) ? at : null,
			ActualOutput = decimal.TryParse(_actualOutput.Value, out var ao) ? ao : null,
			Rating = (int)Math.Round(_rating.Value),
			TastingNotes = string.IsNullOrWhiteSpace(_tastingNotes.Value) ? null : _tastingNotes.Value,
			DrinkType = _drinkTypeIndex.Value >= 0 && _drinkTypeIndex.Value < DrinkTypes.Length
				? DrinkTypes[_drinkTypeIndex.Value] : "Espresso",
			MachineId = machineIdx >= 0 && machineIdx < machines.Count ? machines[machineIdx].Id : null,
			GrinderId = grinderIdx >= 0 && grinderIdx < grinders.Count ? grinders[grinderIdx].Id : null,
			MadeById = madeByIdx >= 0 && madeByIdx < profiles.Count ? profiles[madeByIdx].Id : null,
			MadeForId = madeForIdx >= 0 && madeForIdx < profiles.Count ? profiles[madeForIdx].Id : null,
		};

		store.CreateShot(shot);
		_saved.Value = true;
	}

	void ResetForm()
	{
		_actualTime.Value = "";
		_actualOutput.Value = "";
		_rating.Value = 3;
		_tastingNotes.Value = "";
		_saved.Value = false;
	}

	static string RatingStars(int rating)
	{
		rating = Math.Clamp(rating, 0, 5);
		return new string('★', rating) + new string('☆', 5 - rating);
	}
}
