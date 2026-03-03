using Comet;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiBorder = Microsoft.Maui.Controls.Border;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;
using MauiSlider = Microsoft.Maui.Controls.Slider;
using MauiEditor = Microsoft.Maui.Controls.Editor;
using MauiButton = Microsoft.Maui.Controls.Button;
using MauiGrid = Microsoft.Maui.Controls.Grid;
using MauiBoxView = Microsoft.Maui.Controls.BoxView;
using MauiEllipse = Microsoft.Maui.Controls.Shapes.Ellipse;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;
using MauiFontAttributes = Microsoft.Maui.Controls.FontAttributes;

namespace CometBaristaNotes.Pages;

public class ShotLoggingPage : Comet.View
{
	[State] readonly State<int> _selectedBagIndex = new(-1);
	[State] readonly State<double> _doseIn = new(18.0);
	[State] readonly State<double> _doseOut = new(36.0);
	[State] readonly State<string> _grindSetting = new("15");
	[State] readonly State<string> _expectedTime = new("28");
	[State] readonly State<string> _expectedOutput = new("36");
	[State] readonly State<double> _actualTime = new(0);
	[State] readonly State<int> _rating = new(3);
	[State] readonly State<string> _tastingNotes = new("");
	[State] readonly State<int> _drinkTypeIndex = new(0);
	[State] readonly State<int> _machineIndex = new(0);
	[State] readonly State<int> _grinderIndex = new(0);
	[State] readonly State<int> _madeByIndex = new(0);
	[State] readonly State<int> _madeForIndex = new(0);
	[State] readonly State<bool> _saved = new(false);
	[State] readonly State<bool> _showAdditionalDetails = new(false);

	static readonly string[] DrinkTypes = { "Espresso", "Ristretto", "Lungo", "Doppio", "Americano" };

	double Ratio => _doseIn.Value > 0 ? Math.Round(_doseOut.Value / _doseIn.Value, 1) : 0;

	[Body]
	Comet.View body()
	{
		if (_saved.Value)
		{
			return BuildSuccessView();
		}

		var store = InMemoryDataStore.Instance;
		var bags = store?.GetAllBags().Where(b => !b.IsComplete).ToList() ?? new();
		var machines = store?.GetByType(EquipmentType.Machine) ?? new();
		var grinders = store?.GetByType(EquipmentType.Grinder) ?? new();
		var profiles = store?.GetAllProfiles() ?? new();

		var contentStack = new VerticalStackLayout { Spacing = Theme.SpacingM, Padding = new Thickness(Theme.SpacingM) };

		// Main dose gauges row with equipment button
		contentStack.Add(BuildDoseGaugesRow(machines));

		// Ratio display
		contentStack.Add(BuildRatioDisplay());

		// Time slider
		contentStack.Add(BuildTimeSlider());

		// Made By / Made For row
		contentStack.Add(BuildUserSelectionRow(profiles));

		// Star rating
		contentStack.Add(BuildStarRating());

		// Tasting notes
		contentStack.Add(BuildTastingNotes());

		// Additional Details
		contentStack.Add(BuildAdditionalDetails(bags, grinders));

		// Save button
		var saveBtn = FormHelpers.MakePrimaryButton("Save Shot", () => SaveShot(bags, machines, grinders, profiles));
		saveBtn.Margin = new Thickness(0, Theme.SpacingS, 0, Theme.SpacingXL);
		contentStack.Add(saveBtn);

		var scrollView = new MauiScrollView
		{
			Content = contentStack,
			BackgroundColor = Theme.Background,
		};

		return new MauiViewHost(scrollView);
	}

