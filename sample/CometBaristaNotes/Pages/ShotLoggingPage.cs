using Comet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;
using Syncfusion.Maui.Gauges;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiBorder = Microsoft.Maui.Controls.Border;
using MauiSlider = Microsoft.Maui.Controls.Slider;
using MauiButton = Microsoft.Maui.Controls.Button;
using MauiGrid = Microsoft.Maui.Controls.Grid;
using MauiBoxView = Microsoft.Maui.Controls.BoxView;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;
using MauiFontAttributes = Microsoft.Maui.Controls.FontAttributes;
using MauiPicker = Microsoft.Maui.Controls.Picker;

namespace CometBaristaNotes.Pages;

/// <summary>
/// Shot logging page using imperative updates for smooth interaction.
/// Native MAUI controls are built once and updated directly in event handlers,
/// avoiding full UI rebuild on each state change.
/// </summary>
public class ShotLoggingPageState { }

public class ShotLoggingPage : Component<ShotLoggingPageState>
{
// Edit mode: if > 0, we're editing an existing shot
int _editingShotId = 0;
readonly IFeedbackService _feedbackService;

// Current values (not Comet State<T> — we update controls directly)
double _doseIn = 18.0;
double _doseOut = 36.0;
double _actualTime = 0;
int _rating = 2;
string _tastingNotes = "";
string _grindSetting = "15";
string _expectedTime = "28";
string _expectedOutput = "36";
int _drinkTypeIndex = 0;
int _machineIndex = 0;
int _grinderIndex = 0;
int _madeByIndex = 0;
int _madeForIndex = 0;
int _selectedBagIndex = -1;

bool IsEditMode => _editingShotId > 0;

public ShotLoggingPage()
{
	_feedbackService = IPlatformApplication.Current!.Services.GetRequiredService<IFeedbackService>();
}

public ShotLoggingPage(int shotId)
{
	_feedbackService = IPlatformApplication.Current!.Services.GetRequiredService<IFeedbackService>();
	_editingShotId = shotId;
}

// References to mutable controls
MauiLabel? _doseInValueLabel, _doseInUnitLabel;
MauiLabel? _doseOutValueLabel, _doseOutUnitLabel;
RangePointer? _doseInRange, _doseOutRange;
ShapePointer? _doseInPointer, _doseOutPointer;
MauiLabel? _ratioLabel;
MauiLabel? _timeValueLabel;
MauiLabel? _machineNameLabel;
VerticalStackLayout? _additionalStack;
Microsoft.Maui.Controls.ActivityIndicator? _savingIndicator;
View? _saveButton;

// Data
List<Bag> _bags = new();
List<Equipment> _machines = new();
List<Equipment> _grinders = new();
List<UserProfile> _profiles = new();

static readonly string[] DrinkTypes = { "Espresso", "Americano", "Latte", "Cappuccino", "Flat White", "Cortado" };

double Ratio => _doseIn > 0 ? Math.Round(_doseOut / _doseIn, 1) : 0;

public override View Render()
{
var store = InMemoryDataStore.Instance;
_bags = store?.GetAllBags().Where(b => !b.IsComplete).ToList() ?? new();
_machines = store?.GetByType(EquipmentType.Machine) ?? new();
_grinders = store?.GetByType(EquipmentType.Grinder) ?? new();
_profiles = store?.GetAllProfiles() ?? new();

// Load existing shot data in edit mode
if (IsEditMode)
	LoadExistingShot(store);

_savingIndicator = new Microsoft.Maui.Controls.ActivityIndicator
{
	Color = Theme.Primary,
	IsRunning = false,
	IsVisible = false,
	HeightRequest = 32,
};

var items = new List<View>();

// Saving indicator — MAUI ActivityIndicator, wrapped
items.Add(new MauiViewHost(_savingIndicator));

// Syncfusion gauges & imperative MAUI sections — wrapped
items.Add(new MauiViewHost(BuildDoseGaugesRow()));
items.Add(new MauiViewHost(BuildRatioDisplay()));
items.Add(new MauiViewHost(BuildTimeSlider()));
items.Add(new MauiViewHost(BuildUserSelectionRow()));
items.Add(new MauiViewHost(BuildRating()));

// Tasting notes — pure Comet
items.Add(BuildTastingNotes());

// Save button — already Comet
var saveBtn = FormHelpers.MakePrimaryButton(IsEditMode ? "Update Shot" : "Add Shot", SaveShot)
	.Margin(new Thickness(0, Theme.SpacingS, 0, 0));
_saveButton = saveBtn;
items.Add(saveBtn);

// Additional details — MAUI, wrapped
items.Add(new MauiViewHost(BuildAdditionalDetails()));

// Delete button — pure Comet
if (IsEditMode)
{
	items.Add(Button("Delete Shot", async () => await DeleteShot())
		.FontFamily(Theme.FontSemibold).FontSize(16)
		.Color(Theme.Error).Background(Colors.Transparent)
		.CornerRadius((int)Theme.RadiusPill)
		.Frame(height: Theme.ButtonHeight)
		.Margin(new Thickness(0, Theme.SpacingS, 0, Theme.SpacingXL)));
}
else
{
	saveBtn.Margin(new Thickness(0, Theme.SpacingS, 0, Theme.SpacingXL));
}

var stack = VStack(Theme.SpacingM);
foreach (var item in items) stack.Add(item);
stack.Padding(new Thickness(Theme.SpacingM));

return ScrollView(stack).Background(Theme.Background);
}

Microsoft.Maui.Controls.View BuildDoseGaugesRow()
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

grid.Add(BuildGauge("Dose In", _doseIn, "g", 0, 25,
ref _doseInValueLabel, ref _doseInUnitLabel, ref _doseInRange, ref _doseInPointer,
delta => { _doseIn = Math.Round(Math.Clamp(_doseIn + delta, 10, 25), 1); UpdateDoseIn(); }), 0, 0);

grid.Add(BuildEquipmentButton(), 1, 0);

grid.Add(BuildGauge("Dose Out", _doseOut, "g", 0, 60,
ref _doseOutValueLabel, ref _doseOutUnitLabel, ref _doseOutRange, ref _doseOutPointer,
delta => { _doseOut = Math.Round(Math.Clamp(_doseOut + delta, 20, 60), 1); UpdateDoseOut(); }), 2, 0);

return grid;
}

