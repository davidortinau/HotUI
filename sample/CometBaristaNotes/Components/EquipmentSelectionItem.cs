using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using System.ComponentModel;
using System.Globalization;

namespace CometBaristaNotes.Components;

/// <summary>
/// View model for equipment items in the selection popup.
/// Implements INotifyPropertyChanged so checkbox state updates in the DataTemplate.
/// </summary>
public class EquipmentSelectionItem : INotifyPropertyChanged
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public EquipmentType EquipmentType { get; set; }
	public string TypeName => EquipmentType.ToString();

	bool _isSelected;
	public bool IsSelected
	{
		get => _isSelected;
		set
		{
			if (_isSelected != value)
			{
				_isSelected = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
			}
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>
/// Converts a bool to one of two string values.
/// </summary>
public class BoolToStringConverter : Microsoft.Maui.Controls.IValueConverter
{
	readonly string _trueValue;
	readonly string _falseValue;

	public BoolToStringConverter(string trueValue, string falseValue)
	{
		_trueValue = trueValue;
		_falseValue = falseValue;
	}

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> value is true ? _trueValue : _falseValue;

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}

/// <summary>
/// Converts a bool to one of two Color values.
/// </summary>
public class BoolToColorConverter : Microsoft.Maui.Controls.IValueConverter
{
	readonly Color _trueColor;
	readonly Color _falseColor;

	public BoolToColorConverter(Color trueColor, Color falseColor)
	{
		_trueColor = trueColor;
		_falseColor = falseColor;
	}

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> value is true ? _trueColor : _falseColor;

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
