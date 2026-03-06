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
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;
using MauiFontAttributes = Microsoft.Maui.Controls.FontAttributes;

namespace CometBaristaNotes.Pages;

public class BagDetailPage : Comet.View
{
	readonly int _bagId;

	[State] readonly State<string> _beanName = new("");
	[State] readonly State<int> _beanId = new(0);
	[State] readonly State<string> _roastDate = new("");
	[State] readonly State<string> _notes = new("");
	[State] readonly State<bool> _isComplete = new(false);
	[State] readonly State<int> _shotCount = new(0);
	[State] readonly State<bool> _isLoaded = new(false);
	[State] readonly State<string> _error = new("");
	[State] readonly State<RatingAggregate> _rating = new(new());

	public BagDetailPage(int bagId = 0) { _bagId = bagId; }

	void LoadBag()
	{
		if (_bagId <= 0) { _isLoaded.Value = true; return; }

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		var bag = store.GetBag(_bagId);
		if (bag == null) { _error.Value = "Bag not found"; _isLoaded.Value = true; return; }

		_beanName.Value = bag.BeanName ?? "";
		_beanId.Value = bag.BeanId;
		_roastDate.Value = bag.RoastDate.ToString("yyyy-MM-dd");
		_notes.Value = bag.Notes ?? "";
		_isComplete.Value = bag.IsComplete;
		_shotCount.Value = bag.ShotCount;
		_rating.Value = store.GetBagRating(_bagId);

		_isLoaded.Value = true;
	}

	void Save()
	{
		_error.Value = "";
		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		if (_bagId > 0)
		{
			store.UpdateBag(new Bag
			{
				Id = _bagId,
				BeanId = _beanId.Value,
				RoastDate = DateTime.TryParse(_roastDate.Value, out var d) ? d : DateTime.Now,
				Notes = string.IsNullOrWhiteSpace(_notes.Value) ? null : _notes.Value,
				IsComplete = _isComplete.Value,
				IsActive = true
			});
		}
		Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
	}

	async void DeleteBag()
	{
		var page = Application.Current?.Windows.FirstOrDefault()?.Page;
		if (page == null) return;

		var message = _shotCount.Value > 0
			? $"This bag has {_shotCount.Value} shot(s) logged. Deleting it will hide it from all lists. Continue?"
			: "Are you sure you want to delete this bag?";

		var confirmed = await page.DisplayAlertAsync("Delete Bag", message, "Delete", "Cancel");
		if (!confirmed) return;

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		store.ArchiveBag(_bagId);
		await Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
	}

	void ReactivateBag()
	{
		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		store.ReactivateBag(_bagId);
		_isComplete.Value = false;
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadBag();

		if (_bagId <= 0)
		{
			var errorStack = new VerticalStackLayout
			{
				Padding = new Thickness(Theme.SpacingM),
				BackgroundColor = Theme.Background,
			};
			errorStack.Add(new MauiLabel { Text = "Bag not found", FontFamily = Theme.FontRegular, TextColor = Theme.TextSecondary });
			return new MauiViewHost(errorStack);
		}

		var stack = new VerticalStackLayout { Spacing = Theme.SpacingS, Padding = new Thickness(Theme.SpacingM) };

		stack.Add(FormHelpers.MakeSectionHeader("BAG DETAILS"));
		stack.Add(FormHelpers.MakeReadOnlyField("Bean", _beanName.Value));
		stack.Add(FormHelpers.MakeReadOnlyField("Roast Date", _roastDate.Value));
		stack.Add(FormHelpers.MakeFormEntryWithLimit("Notes", _notes.Value, "Bag notes", 500, v => _notes.Value = v));

		// Shot count card
		var shotCountStack = new HorizontalStackLayout();
		var shotInfoStack = new VerticalStackLayout { Spacing = 2 };
		shotInfoStack.Add(new MauiLabel { Text = "Shots Logged", FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary });
		shotInfoStack.Add(new MauiLabel { Text = $"{_shotCount.Value}", FontFamily = Theme.FontSemibold, FontSize = 24, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary });
		shotCountStack.Add(shotInfoStack);
		stack.Add(FormHelpers.MakeCard(shotCountStack));

		// Status toggle card
		stack.Add(FormHelpers.MakeToggleRow(
			_isComplete.Value ? "Status: Complete" : "Status: Active",
			_isComplete.Value,
			v => _isComplete.Value = v
		));

		if (!string.IsNullOrEmpty(_error.Value))
			stack.Add(new MauiLabel { Text = _error.Value, TextColor = Theme.Error, FontSize = 14 });

		stack.Add(FormHelpers.MakePrimaryButton("Save Changes", Save));

		if (_isComplete.Value)
			stack.Add(FormHelpers.MakeSecondaryButton("Reactivate Bag", ReactivateBag));

		stack.Add(FormHelpers.MakeDangerButton("Delete Bag", DeleteBag));

		stack.Add(FormHelpers.MakeSectionHeader("RATINGS"));
		stack.Add(RatingDisplayFactory.Create(_rating.Value));

		var scrollView = new MauiScrollView
		{
			Content = stack,
			BackgroundColor = Theme.Background,
		};

		return new MauiViewHost(scrollView);
	}
}
