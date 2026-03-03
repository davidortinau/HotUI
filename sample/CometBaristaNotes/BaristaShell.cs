using CometBaristaNotes.Pages;
using Microsoft.Maui.Controls;
using MauiPage = Microsoft.Maui.Controls.ContentPage;
using MauiShell = Microsoft.Maui.Controls.Shell;

namespace CometBaristaNotes;

public class BaristaShell : MauiShell
{
	public BaristaShell()
	{
		var tabBar = new TabBar();

		tabBar.Items.Add(new Microsoft.Maui.Controls.ShellContent
		{
			Title = "New Shot",
			ContentTemplate = new DataTemplate(() => MakeCometPage(new ShotLoggingPage(), "New Shot")),
			Route = "newshot"
		});

		tabBar.Items.Add(new Microsoft.Maui.Controls.ShellContent
		{
			Title = "Activity",
			ContentTemplate = new DataTemplate(() => MakeCometPage(new ActivityFeedPage(), "Activity")),
			Route = "activity"
		});

		tabBar.Items.Add(new Microsoft.Maui.Controls.ShellContent
		{
			Title = "Settings",
			ContentTemplate = new DataTemplate(() => MakeCometPage(new SettingsPage(), "Settings")),
			Route = "settings"
		});

		Items.Add(tabBar);

		// Register detail routes
		Routing.RegisterRoute("bean-detail", typeof(BeanDetailShellPage));
		Routing.RegisterRoute("bag-detail", typeof(BagDetailShellPage));
		Routing.RegisterRoute("equipment-detail", typeof(EquipmentDetailShellPage));
		Routing.RegisterRoute("beans", typeof(BeanManagementShellPage));
		Routing.RegisterRoute("equipment", typeof(EquipmentManagementShellPage));
		Routing.RegisterRoute("profiles", typeof(UserProfileManagementShellPage));
		Routing.RegisterRoute("profile-form", typeof(ProfileFormShellPage));
	}

	static MauiPage MakeCometPage(Comet.View cometView, string title)
	{
		var page = new MauiPage
		{
			Title = title,
			BackgroundColor = Color.FromArgb("#F5F5F5"),
		};

		var container = new Microsoft.Maui.Controls.ContentView
		{
			BackgroundColor = Color.FromArgb("#F5F5F5"),
		};

		page.Content = container;

		page.Loaded += (s, e) =>
		{
			if (page.Handler?.MauiContext == null) return;
			EmbedCometView(container, cometView, page.Handler.MauiContext);
		};

		MauiShell.SetNavBarIsVisible(page, true);
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

			if (viewToRender is Comet.MauiViewHost mvh)
			{
				var hostedView = mvh.HostedView;
				if (hostedView is Microsoft.Maui.Controls.View mauiView)
				{
					container.Content = mauiView;
					return;
				}
			}

			container.Content = new Comet.CometHost(cometView);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[EmbedCometView] Failed: {ex.Message}");
		}
	}
}

// Shell page wrappers for detail routes
[QueryProperty(nameof(BeanId), "id")]
public class BeanDetailShellPage : MauiPage
{
	string _beanId = "";
	Microsoft.Maui.Controls.ContentView _container = new();
	Comet.View? _cometView;
	bool _embedded;

	public BeanDetailShellPage() { Content = _container; }

	public string BeanId
	{
		get => _beanId;
		set { _beanId = value; LoadPage(); }
	}

	void LoadPage()
	{
		if (!int.TryParse(_beanId, out var id)) return;
		Title = "Bean Detail";
		_cometView?.Dispose();
		_cometView = new BeanDetailPage(id);
		_embedded = false;
		TryEmbed();
	}

	void TryEmbed()
	{
		if (_embedded || _cometView == null || Handler?.MauiContext == null) return;
		BaristaShell.EmbedCometView(_container, _cometView, Handler.MauiContext);
		_embedded = true;
	}

	protected override void OnHandlerChanged() { base.OnHandlerChanged(); TryEmbed(); }
}

[QueryProperty(nameof(BagId), "id")]
public class BagDetailShellPage : MauiPage
{
	string _bagId = "";
	Microsoft.Maui.Controls.ContentView _container = new();
	Comet.View? _cometView;
	bool _embedded;

	public BagDetailShellPage() { Content = _container; }

	public string BagId
	{
		get => _bagId;
		set { _bagId = value; LoadPage(); }
	}