Microsoft.Maui.Controls.View BuildGauge(string label, double value, string unit,
double min, double max,
ref MauiLabel? valueLabel, ref MauiLabel? unitLabel,
ref RangePointer? range, ref ShapePointer? pointer,
Action<double> onStep)
{
var gaugeGrid = new MauiGrid { WidthRequest = 160, HeightRequest = 160, HorizontalOptions = LayoutOptions.Center };

var gauge = new SfRadialGauge { WidthRequest = 160, HeightRequest = 160, BackgroundColor = Colors.Transparent };
var axis = new RadialAxis
{
Minimum = min, Maximum = max,
Interval = (max - min) / 5,
MinorTicksPerInterval = 1,
ShowLabels = true, ShowTicks = false,
RadiusFactor = 0.8,
LabelFormat = "0",
AxisLabelStyle = new GaugeLabelStyle { TextColor = Theme.TextSecondary, FontSize = 10 },
AxisLineStyle = new RadialLineStyle
{
Fill = new SolidColorBrush(Theme.SurfaceVariant),
Thickness = 20,
CornerStyle = CornerStyle.BothCurve
}
};

var r = new RangePointer
{
Value = value,
CornerStyle = CornerStyle.BothCurve,
PointerWidth = 20,
Fill = new SolidColorBrush(Theme.Primary)
};
range = r;
axis.Pointers.Add(r);

var p = new ShapePointer
{
Value = value,
IsInteractive = true,
StepFrequency = 0.1,
ShapeType = ShapeType.Circle,
ShapeHeight = 28, ShapeWidth = 28,
Fill = new SolidColorBrush(Theme.Primary),
HasShadow = true, Offset = 0,
};
p.ValueChanged += (s, e) =>
{
var rounded = Math.Round(e.Value, 1);
onStep(rounded - value);
value = rounded;
};
pointer = p;
axis.Pointers.Add(p);

gauge.Axes.Add(axis);
gaugeGrid.Add(gauge);

// Center value overlay
var vl = new MauiLabel { Text = $"{value:F1}", FontFamily = Theme.FontSemibold, FontSize = 20, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
var ul = new MauiLabel { Text = unit, FontFamily = Theme.FontRegular, FontSize = 9, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center };
valueLabel = vl;
unitLabel = ul;
var centerStack = new VerticalStackLayout { Spacing = 0, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TranslationY = 10 };
centerStack.Add(vl);
centerStack.Add(ul);
gaugeGrid.Add(centerStack);

// Stepper buttons at bottom corners of gauge, with icon between
var stepMinusBtn = new MauiButton
{
	Text = Icons.Remove, FontFamily = Icons.FontFamily, FontSize = 12,
	TextColor = Theme.TextSecondary, BackgroundColor = Colors.Transparent,
	WidthRequest = 32, HeightRequest = 32,
	CornerRadius = 16, Padding = 0, BorderWidth = 0,
	HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.End,
	TranslationY = 10,
};
stepMinusBtn.Clicked += (s, e) => onStep(-0.1);

var stepPlusBtn = new MauiButton
{
	Text = Icons.Add, FontFamily = Icons.FontFamily, FontSize = 12,
	TextColor = Theme.TextSecondary, BackgroundColor = Colors.Transparent,
	WidthRequest = 32, HeightRequest = 32,
	CornerRadius = 16, Padding = 0, BorderWidth = 0,
	HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.End,
	TranslationY = 10,
};
stepPlusBtn.Clicked += (s, e) => onStep(0.1);

// Coffee icon glyph at bottom center
var coffeeIconGlyph = label == "Dose In" ? "u" : "t";
var coffeeIcon = new MauiLabel
{
	Text = coffeeIconGlyph, FontFamily = Icons.CoffeeFontFamily, FontSize = 24,
	TextColor = Theme.TextSecondary,
	HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center,
	HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.End,
};

gaugeGrid.Add(stepMinusBtn);
gaugeGrid.Add(stepPlusBtn);
gaugeGrid.Add(coffeeIcon);

return gaugeGrid;
}

void UpdateDoseIn()
{
_doseInValueLabel!.Text = $"{_doseIn:F1}";
_doseInRange!.Value = _doseIn;
_doseInPointer!.Value = _doseIn;
_ratioLabel!.Text = $"1:{Ratio:F1}";
}

void UpdateDoseOut()
{
_doseOutValueLabel!.Text = $"{_doseOut:F1}";
_doseOutRange!.Value = _doseOut;
_doseOutPointer!.Value = _doseOut;
_ratioLabel!.Text = $"1:{Ratio:F1}";
}

Microsoft.Maui.Controls.View BuildEquipmentButton()
{
var stack = new VerticalStackLayout { Spacing = Theme.SpacingS, HorizontalOptions = LayoutOptions.Center };
stack.Add(new MauiBoxView { HeightRequest = 20, BackgroundColor = Colors.Transparent });

var circleGrid = new MauiGrid { WidthRequest = 50, HeightRequest = 50 };
circleGrid.Add(new MauiBorder
{
BackgroundColor = Theme.SurfaceVariant, StrokeThickness = 0,
StrokeShape = new RoundRectangle { CornerRadius = 25 },
WidthRequest = 50, HeightRequest = 50,
});
circleGrid.Add(new MauiLabel { Text = Icons.Machine, FontFamily = Icons.CoffeeFontFamily, FontSize = 32, TextColor = Theme.TextPrimary, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center });

_machineNameLabel = new MauiLabel { Text = "Select", FontFamily = Theme.FontRegular, FontSize = 11, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center, WidthRequest = 80 };

var tap = new TapGestureRecognizer();
tap.Tapped += async (s, e) => await ShowEquipmentSelectionPopup();
circleGrid.GestureRecognizers.Add(tap);
stack.GestureRecognizers.Add(tap);

stack.Add(circleGrid);
stack.Add(_machineNameLabel);
return stack;
}

async Task ShowEquipmentSelectionPopup()
{
try
{
var equipment = InMemoryDataStore.Instance?.GetAllEquipment() ?? new List<Models.Equipment>();
if (equipment.Count == 0)
{
	var alertPage = Services.PageHelper.GetCurrentPage();
	if (alertPage != null)
		await alertPage.DisplayAlertAsync("No Equipment", "Add equipment in Settings first.", "OK");
	return;
}

var items = equipment
	.OrderBy(e => e.Type)
	.ThenBy(e => e.Name)
	.Select(e => new EquipmentSelectionItem
	{
		Id = e.Id,
		Name = e.Name,
		EquipmentType = e.Type,
		IsSelected = (e.Type == EquipmentType.Machine && _machineIndex > 0 && _machineIndex <= _machines.Count && _machines[_machineIndex - 1].Id == e.Id)
			|| (e.Type == EquipmentType.Grinder && _grinderIndex > 0 && _grinderIndex <= _grinders.Count && _grinders[_grinderIndex - 1].Id == e.Id)
	})
	.ToList();

var popup = new UXDivers.Popups.Maui.Controls.ListActionPopup
{
	Title = "Select Equipment",
	ActionButtonText = "Done",
	ShowActionButton = true,
	ItemsSource = items,
	ItemDataTemplate = new DataTemplate(() =>
	{
		var tapGesture = new TapGestureRecognizer();
		tapGesture.SetBinding(TapGestureRecognizer.CommandParameterProperty, ".");
		tapGesture.Tapped += (s, e) =>
		{
			if (e is TappedEventArgs args && args.Parameter is EquipmentSelectionItem item)
			{
				item.IsSelected = !item.IsSelected;
				ApplyEquipmentSelection(items);
			}
		};

		var layout = new HorizontalStackLayout { Spacing = 12, Padding = new Thickness(0, 8) };
		layout.GestureRecognizers.Add(tapGesture);

		var checkIcon = new MauiLabel
		{
			FontSize = 24,
			VerticalOptions = LayoutOptions.Center,
		};
		checkIcon.SetBinding(MauiLabel.TextProperty, new Microsoft.Maui.Controls.Binding("IsSelected",
			converter: new BoolToStringConverter("✓", "○")));
		checkIcon.SetBinding(MauiLabel.TextColorProperty, new Microsoft.Maui.Controls.Binding("IsSelected",
			converter: new BoolToColorConverter(Theme.Primary, Theme.TextSecondary)));

		var nameLabel = new MauiLabel { FontSize = 16, VerticalOptions = LayoutOptions.Center, TextColor = Colors.White };
		nameLabel.SetBinding(MauiLabel.TextProperty, "Name");

		var typeLabel = new MauiLabel { FontSize = 12, VerticalOptions = LayoutOptions.Center, TextColor = Colors.Gray };
		typeLabel.SetBinding(MauiLabel.TextProperty, "TypeName");

		var textStack = new VerticalStackLayout { Spacing = 2 };
		textStack.Add(nameLabel);
		textStack.Add(typeLabel);

		layout.Add(checkIcon);
		layout.Add(textStack);
		return layout;
	})
};

popup.ActionButtonCommand = new Command(() => ApplyEquipmentSelection(items));

// Close any existing popup first (reference pattern from BaristaNotes)
try { await UXDivers.Popups.Services.IPopupService.Current.PopAsync(); } catch { }

await UXDivers.Popups.Services.IPopupService.Current.PushAsync(popup);
}
catch (Exception ex)
{
	System.Diagnostics.Debug.WriteLine($"[Equipment] ERROR: {ex}");
	// Fallback to action sheet
	var page = Services.PageHelper.GetCurrentPage();
	if (page != null)
	{
		var equipment = InMemoryDataStore.Instance?.GetAllEquipment() ?? new List<Models.Equipment>();
		var names = equipment.OrderBy(e => e.Type).ThenBy(e => e.Name)
			.Select(e => $"{e.Type}: {e.Name}").ToArray();
		var result = await page.DisplayActionSheetAsync("Select Equipment", "Cancel", null, names);
		if (result != null && result != "Cancel")
		{
			var selected = equipment.FirstOrDefault(e => $"{e.Type}: {e.Name}" == result);
			if (selected?.Type == EquipmentType.Machine)
			{
				_machineIndex = _machines.FindIndex(m => m.Id == selected.Id) + 1;
				var name = selected.Name;
				if (_machineNameLabel != null)
					_machineNameLabel.Text = name.Length > 10 ? name[..10] + "…" : name;
			}
			else if (selected?.Type == EquipmentType.Grinder)
			{
				_grinderIndex = _grinders.FindIndex(g => g.Id == selected.Id) + 1;
			}
		}
	}
}
}

void ApplyEquipmentSelection(List<EquipmentSelectionItem> items)
{
var selectedMachine = items.FirstOrDefault(i => i.EquipmentType == EquipmentType.Machine && i.IsSelected);
if (selectedMachine != null)
{
	_machineIndex = _machines.FindIndex(m => m.Id == selectedMachine.Id) + 1;
	var name = selectedMachine.Name;
	if (_machineNameLabel != null)
		_machineNameLabel.Text = name.Length > 10 ? name[..10] + "…" : name;
}
else
{
	_machineIndex = 0;
	if (_machineNameLabel != null)
		_machineNameLabel.Text = "Select";
}

var selectedGrinder = items.FirstOrDefault(i => i.EquipmentType == EquipmentType.Grinder && i.IsSelected);
if (selectedGrinder != null)
	_grinderIndex = _grinders.FindIndex(g => g.Id == selectedGrinder.Id) + 1;
else
	_grinderIndex = 0;
}

Microsoft.Maui.Controls.View BuildRatioDisplay()
{
_ratioLabel = new MauiLabel
{
	Text = $"1:{Ratio:F1}",
	FontFamily = Theme.FontSemibold,
	FontSize = 20,
	TextColor = Theme.TextSecondary,
	HorizontalTextAlignment = TextAlignment.Center,
	HorizontalOptions = LayoutOptions.Center,
	Margin = new Thickness(0, 8, 0, 0),
};
return _ratioLabel;
}

Microsoft.Maui.Controls.View BuildTimeSlider()
{
var stack = new VerticalStackLayout { Spacing = 0 };

_timeValueLabel = new MauiLabel
{
	Text = $"Time: {_actualTime:F0}s",
	FontFamily = Theme.FontRegular,
	FontSize = 12,
	TextColor = Theme.TextSecondary,
	Margin = new Thickness(16, 0, 0, 4),
};
stack.Add(_timeValueLabel);

var slider = new MauiSlider
{
	Minimum = 0, Maximum = 60, Value = _actualTime,
	MinimumTrackColor = Theme.Primary, MaximumTrackColor = Theme.SurfaceVariant,
	Margin = new Thickness(16, 0),
	VerticalOptions = LayoutOptions.Center,
};
slider.ValueChanged += (s, e) =>
{
_actualTime = e.NewValue;
_timeValueLabel.Text = $"Time: {e.NewValue:F0}s";
};

var sliderBorder = new MauiBorder
{
	Content = slider,
	StrokeThickness = 0,
	StrokeShape = new RoundRectangle { CornerRadius = Theme.RadiusPill },
	BackgroundColor = Theme.SurfaceVariant,
	HeightRequest = Theme.FormFieldHeight,
};

stack.Add(sliderBorder);
return stack;
}

Microsoft.Maui.Controls.View BuildUserSelectionRow()
{
var profileNames = new[] { "None" }.Concat(_profiles.Select(p => p.Name)).ToArray();

var grid = new MauiGrid
{
	ColumnDefinitions = new ColumnDefinitionCollection
	{
		new ColumnDefinition(GridLength.Star),
		new ColumnDefinition(GridLength.Auto),
		new ColumnDefinition(GridLength.Star),
	},
	RowDefinitions = new RowDefinitionCollection
	{
		new RowDefinition(GridLength.Auto),
	},
	ColumnSpacing = 16,
	HorizontalOptions = LayoutOptions.Center,
};

var madeByAvatarLabel = new MauiLabel();
var madeByNameLabel = new MauiLabel();
var madeByCircleBg = new MauiBorder();

var madeByStack = BuildAvatarControl("Made By", _madeByIndex, profileNames,
ref madeByAvatarLabel, ref madeByNameLabel, ref madeByCircleBg,
() => ShowProfileSelectionPopup("Made By", idx =>
{
	_madeByIndex = idx;
	UpdateAvatar(_madeByIndex, profileNames, madeByAvatarLabel, madeByNameLabel, madeByCircleBg);
}));
grid.Add(madeByStack, 0, 0);

var arrow = new MauiLabel
{
	Text = "→",
	FontFamily = Theme.FontRegular,
	FontSize = 24,
	TextColor = Theme.TextMuted,
	HorizontalTextAlignment = TextAlignment.Center,
	VerticalTextAlignment = TextAlignment.Center,
};
grid.Add(arrow, 1, 0);

var madeForAvatarLabel = new MauiLabel();
var madeForNameLabel = new MauiLabel();
var madeForCircleBg = new MauiBorder();

var madeForStack = BuildAvatarControl("Made For", _madeForIndex, profileNames,
ref madeForAvatarLabel, ref madeForNameLabel, ref madeForCircleBg,
() => ShowProfileSelectionPopup("Made For", idx =>
{
	_madeForIndex = idx;
	UpdateAvatar(_madeForIndex, profileNames, madeForAvatarLabel, madeForNameLabel, madeForCircleBg);
}));
grid.Add(madeForStack, 2, 0);

return grid;
}

async void ShowProfileSelectionPopup(string title, Action<int> onSelected)
{
var items = new List<string> { "None" };
items.AddRange(_profiles.Select(p => p.Name));

try
{
	var popup = new UXDivers.Popups.Maui.Controls.ListActionPopup
	{
		Title = title,
		ShowActionButton = false,
		ItemsSource = items.Select((name, idx) => new { Name = name, Index = idx }).ToList(),
		ItemDataTemplate = new Microsoft.Maui.Controls.DataTemplate(() =>
		{
			var tapGesture = new TapGestureRecognizer();
			tapGesture.Tapped += async (s, e) =>
			{
				var element = s as Microsoft.Maui.Controls.Element;
				var bindingCtx = element?.BindingContext;
				if (bindingCtx != null)
				{
					var indexProp = bindingCtx.GetType().GetProperty("Index");
					if (indexProp != null)
					{
						var idx = (int)indexProp.GetValue(bindingCtx)!;
						onSelected(idx);
						try { await UXDivers.Popups.Services.IPopupService.Current.PopAsync(); } catch { }
					}
				}
			};

			var layout = new HorizontalStackLayout { Spacing = 12, Padding = new Thickness(0, 8) };
			layout.GestureRecognizers.Add(tapGesture);

			var circle = new MauiBorder
			{
				StrokeShape = new Microsoft.Maui.Controls.Shapes.Ellipse(),
				HeightRequest = 40,
				WidthRequest = 40,
				BackgroundColor = Theme.Primary,
				StrokeThickness = 0,
			};
			var initial = new MauiLabel
			{
				FontFamily = Theme.FontSemibold,
				FontSize = 16,
				TextColor = Colors.White,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
			};
			initial.SetBinding(MauiLabel.TextProperty, new Microsoft.Maui.Controls.Binding("Name",
				converter: new InitialConverter()));
			circle.Content = initial;
			layout.Children.Add(circle);

			var label = new MauiLabel
			{
				FontSize = 16,
				TextColor = Colors.White,
				VerticalOptions = LayoutOptions.Center,
			};
			label.SetBinding(MauiLabel.TextProperty, "Name");
			layout.Children.Add(label);
			return layout;
		}),
	};
	await UXDivers.Popups.Services.IPopupService.Current.PushAsync(popup);
}
catch
{
	// Fallback to ActionSheet if UXDivers popup fails
	var fallbackPage = Services.PageHelper.GetCurrentPage();
	var result = fallbackPage != null
		? await fallbackPage.DisplayActionSheetAsync(title, "Cancel", null, items.ToArray())
		: null;
	if (result != null && result != "Cancel")
	{
		var idx = items.IndexOf(result);
		if (idx >= 0) onSelected(idx);
	}
}
}

Microsoft.Maui.Controls.View BuildAvatarControl(string title, int idx, string[] names,
ref MauiLabel avatarLabel, ref MauiLabel nameLabel, ref MauiBorder circleBg, Action onTap)
{
var stack = new VerticalStackLayout { Spacing = Theme.SpacingXS };

// Avatar circle (60x60)
var circleGrid = new MauiGrid { WidthRequest = 60, HeightRequest = 60 };
var bg = new MauiBorder
{
BackgroundColor = idx == 0 ? Theme.SurfaceVariant : Theme.Primary,
StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 30 }, WidthRequest = 60, HeightRequest = 60,
};
circleBg = bg;
circleGrid.Add(bg);

var initial = idx == 0 ? "?" : (idx - 1 < _profiles.Count ? _profiles[idx - 1].Name[..1].ToUpper() : "?");
var al = new MauiLabel
{
Text = initial, FontFamily = Theme.FontSemibold, FontSize = 24, FontAttributes = MauiFontAttributes.Bold,
TextColor = idx == 0 ? Theme.TextMuted : Colors.White,
HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center,
};
avatarLabel = al;
circleGrid.Add(al);
stack.Add(circleGrid);

// Label BELOW avatar
var labelText = title == "Made By" ? "Made by" : "For";
stack.Add(new MauiLabel { Text = labelText, FontFamily = Theme.FontRegular, FontSize = 12, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center });

var tap = new TapGestureRecognizer();
tap.Tapped += (s, e) => onTap();
stack.GestureRecognizers.Add(tap);

return stack;
}

