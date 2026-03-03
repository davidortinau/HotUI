using Comet;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;
using Syncfusion.Maui.Gauges;

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

/// <summary>
/// Shot logging page using imperative updates for smooth interaction.
/// Native MAUI controls are built once and updated directly in event handlers,
/// avoiding full UI rebuild on each state change.
/// </summary>
public class ShotLoggingPage : Comet.View
{
// Current values (not Comet State<T> — we update controls directly)
double _doseIn = 18.0;
double _doseOut = 36.0;
double _actualTime = 0;
int _rating = 3;
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
bool _showAdditional = false;

// References to mutable controls
MauiLabel? _doseInValueLabel, _doseInUnitLabel;
MauiLabel? _doseOutValueLabel, _doseOutUnitLabel;
RadialRange? _doseInRange, _doseOutRange;
NeedlePointer? _doseInPointer, _doseOutPointer;
MauiLabel? _ratioLabel;
MauiLabel? _timeValueLabel;
MauiLabel? _machineNameLabel;
VerticalStackLayout? _additionalStack;
MauiBorder? _additionalHeaderCard;

// Data
List<Bag> _bags = new();
List<Equipment> _machines = new();
List<Equipment> _grinders = new();
List<UserProfile> _profiles = new();

static readonly string[] DrinkTypes = { "Espresso", "Ristretto", "Lungo", "Doppio", "Americano" };

double Ratio => _doseIn > 0 ? Math.Round(_doseOut / _doseIn, 1) : 0;

[Body]
Comet.View body()
{
var store = InMemoryDataStore.Instance;
_bags = store?.GetAllBags().Where(b => !b.IsComplete).ToList() ?? new();
_machines = store?.GetByType(EquipmentType.Machine) ?? new();
_grinders = store?.GetByType(EquipmentType.Grinder) ?? new();
_profiles = store?.GetAllProfiles() ?? new();

var contentStack = new VerticalStackLayout { Spacing = Theme.SpacingM, Padding = new Thickness(Theme.SpacingM) };

contentStack.Add(BuildDoseGaugesRow());
contentStack.Add(BuildRatioDisplay());
contentStack.Add(BuildTimeSlider());
contentStack.Add(BuildUserSelectionRow());
contentStack.Add(BuildRating());
contentStack.Add(BuildTastingNotes());
contentStack.Add(BuildAdditionalDetails());

var saveBtn = FormHelpers.MakePrimaryButton("Save Shot", SaveShot);
saveBtn.Margin = new Thickness(0, Theme.SpacingS, 0, Theme.SpacingXL);
contentStack.Add(saveBtn);

var scrollView = new MauiScrollView { Content = contentStack, BackgroundColor = Theme.Background };
return new MauiViewHost(scrollView);
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
delta => { _doseIn = Math.Clamp(_doseIn + delta, 10, 25); UpdateDoseIn(); }), 0, 0);

grid.Add(BuildEquipmentButton(), 1, 0);

grid.Add(BuildGauge("Dose Out", _doseOut, "g", 0, 60,
ref _doseOutValueLabel, ref _doseOutUnitLabel, ref _doseOutRange, ref _doseOutPointer,
delta => { _doseOut = Math.Clamp(_doseOut + delta, 20, 60); UpdateDoseOut(); }), 2, 0);

return grid;
}

