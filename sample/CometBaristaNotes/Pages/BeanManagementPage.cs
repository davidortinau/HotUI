using Comet;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiScrollView = Microsoft.Maui.Controls.ScrollView;

namespace CometBaristaNotes.Pages;

public class BeanManagementPage : Comet.View
{
	[State] readonly State<List<Bean>> _beans = new(new());
	[State] readonly State<bool> _isLoaded = new(false);

	void LoadBeans()
	{
		var store = InMemoryDataStore.Instance;
		if (store == null) return;
		_beans.Value = store.GetAllBeans();
		_isLoaded.Value = true;
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadBeans();

		var beans = _beans.Value;

		if (beans.Count == 0)
		{
			var emptyStack = new VerticalStackLayout
			{
				Spacing = Theme.SpacingM,
				Padding = new Thickness(Theme.SpacingL),
				BackgroundColor = Theme.Background,
				VerticalOptions = LayoutOptions.Fill,
			};
			emptyStack.Add(FormHelpers.MakeEmptyState("☕", "No Beans Yet", "Add your favorite coffee beans to track freshness and tasting notes"));
			emptyStack.Add(FormHelpers.MakePrimaryButton("+ Add Bean", () =>
			{
				Microsoft.Maui.Controls.Shell.Current.GoToAsync("bean-detail?id=0");
			}));
			return new MauiViewHost(emptyStack);
		}

		var stack = new VerticalStackLayout { Spacing = Theme.SpacingS, Padding = new Thickness(Theme.SpacingM) };

		stack.Add(FormHelpers.MakePrimaryButton("+ Add Bean", () =>
		{
			Microsoft.Maui.Controls.Shell.Current.GoToAsync("bean-detail?id=0");
		}));

		foreach (var bean in beans)
		{
			stack.Add(FormHelpers.MakeListCard(
				bean.Name,
				bean.Roaster,
				bean.Origin,
				() => Microsoft.Maui.Controls.Shell.Current.GoToAsync($"bean-detail?id={bean.Id}")
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