void UpdateAvatar(int idx, string[] names, MauiLabel avatarLabel, MauiLabel nameLabel, MauiBorder circleBg)
{
var initial = idx == 0 ? "?" : (idx - 1 < _profiles.Count ? _profiles[idx - 1].Name[..1].ToUpper() : "?");
var fullName = idx == 0 ? "None" : (idx - 1 < _profiles.Count ? _profiles[idx - 1].Name : "None");
avatarLabel.Text = initial;
avatarLabel.TextColor = idx == 0 ? Theme.TextMuted : Colors.White;
nameLabel.Text = fullName;
circleBg.BackgroundColor = idx == 0 ? Theme.SurfaceVariant : Theme.Primary;
}

Microsoft.Maui.Controls.View BuildRating()
{
var sentiments = new[]
{
Icons.SentimentVeryDissatisfied,
Icons.SentimentDissatisfied,
Icons.SentimentNeutral,
Icons.SentimentSatisfied,
Icons.SentimentVerySatisfied,
};
var icons = new MauiLabel[sentiments.Length];
var row = new HorizontalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.Center };

for (int i = 0; i < sentiments.Length; i++)
{
var idx = i;
var lbl = new MauiLabel
{
Text = sentiments[i],
FontFamily = Icons.FontFamily,
FontSize = 32,
TextColor = i == _rating ? Theme.Primary : Theme.TextMuted,
};
icons[i] = lbl;

var tap = new TapGestureRecognizer();
tap.Tapped += (s, e) =>
{
_rating = idx;
for (int j = 0; j < sentiments.Length; j++)
icons[j].TextColor = j == _rating ? Theme.Primary : Theme.TextMuted;
};
lbl.GestureRecognizers.Add(tap);
row.Add(lbl);
}
return row;
}