	Comet.View BuildSuccessView()
	{
		var stack = new VerticalStackLayout
		{
			Spacing = 16,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Padding = new Thickness(Theme.SpacingXL),
			BackgroundColor = Theme.Background,
		};

		stack.Add(new MauiLabel { Text = "✅", FontSize = 48, HorizontalTextAlignment = TextAlignment.Center });
		stack.Add(new MauiLabel { Text = "Shot Logged!", FontSize = 24, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary, HorizontalTextAlignment = TextAlignment.Center });
		stack.Add(new MauiLabel { Text = "Your espresso shot has been recorded.", FontSize = 14, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center });

		var btn = FormHelpers.MakePrimaryButton("Log Another", ResetForm);
		btn.Margin = new Thickness(0, 16, 0, 0);
		stack.Add(btn);

		return new MauiViewHost(stack);
	}

	Microsoft.Maui.Controls.View BuildDoseGaugesRow(List<Equipment> machines)
	{
		var grid = new MauiGrid
		{
			ColumnDefinitions =
			{
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star),
			},
		};

		grid.Add(BuildCircularGauge("Dose In", _doseIn.Value, "g", 10, 25, v => _doseIn.Value = v), 0, 0);
		grid.Add(BuildEquipmentButton(machines), 1, 0);
		grid.Add(BuildCircularGauge("Dose Out", _doseOut.Value, "g", 20, 60, v => _doseOut.Value = v), 2, 0);