Microsoft.Maui.Controls.View BuildGauge(string label, double value, string unit,
double min, double max,
ref MauiLabel? valueLabel, ref MauiLabel? unitLabel,
ref RadialRange? range, ref NeedlePointer? pointer,
Action<double> onStep)
{
var stack = new VerticalStackLayout { Spacing = Theme.SpacingS, HorizontalOptions = LayoutOptions.Center };
stack.Add(new MauiLabel { Text = label, FontFamily = Theme.FontSemibold, FontSize = 12, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center });

var gauge = new SfRadialGauge { WidthRequest = 140, HeightRequest = 140 };
var axis = new RadialAxis
{
Minimum = min, Maximum = max,
ShowLabels = false, ShowTicks = false,
AxisLineStyle = new RadialLineStyle { Thickness = 8, Fill = new SolidColorBrush(Theme.SurfaceVariant) }
};

var r = new RadialRange { StartValue = 0, EndValue = value, Fill = new SolidColorBrush(Theme.Primary), StartWidth = 8, EndWidth = 8 };
range = r;
axis.Ranges.Add(r);

var p = new NeedlePointer
{
Value = value, NeedleFill = new SolidColorBrush(Theme.Primary),
NeedleLength = 0.6, NeedleStartWidth = 2, NeedleEndWidth = 2,
KnobRadius = 6, KnobFill = new SolidColorBrush(Theme.Primary)
};
pointer = p;
axis.Pointers.Add(p);

var vl = new MauiLabel { Text = $"{value:F1}", FontFamily = Theme.FontSemibold, FontSize = 22, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary, HorizontalTextAlignment = TextAlignment.Center };
var ul = new MauiLabel { Text = unit, FontFamily = Theme.FontRegular, FontSize = 12, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center };
valueLabel = vl;
unitLabel = ul;

var ann = new GaugeAnnotation { DirectionUnit = AnnotationDirection.Angle, DirectionValue = 90, PositionFactor = 0 };
var annContent = new VerticalStackLayout { Spacing = 0, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
annContent.Add(vl);
annContent.Add(ul);
ann.Content = annContent;
axis.Annotations.Add(ann);

gauge.Axes.Add(axis);
stack.Add(gauge);

// Stepper buttons
var stepRow = new HorizontalStackLayout { Spacing = Theme.SpacingS, HorizontalOptions = LayoutOptions.Center };
stepRow.Add(MakeStepButton(Icons.Remove, () => onStep(-0.5)));
stepRow.Add(MakeStepButton(Icons.Add, () => onStep(0.5)));
stack.Add(stepRow);

return stack;
}

Microsoft.Maui.Controls.View MakeStepButton(string icon, Action onTap)
{
var btn = new MauiButton
{
Text = icon, FontFamily = Icons.FontFamily, FontSize = 20,
TextColor = Theme.Primary, BackgroundColor = Theme.SurfaceVariant,
WidthRequest = 44, HeightRequest = 44,
CornerRadius = (int)Theme.RadiusCard,
Padding = 0, BorderWidth = 0,
};
btn.Clicked += (s, e) => onTap();
return btn;
}

void UpdateDoseIn()
{
_doseInValueLabel!.Text = $"{_doseIn:F1}";
_doseInRange!.EndValue = _doseIn;
_doseInPointer!.Value = _doseIn;
_ratioLabel!.Text = $"1:{Ratio:F1}";
}

void UpdateDoseOut()
{
_doseOutValueLabel!.Text = $"{_doseOut:F1}";
_doseOutRange!.EndValue = _doseOut;
_doseOutPointer!.Value = _doseOut;
_ratioLabel!.Text = $"1:{Ratio:F1}";
}

Microsoft.Maui.Controls.View BuildEquipmentButton()
{
var stack = new VerticalStackLayout { Spacing = Theme.SpacingS, HorizontalOptions = LayoutOptions.Center };
stack.Add(new MauiBoxView { HeightRequest = 20, BackgroundColor = Colors.Transparent });

var circleGrid = new MauiGrid { WidthRequest = Theme.EquipmentButtonSize, HeightRequest = Theme.EquipmentButtonSize };
circleGrid.Add(new MauiBorder
{
BackgroundColor = Theme.Primary, StrokeThickness = 0,
StrokeShape = new MauiEllipse(),
WidthRequest = Theme.EquipmentButtonSize, HeightRequest = Theme.EquipmentButtonSize,
});
circleGrid.Add(new MauiLabel { Text = Icons.Machine, FontFamily = Icons.CoffeeFontFamily, FontSize = 24, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center });

_machineNameLabel = new MauiLabel { Text = "Select", FontFamily = Theme.FontRegular, FontSize = 11, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center, WidthRequest = 80 };

var tap = new TapGestureRecognizer();
tap.Tapped += (s, e) =>
{
_machineIndex = (_machineIndex + 1) % (_machines.Count + 1);
var name = _machineIndex > 0 && _machineIndex <= _machines.Count ? _machines[_machineIndex - 1].Name : "Select";
_machineNameLabel.Text = name.Length > 10 ? name[..10] + "…" : name;
};
circleGrid.GestureRecognizers.Add(tap);

stack.Add(circleGrid);
stack.Add(_machineNameLabel);
return stack;
}

Microsoft.Maui.Controls.View BuildRatioDisplay()
{
var hstack = new HorizontalStackLayout { Spacing = Theme.SpacingXS, HorizontalOptions = LayoutOptions.Center, Padding = new Thickness(Theme.SpacingS) };
hstack.Add(new MauiLabel { Text = "Ratio: ", FontFamily = Theme.FontRegular, FontSize = 16, TextColor = Theme.TextSecondary });
_ratioLabel = new MauiLabel { Text = $"1:{Ratio:F1}", FontFamily = Theme.FontSemibold, FontSize = 18, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary };
hstack.Add(_ratioLabel);
return hstack;
}

Microsoft.Maui.Controls.View BuildTimeSlider()
{
var content = new VerticalStackLayout { Spacing = Theme.SpacingS };
var header = new MauiGrid();
header.Add(new MauiLabel { Text = "Time", FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary, HorizontalOptions = LayoutOptions.Start });
_timeValueLabel = new MauiLabel { Text = $"{_actualTime:F0}s", FontFamily = Theme.FontSemibold, FontSize = 16, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary, HorizontalOptions = LayoutOptions.End };
header.Add(_timeValueLabel);
content.Add(header);

var slider = new MauiSlider { Minimum = 0, Maximum = 60, Value = _actualTime, MinimumTrackColor = Theme.Primary, MaximumTrackColor = Theme.SurfaceVariant };
slider.ValueChanged += (s, e) =>
{
_actualTime = e.NewValue;
_timeValueLabel.Text = $"{e.NewValue:F0}s";
};
content.Add(slider);

return FormHelpers.MakeCard(content);
}

Microsoft.Maui.Controls.View BuildUserSelectionRow()
{
var profileNames = new[] { "None" }.Concat(_profiles.Select(p => p.Name)).ToArray();
var hstack = new HorizontalStackLayout { Spacing = Theme.SpacingM };

var madeByAvatarLabel = new MauiLabel();
var madeByNameLabel = new MauiLabel();
var madeByCircleBg = new MauiBorder();

var madeByStack = BuildAvatarControl("Made By", _madeByIndex, profileNames,
ref madeByAvatarLabel, ref madeByNameLabel, ref madeByCircleBg,
() => { _madeByIndex = (_madeByIndex + 1) % profileNames.Length; UpdateAvatar(_madeByIndex, profileNames, madeByAvatarLabel, madeByNameLabel, madeByCircleBg); });
hstack.Add(madeByStack);

hstack.Add(new MauiLabel { Text = "→", FontFamily = Theme.FontRegular, FontSize = 20, TextColor = Theme.TextMuted, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, Theme.SpacingM) });

var madeForAvatarLabel = new MauiLabel();
var madeForNameLabel = new MauiLabel();
var madeForCircleBg = new MauiBorder();

var madeForStack = BuildAvatarControl("Made For", _madeForIndex, profileNames,
ref madeForAvatarLabel, ref madeForNameLabel, ref madeForCircleBg,
() => { _madeForIndex = (_madeForIndex + 1) % profileNames.Length; UpdateAvatar(_madeForIndex, profileNames, madeForAvatarLabel, madeForNameLabel, madeForCircleBg); });
hstack.Add(madeForStack);

return FormHelpers.MakeCard(hstack);
}

