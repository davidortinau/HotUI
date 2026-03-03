using CometAllTheLists.Pages;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using MauiPage = Microsoft.Maui.Controls.ContentPage;
using MauiShell = Microsoft.Maui.Controls.Shell;

namespace CometAllTheLists;

public class AllTheListsShell : MauiShell
{
	public AllTheListsShell()
	{
		MauiShell.SetNavBarIsVisible(this, false);

		var tabBar = new TabBar();

		tabBar.Items.Add(new Microsoft.Maui.Controls.ShellContent
		{
			Title = "Shopping",
			ContentTemplate = new DataTemplate(() => MakeCometPage(new ShoppingPage(), "Shopping")),
			Route = "shopping"
		});

		tabBar.Items.Add(new Microsoft.Maui.Controls.ShellContent
		{
			Title = "Collections",
			ContentTemplate = new DataTemplate(() => MakeCometPage(new CollectionViewPage(), "Collections")),
			Route = "collections"
		});

		tabBar.Items.Add(new Microsoft.Maui.Controls.ShellContent
		{
			Title = "Inbox",
			ContentTemplate = new DataTemplate(() => MakeCometPage(new InboxPage(), "Inbox")),
			Route = "inbox"
		});

		tabBar.Items.Add(new Microsoft.Maui.Controls.ShellContent
		{
			Title = "Streaming",
			ContentTemplate = new DataTemplate(() => MakeCometPage(new StreamingServicePage(), "Streaming")),
			Route = "streaming"
		});

		tabBar.Items.Add(new Microsoft.Maui.Controls.ShellContent
		{
			Title = "Contacts",
			ContentTemplate = new DataTemplate(() => MakeCometPage(new AddressBookPage(), "Contacts")),
			Route = "contacts"
		});

		Items.Add(tabBar);
	}

	static MauiPage MakeCometPage(Comet.View cometView, string title)
	{
		var page = new MauiPage
		{
			Title = title,
			BackgroundColor = Colors.White,
		};

		var container = new Microsoft.Maui.Controls.ContentView();

		page.Content = container;

		page.Loaded += (s, e) =>
		{
			if (page.Handler?.MauiContext == null) return;
			EmbedCometView(container, cometView, page.Handler.MauiContext);
		};

		MauiShell.SetNavBarIsVisible(page, false);
		return page;
	}

	internal static void EmbedCometView(
		Microsoft.Maui.Controls.ContentView container,
		Comet.View cometView,
		IMauiContext mauiContext)
	{
		try
		{
			var renderView = cometView.GetView();
			IView viewToRender = (renderView != null && renderView != cometView) ? renderView : cometView;

			if (viewToRender is MauiViewHost mvh)
			{
				var hostedView = mvh.HostedView;
				if (hostedView is Microsoft.Maui.Controls.View mauiView)
				{
					container.Content = mauiView;
					return;
				}
			}

			container.Content = new CometHost(cometView);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[EmbedCometView] Failed: {ex.Message}");
		}
	}
}

public class ShellMauiApp : Application
{
	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AllTheListsShell());
	}
}

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<ShellMauiApp>();
		builder.UseCometHandlers();
		return builder.Build();
	}
}
