using Comet;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;

namespace CometBaristaNotes.Pages;

public class BagFormPage : Comet.View
{
	readonly int _beanId;

	[State] readonly State<string> _roastDate = new(DateTime.Now.ToString("yyyy-MM-dd"));
	[State] readonly State<string> _notes = new("");
	[State] readonly State<string> _error = new("");
	[State] readonly State<string> _beanName = new("");
	[State] readonly State<bool> _isLoaded = new(false);

	public BagFormPage(int beanId = 0) { _beanId = beanId; }

	void LoadBeanName()
	{
		var store = InMemoryDataStore.Instance;
		if (store != null)
		{
			var bean = store.GetBean(_beanId);
			_beanName.Value = bean?.Name ?? "Unknown Bean";
		}
		_isLoaded.Value = true;
	}

	void Save()
	{
		if (!DateTime.TryParse(_roastDate.Value, out var roastDate))
		{
			_error.Value = "Please enter a valid date (yyyy-MM-dd)";
			return;
		}

		if (roastDate.Date > DateTime.Now.Date)
		{
			_error.Value = "Roast date cannot be in the future";
			return;
		}

		_error.Value = "";

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		store.CreateBag(new Bag
		{
			BeanId = _beanId,
			RoastDate = roastDate,
			Notes = string.IsNullOrWhiteSpace(_notes.Value) ? null : _notes.Value,
		});

		Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadBeanName();

		var stack = new VerticalStackLayout { Spacing = Theme.SpacingS, Padding = new Thickness(Theme.SpacingM) };

		stack.Add(FormHelpers.MakeSectionHeader("ADD BAG"));
		stack.Add(FormHelpers.MakeReadOnlyField("Bean", _beanName.Value));
		stack.Add(FormHelpers.MakeFormEntry("Roast Date", _roastDate.Value, "yyyy-MM-dd", v => _roastDate.Value = v));
		stack.Add(FormHelpers.MakeFormEntry("Notes (optional)", _notes.Value, "e.g., From Trader Joe's, Gift from friend", v => _notes.Value = v));

		if (!string.IsNullOrEmpty(_error.Value))
			stack.Add(new MauiLabel { Text = _error.Value, TextColor = Theme.Error, FontFamily = Theme.FontRegular, FontSize = 14 });

		stack.Add(FormHelpers.MakePrimaryButton("Add Bag", Save));

		var scrollView = new MauiScrollView
		{
			Content = stack,
			BackgroundColor = Theme.Background,
		};

		return new MauiViewHost(scrollView);
	}
}