View BuildTastingNotes() =>
	VStack(
		Text("Tasting Notes (optional)")
			.FontFamily(Theme.FontRegular).FontSize(12).Color(Theme.TextSecondary)
			.Margin(new Thickness(16, 0, 0, 4)),
		Border(
			TextEditor(_tastingNotes)
				.FontSize(16).FontFamily(Theme.FontRegular)
				.Color(Theme.TextPrimary).Background(Colors.Transparent)
				.Frame(height: 80)
				.Margin(new Thickness(16, 8))
				.Placeholder("E.g., bright, fruity, slightly sour...")
				.OnTextChanged(v => _tastingNotes = v)
		)
		.CornerRadius(Theme.RadiusEditor)
		.Background(Theme.SurfaceVariant)
		.StrokeThickness(0)
	);

// Bag picker reference for refreshing after inline creation
MauiPicker? _bagPicker;

Microsoft.Maui.Controls.View BuildBagPickerWithAdd(string[] bagNames)
{
var stack = new VerticalStackLayout { Spacing = 4 };
stack.Add(new MauiLabel { Text = "Bag", FontFamily = Theme.FontRegular, FontSize = 12, TextColor = Theme.TextSecondary, Margin = new Thickness(16, 0, 0, 4) });

var row = new MauiGrid
{
	ColumnDefinitions =
	{
		new ColumnDefinition(GridLength.Star),
		new ColumnDefinition(GridLength.Auto),
	},
	ColumnSpacing = Theme.SpacingS,
};

_bagPicker = new MauiPicker
{
	TextColor = Theme.TextPrimary,
	BackgroundColor = Theme.SurfaceVariant,
	HeightRequest = Theme.FormFieldHeight,
};
foreach (var item in bagNames) _bagPicker.Items.Add(item);
if (_selectedBagIndex >= 0 && _selectedBagIndex < bagNames.Length) _bagPicker.SelectedIndex = _selectedBagIndex;
_bagPicker.SelectedIndexChanged += (s, e) => _selectedBagIndex = _bagPicker.SelectedIndex;

var pickerBorder = new MauiBorder
{
	Content = _bagPicker,
	StrokeThickness = 0,
	StrokeShape = new RoundRectangle { CornerRadius = Theme.RadiusPill },
	BackgroundColor = Theme.SurfaceVariant,
};

var addBtn = new MauiButton
{
	Text = Icons.Add,
	FontFamily = Icons.FontFamily,
	FontSize = 22,
	TextColor = Colors.White,
	BackgroundColor = Theme.Primary,
	WidthRequest = Theme.FormFieldHeight,
	HeightRequest = Theme.FormFieldHeight,
	CornerRadius = (int)Theme.RadiusPill,
	Padding = 0,
};
addBtn.Clicked += async (s, e) => await AddNewBeanInline();

row.Add(pickerBorder, 0, 0);
row.Add(addBtn, 1, 0);
stack.Add(row);
return stack;
}

