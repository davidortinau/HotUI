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
using MauiGrid = Microsoft.Maui.Controls.Grid;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;
using MauiFontAttributes = Microsoft.Maui.Controls.FontAttributes;

namespace CometBaristaNotes.Pages;

public class BeanDetailPage : Comet.View
{
	readonly int _beanId;

	[State] readonly State<string> _name = new("");
	[State] readonly State<string> _roaster = new("");
	[State] readonly State<string> _origin = new("");
	[State] readonly State<string> _notes = new("");
	[State] readonly State<bool> _isLoaded = new(false);
	[State] readonly State<string> _error = new("");
	[State] readonly State<List<Bag>> _bags = new(new());
	[State] readonly State<RatingAggregate> _rating = new(new());

	public BeanDetailPage(int beanId = 0) { _beanId = beanId; }

	void LoadBean()
	{
		if (_beanId <= 0) { _isLoaded.Value = true; return; }

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		var bean = store.GetBean(_beanId);
		if (bean == null) { _error.Value = "Bean not found"; _isLoaded.Value = true; return; }

		_name.Value = bean.Name;
		_roaster.Value = bean.Roaster ?? "";
		_origin.Value = bean.Origin ?? "";
		_notes.Value = bean.Notes ?? "";
		_bags.Value = store.GetBagsForBean(_beanId);
		_rating.Value = store.GetBeanRating(_beanId);

		_isLoaded.Value = true;
	}

	void Save()
	{
		if (string.IsNullOrWhiteSpace(_name.Value))
		{
			_error.Value = "Bean name is required";
			return;
		}
		_error.Value = "";

		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		if (_beanId > 0)
		{
			store.UpdateBean(new Bean
			{
				Id = _beanId,
				Name = _name.Value,
				Roaster = string.IsNullOrWhiteSpace(_roaster.Value) ? null : _roaster.Value,
				Origin = string.IsNullOrWhiteSpace(_origin.Value) ? null : _origin.Value,
				Notes = string.IsNullOrWhiteSpace(_notes.Value) ? null : _notes.Value,
				IsActive = true
			});
		}
		else
		{
			store.CreateBean(new Bean
			{
				Name = _name.Value,
				Roaster = string.IsNullOrWhiteSpace(_roaster.Value) ? null : _roaster.Value,
				Origin = string.IsNullOrWhiteSpace(_origin.Value) ? null : _origin.Value,
				Notes = string.IsNullOrWhiteSpace(_notes.Value) ? null : _notes.Value,
			});
		}

		Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadBean();

		var isEdit = _beanId > 0;

		var stack = new VerticalStackLayout { Spacing = Theme.SpacingS, Padding = new Thickness(Theme.SpacingM) };

		stack.Add(FormHelpers.MakeSectionHeader(isEdit ? "EDIT BEAN" : "NEW BEAN"));
		stack.Add(FormHelpers.MakeFormEntry("Name *", _name.Value, "Bean name", v => _name.Value = v));
		stack.Add(FormHelpers.MakeFormEntry("Roaster", _roaster.Value, "Roaster name", v => _roaster.Value = v));
		stack.Add(FormHelpers.MakeFormEntry("Origin", _origin.Value, "Country or region", v => _origin.Value = v));
		stack.Add(FormHelpers.MakeFormEntry("Notes", _notes.Value, "Tasting notes, processing, etc.", v => _notes.Value = v));

		if (!string.IsNullOrEmpty(_error.Value))
			stack.Add(new MauiLabel { Text = _error.Value, TextColor = Theme.Error, FontFamily = Theme.FontRegular, FontSize = 14 });

		stack.Add(FormHelpers.MakePrimaryButton(isEdit ? "Save Changes" : "Create Bean", Save));

		if (isEdit)
		{
			stack.Add(FormHelpers.MakeSectionHeader("RATINGS"));
			stack.Add(RatingDisplayFactory.Create(_rating.Value));

			stack.Add(FormHelpers.MakeSectionHeader("BAGS"));
			if (_bags.Value.Count == 0)
				stack.Add(new MauiLabel { Text = "No bags added yet", FontFamily = Theme.FontRegular, FontSize = 14, TextColor = Theme.TextSecondary });

			stack.Add(FormHelpers.MakeSecondaryButton("+ Add Bag", () =>
			{
				Microsoft.Maui.Controls.Shell.Current.GoToAsync($"bag-detail?id=0&beanId={_beanId}");
			}));

			foreach (var bag in _bags.Value)
			{
				stack.Add(BuildBagCard(bag));
			}
		}

		var scrollView = new MauiScrollView
		{
			Content = stack,
			BackgroundColor = Theme.Background,
		};

		return new MauiViewHost(scrollView);
	}

	Microsoft.Maui.Controls.View BuildBagCard(Bag bag)
	{
		var grid = new MauiGrid
		{
			ColumnDefinitions =
			{
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto),
			},
		};

		var infoStack = new VerticalStackLayout { Spacing = 4 };
		infoStack.Add(new MauiLabel { Text = $"Roasted {bag.RoastDate:MMM d, yyyy}", FontFamily = Theme.FontSemibold, FontSize = 14, FontAttributes = MauiFontAttributes.Bold, TextColor = Theme.TextPrimary });

		if (bag.Notes != null)
			infoStack.Add(new MauiLabel { Text = bag.Notes, FontFamily = Theme.FontRegular, FontSize = 12, TextColor = Theme.TextSecondary });

		var statsStack = new HorizontalStackLayout { Spacing = 12 };
		statsStack.Add(new MauiLabel { Text = $"{bag.ShotCount} shots", FontFamily = Theme.FontRegular, FontSize = 12, TextColor = Theme.TextMuted });
		statsStack.Add(bag.AverageRating.HasValue
			? new MauiLabel { Text = $"{Icons.SentimentVerySatisfied} {bag.AverageRating.Value:F1}", FontFamily = Icons.FontFamily, FontSize = 12, TextColor = Theme.StarFilled }
			: new MauiLabel { Text = "No ratings", FontFamily = Theme.FontRegular, FontSize = 12, TextColor = Theme.TextMuted });
		statsStack.Add(new MauiLabel { Text = bag.IsComplete ? "Complete" : "Active", FontFamily = Theme.FontRegular, FontSize = 12, TextColor = bag.IsComplete ? Theme.Success : Theme.Primary });
		infoStack.Add(statsStack);

		grid.Add(infoStack, 0, 0);
		grid.Add(new MauiLabel { Text = Icons.ChevronRight, FontFamily = Icons.FontFamily, FontSize = 20, TextColor = Theme.TextMuted, VerticalTextAlignment = TextAlignment.Center }, 1, 0);

		var border = new MauiBorder
		{
			Content = grid,
			BackgroundColor = Theme.CardBackground,
			Stroke = new SolidColorBrush(Theme.CardStroke),
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = Theme.RadiusCard },
			Padding = new Thickness(Theme.SpacingM),
		};

		var tap = new TapGestureRecognizer();
		tap.Tapped += (s, e) => Microsoft.Maui.Controls.Shell.Current.GoToAsync($"bag-detail?id={bag.Id}");
		border.GestureRecognizers.Add(tap);

		return border;
	}
}