Microsoft.Maui.Controls.View BuildAvatarControl(string title, int idx, string[] names,
ref MauiLabel avatarLabel, ref MauiLabel nameLabel, ref MauiBorder circleBg, Action onTap)
{
var stack = new VerticalStackLayout { Spacing = Theme.SpacingXS };
stack.Add(new MauiLabel { Text = title, FontFamily = Theme.FontRegular, FontSize = 12, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center });

var circleGrid = new MauiGrid { WidthRequest = 44, HeightRequest = 44 };
var bg = new MauiBorder
{
BackgroundColor = idx == 0 ? Theme.SurfaceVariant : Theme.Primary,
StrokeThickness = 0, StrokeShape = new MauiEllipse(), WidthRequest = 44, HeightRequest = 44,
};
circleBg = bg;
circleGrid.Add(bg);

var initial = idx == 0 ? "?" : (idx - 1 < _profiles.Count ? _profiles[idx - 1].Name[..1].ToUpper() : "?");
var al = new MauiLabel
{
Text = initial, FontFamily = Theme.FontSemibold, FontSize = 18, FontAttributes = MauiFontAttributes.Bold,
TextColor = idx == 0 ? Theme.TextMuted : Colors.White,
HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center,
};
avatarLabel = al;
circleGrid.Add(al);
stack.Add(circleGrid);

var fullName = idx == 0 ? "None" : (idx - 1 < _profiles.Count ? _profiles[idx - 1].Name : "None");
var nl = new MauiLabel { Text = fullName, FontFamily = Theme.FontRegular, FontSize = 11, TextColor = Theme.TextSecondary, HorizontalTextAlignment = TextAlignment.Center };
nameLabel = nl;
stack.Add(nl);

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
var content = new VerticalStackLayout { Spacing = Theme.SpacingS };
content.Add(new MauiLabel { Text = "Rating", FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary });

var sentiments = new[] { Icons.SentimentVeryDissatisfied, Icons.SentimentDissatisfied, Icons.SentimentNeutral, Icons.SentimentSatisfied, Icons.SentimentVerySatisfied };
var icons = new MauiLabel[5];
var row = new HorizontalStackLayout { Spacing = Theme.SpacingS, HorizontalOptions = LayoutOptions.Center };

for (int i = 0; i < 5; i++)
{
var idx = i;
var lbl = new MauiLabel
{
Text = sentiments[i], FontFamily = Icons.FontFamily, FontSize = 32,
TextColor = (i + 1) <= _rating ? Theme.Primary : Theme.StarEmpty,
};
icons[i] = lbl;

var tap = new TapGestureRecognizer();
tap.Tapped += (s, e) =>
{
_rating = idx + 1;
for (int j = 0; j < 5; j++)
icons[j].TextColor = (j + 1) <= _rating ? Theme.Primary : Theme.StarEmpty;
};
lbl.GestureRecognizers.Add(tap);
row.Add(lbl);
}
content.Add(row);
return FormHelpers.MakeCard(content);
}