async Task AddNewBeanInline()
{
var page = Services.PageHelper.GetCurrentPage();
if (page == null) return;

var beanName = await page.DisplayPromptAsync(
	"New Bean",
	"Enter the bean name to create a new bean and bag:",
	"Create",
	"Cancel",
	"e.g. Ethiopia Sidamo",
	maxLength: 100,
	keyboard: Keyboard.Default);

if (string.IsNullOrWhiteSpace(beanName)) return;

var store = InMemoryDataStore.Instance;
if (store == null) return;

var bean = store.CreateBean(new Bean { Name = beanName.Trim() });
var bag = store.CreateBag(new Bag { BeanId = bean.Id, RoastDate = DateTime.Now });

// Refresh local bags list and update picker
_bags = store.GetAllBags().Where(b => !b.IsComplete).ToList();
var newIndex = _bags.FindIndex(b => b.Id == bag.Id);

if (_bagPicker != null)
{
	_bagPicker.Items.Clear();
	foreach (var b in _bags)
		_bagPicker.Items.Add(b.BeanName ?? $"Bag #{b.Id}");
	if (newIndex >= 0)
	{
		_bagPicker.SelectedIndex = newIndex;
		_selectedBagIndex = newIndex;
	}
}
}

Microsoft.Maui.Controls.View BuildAdditionalDetails()
{
var wrapper = new VerticalStackLayout { Spacing = Theme.SpacingS };

// Divider line
var divider = new MauiBoxView
{
	HeightRequest = 1,
	BackgroundColor = Theme.Outline,
	HorizontalOptions = LayoutOptions.Fill,
	Margin = new Thickness(0, Theme.SpacingL, 0, 0),
};
wrapper.Add(divider);

// Section header
var headerLabel = new MauiLabel
{
	Text = "Additional Details",
	FontFamily = Theme.FontRegular,
	FontSize = 14,
	TextColor = Theme.TextMuted,
};
wrapper.Add(headerLabel);

_additionalStack = new VerticalStackLayout { Spacing = Theme.SpacingM, IsVisible = true };

var bagNames = _bags.Select(b => b.BeanName ?? $"Bag #{b.Id}").ToArray();

// Field order: Bag → Grind Setting → Expected Time → Expected Output → Drink Type
_additionalStack.Add(BuildBagPickerWithAdd(bagNames));
_additionalStack.Add(FormHelpers.MakeFormEntry("Grind Setting", _grindSetting, "e.g. 15", v => _grindSetting = v));
_additionalStack.Add(FormHelpers.MakeFormEntry("Expected Time (s)", _expectedTime, "28", v => _expectedTime = v));
_additionalStack.Add(FormHelpers.MakeFormEntry("Expected Output (g)", _expectedOutput, "36", v => _expectedOutput = v));
_additionalStack.Add(FormHelpers.MakeFormPicker("Drink Type", _drinkTypeIndex, DrinkTypes, v => _drinkTypeIndex = v));

_additionalStack.Padding = new Thickness(0, 0, 0, Theme.SpacingXL);
wrapper.Add(_additionalStack);
return wrapper;
}