	void LoadPage()
	{
		if (!int.TryParse(_bagId, out var id)) return;
		Title = "Bag Detail";
		_cometView?.Dispose();
		_cometView = new BagDetailPage(id);
		_embedded = false;
		TryEmbed();
	}

	void TryEmbed()
	{
		if (_embedded || _cometView == null || Handler?.MauiContext == null) return;
		BaristaShell.EmbedCometView(_container, _cometView, Handler.MauiContext);
		_embedded = true;
	}

	protected override void OnHandlerChanged() { base.OnHandlerChanged(); TryEmbed(); }
}

[QueryProperty(nameof(EquipmentId), "id")]
public class EquipmentDetailShellPage : MauiPage
{
	string _equipmentId = "";
	Microsoft.Maui.Controls.ContentView _container = new();
	Comet.View? _cometView;
	bool _embedded;

	public EquipmentDetailShellPage() { Content = _container; }

	public string EquipmentId
	{
		get => _equipmentId;
		set { _equipmentId = value; LoadPage(); }
	}

	void LoadPage()
	{
		if (!int.TryParse(_equipmentId, out var id)) return;
		Title = "Equipment Detail";
		_cometView?.Dispose();
		_cometView = new EquipmentDetailPage(id);
		_embedded = false;
		TryEmbed();
	}

	void TryEmbed()
	{
		if (_embedded || _cometView == null || Handler?.MauiContext == null) return;
		BaristaShell.EmbedCometView(_container, _cometView, Handler.MauiContext);
		_embedded = true;
	}

	protected override void OnHandlerChanged() { base.OnHandlerChanged(); TryEmbed(); }
}

public class BeanManagementShellPage : MauiPage
{
	Microsoft.Maui.Controls.ContentView _container = new();
	Comet.View _cometView = new BeanManagementPage();
	bool _embedded;

	public BeanManagementShellPage() { Title = "Beans"; Content = _container; }

	void TryEmbed()
	{
		if (_embedded || Handler?.MauiContext == null) return;
		BaristaShell.EmbedCometView(_container, _cometView, Handler.MauiContext);
		_embedded = true;
	}

	protected override void OnHandlerChanged() { base.OnHandlerChanged(); TryEmbed(); }
}

public class EquipmentManagementShellPage : MauiPage
{
	Microsoft.Maui.Controls.ContentView _container = new();
	Comet.View _cometView = new EquipmentManagementPage();
	bool _embedded;

	public EquipmentManagementShellPage() { Title = "Equipment"; Content = _container; }

	void TryEmbed()
	{
		if (_embedded || Handler?.MauiContext == null) return;
		BaristaShell.EmbedCometView(_container, _cometView, Handler.MauiContext);
		_embedded = true;
	}

	protected override void OnHandlerChanged() { base.OnHandlerChanged(); TryEmbed(); }
}

public class UserProfileManagementShellPage : MauiPage
{
	Microsoft.Maui.Controls.ContentView _container = new();
	Comet.View _cometView = new UserProfileManagementPage();
	bool _embedded;

	public UserProfileManagementShellPage() { Title = "Profiles"; Content = _container; }

	void TryEmbed()
	{
		if (_embedded || Handler?.MauiContext == null) return;
		BaristaShell.EmbedCometView(_container, _cometView, Handler.MauiContext);
		_embedded = true;
	}

	protected override void OnHandlerChanged() { base.OnHandlerChanged(); TryEmbed(); }
}

[QueryProperty(nameof(ProfileId), "id")]
public class ProfileFormShellPage : MauiPage
{
	string _profileId = "";
	Microsoft.Maui.Controls.ContentView _container = new();
	Comet.View? _cometView;
	bool _embedded;

	public ProfileFormShellPage() { Content = _container; }

	public string ProfileId
	{
		get => _profileId;
		set { _profileId = value; LoadPage(); }
	}

	void LoadPage()
	{
		int.TryParse(_profileId, out var id);
		Title = id > 0 ? "Edit Profile" : "New Profile";
		_cometView?.Dispose();
		_cometView = new ProfileFormPage(id);
		_embedded = false;
		TryEmbed();
	}

	void TryEmbed()
	{
		if (_embedded || _cometView == null || Handler?.MauiContext == null) return;
		BaristaShell.EmbedCometView(_container, _cometView, Handler.MauiContext);
		_embedded = true;
	}

	protected override void OnHandlerChanged() { base.OnHandlerChanged(); TryEmbed(); }
}