		return grid;
	}

	Microsoft.Maui.Controls.View BuildCircularGauge(string label, double value, string unit, double min, double max, Action<double> onChange)
	{
		var stack = new VerticalStackLayout
		{
			Spacing = Theme.SpacingS,
			HorizontalOptions = LayoutOptions.Center,
		};

		stack.Add(new MauiLabel { Text = label, FontSize = 12, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center });

		// Circular display
		var circleGrid = new MauiGrid { WidthRequest = Theme.GaugeSize, HeightRequest = Theme.GaugeSize };

		var circleBorder = new MauiBorder
		{
			BackgroundColor = Theme.CardBackground,
			Stroke = new SolidColorBrush(Theme.CardStroke),
			StrokeThickness = 2,
			StrokeShape = new MauiEllipse(),
			WidthRequest = Theme.GaugeSize,
			HeightRequest = Theme.GaugeSize,
		};
		circleGrid.Add(circleBorder);

		var valueStack = new VerticalStackLayout
		{
			Spacing = 0,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		var valueLabel = new MauiLabel { Text = $"{value:F1}", FontSize = 28, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary, HorizontalTextAlignment = TextAlignment.Center };
		valueStack.Add(valueLabel);
		valueStack.Add(new MauiLabel { Text = unit, FontSize = 14, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center });
		circleGrid.Add(valueStack);

		stack.Add(circleGrid);

		// Stepper controls
		var stepperStack = new HorizontalStackLayout { Spacing = Theme.SpacingS, HorizontalOptions = LayoutOptions.Center };
		stepperStack.Add(BuildStepperButton("-", () =>
		{
			var newVal = Math.Max(min, value - 0.5);
			onChange(newVal);
		}));
		stepperStack.Add(BuildStepperButton("+", () =>
		{
			var newVal = Math.Min(max, value + 0.5);
			onChange(newVal);
		}));
		stack.Add(stepperStack);

		return stack;
	}

	Microsoft.Maui.Controls.View BuildStepperButton(string text, Action onTap)
	{
		var label = new MauiLabel
		{
			Text = text,
			FontSize = 20,
			FontAttributes = MauiFontAttributes.Bold,
			TextColor = Theme.Primary,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
		};

		var border = new MauiBorder
		{
			Content = label,
			BackgroundColor = Theme.SurfaceVariant,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = Theme.RadiusCard },
			WidthRequest = 40,
			HeightRequest = 40,
		};

		var tap = new TapGestureRecognizer();
		tap.Tapped += (s, e) => onTap();
		border.GestureRecognizers.Add(tap);

		return border;
	}

	Microsoft.Maui.Controls.View BuildEquipmentButton(List<Equipment> machines)
	{
		var selectedMachine = _machineIndex.Value > 0 && _machineIndex.Value <= machines.Count
			? machines[_machineIndex.Value - 1].Name
			: "Select";
		var displayName = selectedMachine.Length > 10 ? selectedMachine[..10] + "…" : selectedMachine;

		var stack = new VerticalStackLayout
		{
			Spacing = Theme.SpacingS,
			HorizontalOptions = LayoutOptions.Center,
		};

		stack.Add(new MauiBoxView { HeightRequest = 20, BackgroundColor = Colors.Transparent });

		var circleGrid = new MauiGrid { WidthRequest = Theme.EquipmentButtonSize, HeightRequest = Theme.EquipmentButtonSize };

		var circleBorder = new MauiBorder
		{
			BackgroundColor = Theme.Primary,
			StrokeThickness = 0,
			StrokeShape = new MauiEllipse(),
			WidthRequest = Theme.EquipmentButtonSize,
			HeightRequest = Theme.EquipmentButtonSize,
		};
		circleGrid.Add(circleBorder);
		circleGrid.Add(new MauiLabel { Text = "☕", FontSize = 24, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center });

		var tap = new TapGestureRecognizer();
		tap.Tapped += (s, e) =>
		{
			var newIdx = (_machineIndex.Value + 1) % (machines.Count + 1);
			_machineIndex.Value = newIdx;
		};
		circleGrid.GestureRecognizers.Add(tap);

		stack.Add(circleGrid);
		stack.Add(new MauiLabel { Text = displayName, FontSize = 11, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center, WidthRequest = 80 });

		return stack;
	}

	Microsoft.Maui.Controls.View BuildRatioDisplay()
	{
		var stack = new HorizontalStackLayout
		{
			Spacing = Theme.SpacingXS,
			HorizontalOptions = LayoutOptions.Center,
			Padding = new Thickness(Theme.SpacingS),
		};

		stack.Add(new MauiLabel { Text = "Ratio: ", FontSize = 16, TextColor = Theme.TextSecondary });
		stack.Add(new MauiLabel { Text = $"1:{Ratio:F1}", FontSize = 18, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary });

		return stack;
	}

	Microsoft.Maui.Controls.View BuildTimeSlider()
	{
		var contentStack = new VerticalStackLayout { Spacing = Theme.SpacingS };

		var headerGrid = new MauiGrid();
		headerGrid.Add(new MauiLabel { Text = "Time", FontSize = 14, TextColor = Theme.TextSecondary, HorizontalOptions = LayoutOptions.Start });
		var timeLabel = new MauiLabel { Text = $"{_actualTime.Value:F0}s", FontSize = 16, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary, HorizontalOptions = LayoutOptions.End };
		headerGrid.Add(timeLabel);
		contentStack.Add(headerGrid);

		var slider = new MauiSlider { Minimum = 0, Maximum = 60, Value = _actualTime.Value, MinimumTrackColor = Theme.Primary, MaximumTrackColor = Theme.SurfaceVariant };
		slider.ValueChanged += (s, e) =>
		{
			_actualTime.Value = e.NewValue;
			timeLabel.Text = $"{e.NewValue:F0}s";
		};
		contentStack.Add(slider);

		return FormHelpers.MakeCard(contentStack);
	}

	Microsoft.Maui.Controls.View BuildUserSelectionRow(List<UserProfile> profiles)
	{
		var profileNames = new[] { "None" }.Concat(profiles.Select(p => p.Name)).ToArray();

		var contentStack = new HorizontalStackLayout { Spacing = Theme.SpacingM };

		// Made By
		var madeByStack = new VerticalStackLayout { Spacing = Theme.SpacingXS };
		madeByStack.Add(new MauiLabel { Text = "Made By", FontSize = 12, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center });
		var madeByAvatar = BuildUserAvatar(_madeByIndex.Value, profiles);
		madeByStack.Add(madeByAvatar);

		var madeByTap = new TapGestureRecognizer();
		madeByTap.Tapped += (s, e) => _madeByIndex.Value = (_madeByIndex.Value + 1) % profileNames.Length;
		madeByStack.GestureRecognizers.Add(madeByTap);
		contentStack.Add(madeByStack);

		contentStack.Add(new MauiLabel { Text = "→", FontSize = 20, TextColor = Theme.TextMuted, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, Theme.SpacingM) });

		// Made For
		var madeForStack = new VerticalStackLayout { Spacing = Theme.SpacingXS };
		madeForStack.Add(new MauiLabel { Text = "Made For", FontSize = 12, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center });
		var madeForAvatar = BuildUserAvatar(_madeForIndex.Value, profiles);
		madeForStack.Add(madeForAvatar);

		var madeForTap = new TapGestureRecognizer();
		madeForTap.Tapped += (s, e) => _madeForIndex.Value = (_madeForIndex.Value + 1) % profileNames.Length;
		madeForStack.GestureRecognizers.Add(madeForTap);
		contentStack.Add(madeForStack);

		return FormHelpers.MakeCard(contentStack);
	}

	Microsoft.Maui.Controls.View BuildUserAvatar(int selectedIndex, List<UserProfile> profiles)
	{
		var name = selectedIndex == 0 ? "?" :
			(selectedIndex - 1 < profiles.Count ? profiles[selectedIndex - 1].Name[..1].ToUpper() : "?");
		var fullName = selectedIndex == 0 ? "None" :
			(selectedIndex - 1 < profiles.Count ? profiles[selectedIndex - 1].Name : "None");

		var stack = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center };

		var circleGrid = new MauiGrid { WidthRequest = 44, HeightRequest = 44 };
		var circleBorder = new MauiBorder
		{
			BackgroundColor = selectedIndex == 0 ? Theme.SurfaceVariant : Theme.Primary,
			StrokeThickness = 0,
			StrokeShape = new MauiEllipse(),
			WidthRequest = 44,
			HeightRequest = 44,
		};
		circleGrid.Add(circleBorder);
		circleGrid.Add(new MauiLabel
		{
			Text = name,
			FontSize = 18,
			FontAttributes = MauiFontAttributes.Bold,
			TextColor = selectedIndex == 0 ? Theme.TextMuted : Colors.White,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
		});
		stack.Add(circleGrid);
		stack.Add(new MauiLabel { Text = fullName, FontSize = 11, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center });

		return stack;
	}

	Microsoft.Maui.Controls.View BuildStarRating()
	{
		var contentStack = new VerticalStackLayout { Spacing = Theme.SpacingS };
		contentStack.Add(new MauiLabel { Text = "Rating", FontSize = 14, TextColor = Theme.TextSecondary });

		var starsStack = new HorizontalStackLayout { Spacing = Theme.SpacingS, HorizontalOptions = LayoutOptions.Center };
		for (int i = 1; i <= 5; i++)
		{
			var starIndex = i;
			var star = new MauiLabel
			{
				Text = i <= _rating.Value ? "★" : "☆",
				FontSize = 32,
				TextColor = i <= _rating.Value ? Theme.StarFilled : Theme.StarEmpty,
			};

			var tap = new TapGestureRecognizer();
			tap.Tapped += (s, e) => _rating.Value = starIndex;
			star.GestureRecognizers.Add(tap);
			starsStack.Add(star);
		}
		contentStack.Add(starsStack);

		return FormHelpers.MakeCard(contentStack);
	}

	Microsoft.Maui.Controls.View BuildTastingNotes()
	{
		var contentStack = new VerticalStackLayout { Spacing = Theme.SpacingS };
		contentStack.Add(new MauiLabel { Text = "Tasting Notes", FontSize = 14, TextColor = Theme.TextSecondary });

		var editor = new MauiEditor
		{
			Text = _tastingNotes.Value,
			FontSize = 16,
			TextColor = Theme.TextPrimary,
			BackgroundColor = Theme.SurfaceVariant,
			HeightRequest = 80,
		};
		editor.TextChanged += (s, e) => _tastingNotes.Value = e.NewTextValue ?? "";

		var border = new MauiBorder
		{
			Content = editor,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = Theme.RadiusEditor },
			BackgroundColor = Theme.SurfaceVariant,
		};
		contentStack.Add(border);

		return FormHelpers.MakeCard(contentStack);
	}

	Microsoft.Maui.Controls.View BuildAdditionalDetails(List<Bag> bags, List<Equipment> grinders)
	{
		var bagNames = bags.Select(b => b.BeanName ?? $"Bag #{b.Id}").ToArray();
		var grinderNames = new[] { "None" }.Concat(grinders.Select(g => g.Name)).ToArray();

		var headerGrid = new MauiGrid();
		headerGrid.Add(new MauiLabel { Text = "Additional Details", FontSize = 16, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary, HorizontalOptions = LayoutOptions.Start });
		headerGrid.Add(new MauiLabel { Text = _showAdditionalDetails.Value ? "▼" : "▶", FontSize = 14, TextColor = Theme.TextMuted, HorizontalOptions = LayoutOptions.End });

		var headerCard = FormHelpers.MakeCard(headerGrid);
		var tap = new TapGestureRecognizer();
		tap.Tapped += (s, e) => _showAdditionalDetails.Value = !_showAdditionalDetails.Value;
		headerCard.GestureRecognizers.Add(tap);

		if (!_showAdditionalDetails.Value)
			return headerCard;

		var stack = new VerticalStackLayout { Spacing = Theme.SpacingS };
		stack.Add(headerCard);
		stack.Add(FormHelpers.MakeFormPicker("Coffee Bag", _selectedBagIndex.Value, bagNames, v => _selectedBagIndex.Value = v));
		stack.Add(FormHelpers.MakeFormPicker("Drink Type", _drinkTypeIndex.Value, DrinkTypes, v => _drinkTypeIndex.Value = v));
		stack.Add(FormHelpers.MakeFormPicker("Grinder", _grinderIndex.Value, grinderNames, v => _grinderIndex.Value = v));
		stack.Add(FormHelpers.MakeFormEntry("Grind Setting", _grindSetting.Value ?? "", "e.g. 15", v => _grindSetting.Value = v));
		stack.Add(FormHelpers.MakeFormEntry("Expected Time (s)", _expectedTime.Value ?? "", "28", v => _expectedTime.Value = v));
		stack.Add(FormHelpers.MakeFormEntry("Expected Output (g)", _expectedOutput.Value ?? "", "36", v => _expectedOutput.Value = v));

		return stack;
	}

	void SaveShot(List<Bag> bags, List<Equipment> machines, List<Equipment> grinders, List<UserProfile> profiles)
	{
		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		var bagIdx = _selectedBagIndex.Value;
		if (bagIdx < 0 && bags.Count > 0) bagIdx = 0;
		if (bagIdx < 0 || bagIdx >= bags.Count) return;

		var machineIdx = _machineIndex.Value - 1;
		var grinderIdx = _grinderIndex.Value - 1;
		var madeByIdx = _madeByIndex.Value - 1;
		var madeForIdx = _madeForIndex.Value - 1;

		var shot = new ShotRecord
		{
			BagId = bags[bagIdx].Id,
			DoseIn = (decimal)_doseIn.Value,
			GrindSetting = _grindSetting.Value ?? "15",
			ExpectedTime = decimal.TryParse(_expectedTime.Value, out var et) ? et : 28m,
			ExpectedOutput = decimal.TryParse(_expectedOutput.Value, out var eo) ? eo : 36m,
			ActualTime = (decimal)_actualTime.Value,
			ActualOutput = (decimal)_doseOut.Value,
			Rating = _rating.Value,
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
		_doseIn.Value = 18.0;
		_doseOut.Value = 36.0;
		_actualTime.Value = 0;
		_rating.Value = 3;
		_tastingNotes.Value = "";
		_saved.Value = false;
		_showAdditionalDetails.Value = false;
	}
}