void LoadExistingShot(IDataStore? store)
{
	if (store == null) return;
	var shot = store.GetShot(_editingShotId);
	if (shot == null) return;

	_doseIn = (double)shot.DoseIn;
	_doseOut = (double)(shot.ActualOutput ?? 0m);
	_actualTime = (double)(shot.ActualTime ?? 0m);
	_rating = shot.Rating ?? -1;
	_tastingNotes = shot.TastingNotes ?? "";
	_grindSetting = shot.GrindSetting ?? "15";
	_expectedTime = shot.ExpectedTime.ToString("G");
	_expectedOutput = shot.ExpectedOutput.ToString("G");

	var dtIdx = Array.IndexOf(DrinkTypes, shot.DrinkType);
	_drinkTypeIndex = dtIdx >= 0 ? dtIdx : 0;

	if (shot.MachineId.HasValue)
	{
		var mi = _machines.FindIndex(m => m.Id == shot.MachineId.Value);
		_machineIndex = mi >= 0 ? mi + 1 : 0;
	}
	if (shot.GrinderId.HasValue)
	{
		var gi = _grinders.FindIndex(g => g.Id == shot.GrinderId.Value);
		_grinderIndex = gi >= 0 ? gi + 1 : 0;
	}
	if (shot.MadeById.HasValue)
	{
		var bi = _profiles.FindIndex(p => p.Id == shot.MadeById.Value);
		_madeByIndex = bi >= 0 ? bi + 1 : 0;
	}
	if (shot.MadeForId.HasValue)
	{
		var fi = _profiles.FindIndex(p => p.Id == shot.MadeForId.Value);
		_madeForIndex = fi >= 0 ? fi + 1 : 0;
	}

	if (shot.BagId > 0)
	{
		var allBags = store.GetAllBags();
		var activeBagIdx = _bags.FindIndex(b => b.Id == shot.BagId);
		if (activeBagIdx < 0)
		{
			var bag = allBags.FirstOrDefault(b => b.Id == shot.BagId);
			if (bag != null)
			{
				_bags.Add(bag);
				_selectedBagIndex = _bags.Count - 1;
			}
		}
		else
		{
			_selectedBagIndex = activeBagIdx;
		}
	}
}