Microsoft.Maui.Controls.View BuildTastingNotes()
{
var content = new VerticalStackLayout { Spacing = Theme.SpacingS };
content.Add(new MauiLabel { Text = "Tasting Notes", FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary });

var editor = new MauiEditor
{
Text = _tastingNotes, FontSize = 16, FontFamily = Theme.FontRegular,
TextColor = Theme.TextPrimary, BackgroundColor = Theme.SurfaceVariant, HeightRequest = 80,
Placeholder = "E.g., bright, fruity, slightly sour..."
};
editor.TextChanged += (s, e) => _tastingNotes = e.NewTextValue ?? "";

var border = new MauiBorder
{
Content = editor, StrokeThickness = 0,
StrokeShape = new RoundRectangle { CornerRadius = Theme.RadiusEditor },
BackgroundColor = Theme.SurfaceVariant,
};
content.Add(border);
return FormHelpers.MakeCard(content);
}

Microsoft.Maui.Controls.View BuildAdditionalDetails()
{
var wrapper = new VerticalStackLayout { Spacing = Theme.SpacingS };

var headerGrid = new MauiGrid();
headerGrid.Add(new MauiLabel { Text = "Additional Details", FontFamily = Theme.FontSemibold, FontSize = 16, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary, HorizontalOptions = LayoutOptions.Start });
var chevron = new MauiLabel { Text = "▶", FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextMuted, HorizontalOptions = LayoutOptions.End };
headerGrid.Add(chevron);

_additionalHeaderCard = (MauiBorder)FormHelpers.MakeCard(headerGrid);

_additionalStack = new VerticalStackLayout { Spacing = Theme.SpacingS, IsVisible = false };

var bagNames = _bags.Select(b => b.BeanName ?? $"Bag #{b.Id}").ToArray();
var grinderNames = new[] { "None" }.Concat(_grinders.Select(g => g.Name)).ToArray();

_additionalStack.Add(FormHelpers.MakeFormPicker("Coffee Bag", _selectedBagIndex, bagNames, v => _selectedBagIndex = v));
_additionalStack.Add(FormHelpers.MakeFormPicker("Drink Type", _drinkTypeIndex, DrinkTypes, v => _drinkTypeIndex = v));
_additionalStack.Add(FormHelpers.MakeFormPicker("Grinder", _grinderIndex, grinderNames, v => _grinderIndex = v));
_additionalStack.Add(FormHelpers.MakeFormEntry("Grind Setting", _grindSetting, "e.g. 15", v => _grindSetting = v));
_additionalStack.Add(FormHelpers.MakeFormEntry("Expected Time (s)", _expectedTime, "28", v => _expectedTime = v));
_additionalStack.Add(FormHelpers.MakeFormEntry("Expected Output (g)", _expectedOutput, "36", v => _expectedOutput = v));

var headerTap = new TapGestureRecognizer();
headerTap.Tapped += (s, e) =>
{
_showAdditional = !_showAdditional;
_additionalStack.IsVisible = _showAdditional;
chevron.Text = _showAdditional ? "▼" : "▶";
};
_additionalHeaderCard.GestureRecognizers.Add(headerTap);

wrapper.Add(_additionalHeaderCard);
wrapper.Add(_additionalStack);
return wrapper;
}

