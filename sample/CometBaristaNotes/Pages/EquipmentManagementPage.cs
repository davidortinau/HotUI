using Comet;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;

namespace CometBaristaNotes.Pages;

public class EquipmentManagementPage : Comet.View
{
	[State] readonly State<List<Equipment>> _equipment = new(new());
	[State] readonly State<bool> _isLoaded = new(false);

	void LoadEquipment()
	{
		var store = InMemoryDataStore.Instance;
		if (store == null) return;
		_equipment.Value = store.GetAllEquipment();
		_isLoaded.Value = true;
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadEquipment();

		var items = _equipment.Value;

		if (items.Count == 0)
		{
			var emptyStack = new VerticalStackLayout
			{
				Spacing = Theme.SpacingM,
				Padding = new Thickness(Theme.SpacingL),
				BackgroundColor = Theme.Background,
				VerticalOptions = LayoutOptions.Fill,
			};
			emptyStack.Add(FormHelpers.MakeEmptyState(Icons.Build, "No Equipment Yet", "Add your coffee machines, grinders, and accessories"));
			emptyStack.Add(FormHelpers.MakePrimaryButton("+ Add Equipment", () =>
			{
				Microsoft.Maui.Controls.Shell.Current.GoToAsync("equipment-detail?id=0");
			}));
			return new MauiViewHost(emptyStack);
		}

		var stack = new VerticalStackLayout { Spacing = Theme.SpacingS, Padding = new Thickness(Theme.SpacingM) };

		stack.Add(FormHelpers.MakePrimaryButton("+ Add Equipment", () =>
		{
			Microsoft.Maui.Controls.Shell.Current.GoToAsync("equipment-detail?id=0");
		}));

		foreach (var eq in items)
		{
			stack.Add(FormHelpers.MakeListCard(
				eq.Name,
				eq.Type.ToString(),
				eq.Notes,
				() => Microsoft.Maui.Controls.Shell.Current.GoToAsync($"equipment-detail?id={eq.Id}")
			));
		}

		var scrollView = new MauiScrollView
		{
			Content = stack,
			BackgroundColor = Theme.Background,
		};

		return new MauiViewHost(scrollView);
	}
}