void SaveShot()
{
if (_savingIndicator != null)
{
	_savingIndicator.IsRunning = true;
	_savingIndicator.IsVisible = true;
}
if (_saveButton != null)
	_saveButton.IsEnabled(false);

var store = InMemoryDataStore.Instance;
if (store == null)
{
	SetSavingState(false);
	return;
}

var bagIdx = _selectedBagIndex;
if (bagIdx < 0 && _bags.Count > 0) bagIdx = 0;
if (bagIdx < 0 || bagIdx >= _bags.Count)
{
	Services.PageHelper.DispatchOnMainThread(async () =>
	{
		await _feedbackService.ShowWarning("Please select a coffee bag before saving your shot. Add a bag in Settings if none are available.");
	});
	SetSavingState(false);
	return;
}

var machineIdx = _machineIndex - 1;
var grinderIdx = _grinderIndex - 1;
var madeByIdx = _madeByIndex - 1;
var madeForIdx = _madeForIndex - 1;

var drinkType = _drinkTypeIndex >= 0 && _drinkTypeIndex < DrinkTypes.Length ? DrinkTypes[_drinkTypeIndex] : "Espresso";

var record = new ShotRecord
{
BagId = _bags[bagIdx].Id,
DoseIn = (decimal)_doseIn,
GrindSetting = _grindSetting,
ExpectedTime = decimal.TryParse(_expectedTime, out var et) ? et : 28m,
ExpectedOutput = decimal.TryParse(_expectedOutput, out var eo) ? eo : 36m,
ActualTime = (decimal)_actualTime,
ActualOutput = (decimal)_doseOut,
Rating = _rating >= 0 ? _rating : null,
TastingNotes = string.IsNullOrWhiteSpace(_tastingNotes) ? null : _tastingNotes,
DrinkType = drinkType,
MachineId = machineIdx >= 0 && machineIdx < _machines.Count ? _machines[machineIdx].Id : null,
GrinderId = grinderIdx >= 0 && grinderIdx < _grinders.Count ? _grinders[grinderIdx].Id : null,
MadeById = madeByIdx >= 0 && madeByIdx < _profiles.Count ? _profiles[madeByIdx].Id : null,
MadeForId = madeForIdx >= 0 && madeForIdx < _profiles.Count ? _profiles[madeForIdx].Id : null,
};

if (IsEditMode)
{
	record.Id = _editingShotId;
	store.UpdateShot(record);
}
else
{
	store.CreateShot(record);
}

Services.PageHelper.DispatchOnMainThread(async () =>
{
	if (IsEditMode)
	{
		await _feedbackService.ShowSuccess($"Your {drinkType} shot ({_doseIn:F1}g dose) has been updated.");
		Navigation?.Pop();
	}
	else
	{
		await _feedbackService.ShowSuccess($"Your {drinkType} shot ({_doseIn:F1}g dose) has been recorded.");
	}
	SetSavingState(false);
});
}

void SetSavingState(bool isSaving)
{
	if (_savingIndicator != null)
	{
		_savingIndicator.IsRunning = isSaving;
		_savingIndicator.IsVisible = isSaving;
	}
	if (_saveButton != null)
		_saveButton.IsEnabled(!isSaving);
}

async Task DeleteShot()
{
	var page = Services.PageHelper.GetCurrentPage();
	if (page == null) return;

	var confirm = await page.DisplayAlertAsync(
		"Delete Shot",
		"Are you sure you want to delete this shot? This cannot be undone.",
		"Delete", "Cancel");

	if (confirm)
	{
		InMemoryDataStore.Instance?.DeleteShot(_editingShotId);
		Navigation?.Pop();
	}
}

public class InitialConverter : Microsoft.Maui.Controls.IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
	{
		if (value is string s && !string.IsNullOrEmpty(s))
			return s == "None" ? "?" : s[..1].ToUpper();
		return "?";
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
		=> throw new NotImplementedException();
}
}
