using Comet;
using CometBaristaNotes.Components;
using CometBaristaNotes.Pages;
using Microsoft.Maui.Graphics;
using TabView = Comet.TabView;

namespace CometBaristaNotes;

public class BaristaApp : CometApp
{
	public BaristaApp()
	{
		Body = CreateRootView;
	}

	public static Comet.View CreateRootView()
	{
		var tabs = TabView();
		tabs.Add(MakeTab(new CoffeeDashboardPage(), "Coffee Lab", "cup.and.saucer.fill"));
		tabs.Add(MakeTab(new ActivityFeedPage(), "Activity", "chart.line.uptrend.xyaxis"));
		tabs.Add(MakeTab(new SettingsPage(), "Settings", "gearshape.fill"));
		return tabs;
	}

	static NavigationView MakeTab(Comet.View page, string title, string sfSymbol)
	{
		var nav = NavigationView(page.Title(title));
		nav.SetEnvironment("NavigationBackgroundColor", (Binding<Color>)Theme.Primary);
		nav.SetEnvironment("NavigationTextColor", (Binding<Color>)Theme.Surface);
		nav.SetAutomationId($"barista-{title.Replace(" ", string.Empty).ToLowerInvariant()}-tab-root");
		nav.TabText(title);
		nav.TabIcon(sfSymbol);
		return nav;
	}
}