void SaveShot()
{
var store = InMemoryDataStore.Instance;
if (store == null) return;

var bagIdx = _selectedBagIndex;
if (bagIdx < 0 && _bags.Count > 0) bagIdx = 0;
if (bagIdx < 0 || bagIdx >= _bags.Count) return;

var machineIdx = _machineIndex - 1;
var grinderIdx = _grinderIndex - 1;
var madeByIdx = _madeByIndex - 1;
var madeForIdx = _madeForIndex - 1;

store.CreateShot(new ShotRecord
{
BagId = _bags[bagIdx].Id,
DoseIn = (decimal)_doseIn,
GrindSetting = _grindSetting,
ExpectedTime = decimal.TryParse(_expectedTime, out var et) ? et : 28m,
ExpectedOutput = decimal.TryParse(_expectedOutput, out var eo) ? eo : 36m,
ActualTime = (decimal)_actualTime,
ActualOutput = (decimal)_doseOut,
Rating = _rating,
TastingNotes = string.IsNullOrWhiteSpace(_tastingNotes) ? null : _tastingNotes,
DrinkType = _drinkTypeIndex >= 0 && _drinkTypeIndex < DrinkTypes.Length ? DrinkTypes[_drinkTypeIndex] : "Espresso",
MachineId = machineIdx >= 0 && machineIdx < _machines.Count ? _machines[machineIdx].Id : null,
GrinderId = grinderIdx >= 0 && grinderIdx < _grinders.Count ? _grinders[grinderIdx].Id : null,
MadeById = madeByIdx >= 0 && madeByIdx < _profiles.Count ? _profiles[madeByIdx].Id : null,
MadeForId = madeForIdx >= 0 && madeForIdx < _profiles.Count ? _profiles[madeForIdx].Id : null,
});

// Show success and allow logging another
if (Navigation != null)
Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(async () =>
{
await Microsoft.Maui.Controls.Shell.Current.DisplayAlert("Shot Logged!", "Your espresso shot has been recorded.", "OK");
});
}
}
