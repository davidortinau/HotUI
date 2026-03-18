using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;

namespace CometBaristaNotes.Pages;

public class BeanManagementPageState
{
	public List<Bean> Beans { get; set; } = new();
	public bool IsLoaded { get; set; }
}

public class BeanManagementPage : Component<BeanManagementPageState>
{
	void LoadBeans()
	{
		var store = InMemoryDataStore.Instance;
		if (store == null) return;
		SetState(s =>
		{
			s.Beans = store.GetAllBeans();
			s.IsLoaded = true;
		});
	}

	public override View Render()
	{
		if (!State.IsLoaded)
			LoadBeans();

		var beans = State.Beans;

		if (beans.Count == 0)
		{
			return VStack(Theme.SpacingM,
				FormHelpers.MakeEmptyState(Icons.Coffee, "No Beans Yet", "Add your favorite coffee beans to track freshness and tasting notes"),
				FormHelpers.MakePrimaryButton("+ Add Bean", () => Navigation?.Navigate(new BeanDetailPage(0)))
			)
			.Padding(new Thickness(Theme.SpacingL))
			.Background(Theme.Background);
		}

		var stack = VStack(Theme.SpacingS,
			FormHelpers.MakePrimaryButton("+ Add Bean", () => Navigation?.Navigate(new BeanDetailPage(0)))
		);

		foreach (var bean in beans)
		{
			stack.Add(FormHelpers.MakeListCard(
				bean.Name,
				bean.Roaster,
				bean.Origin,
				() => Navigation?.Navigate(new BeanDetailPage(bean.Id))
			));
		}

		return ScrollView(stack.Padding(new Thickness(Theme.SpacingM)))
			.Background(Theme.Background);
	}
}
