using CometAllTheLists.Pages;
using TabView = Comet.TabView;

namespace CometAllTheLists;

public class AllTheListsApp : CometApp
{
	public AllTheListsApp()
	{
		Body = Build;
	}

	Comet.View Build()
	{
		var tabs = TabView();
		tabs.Add(MakeTab(new ShoppingPage(), "Shopping", "cart.fill"));
		tabs.Add(MakeTab(new CollectionViewPage(), "Collections", "square.grid.2x2.fill"));
		tabs.Add(MakeTab(new InboxPage(), "Inbox", "tray.full.fill"));
		tabs.Add(MakeTab(new StreamingServicePage(), "Streaming", "play.rectangle.fill"));
		tabs.Add(MakeTab(new AddressBookPage(), "Contacts", "person.2.fill"));
		return tabs;
	}

	static NavigationView MakeTab(Comet.View page, string title, string sfSymbol)
	{
		var navigation = NavigationView(page.Title(title));
		navigation.TabText(title);
		navigation.TabIcon(sfSymbol);
		return navigation;
	}
}

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

#if DEBUG
		builder.UseCometSampleDebugHost<AllTheListsApp>();
#else
		builder.UseCometApp<AllTheListsApp>();
#endif
#if DEBUG
		builder.EnableSampleRuntimeDebugging();
#endif
		return builder.Build();
	}
}
