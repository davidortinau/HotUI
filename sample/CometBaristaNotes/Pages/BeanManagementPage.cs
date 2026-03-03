using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Extensions.DependencyInjection;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;
using Button = Comet.Button;
using ScrollView = Comet.ScrollView;

namespace CometBaristaNotes.Pages;

public class BeanManagementPage : Comet.View
{
	[State] readonly State<List<Bean>> _beans = new(new());
	[State] readonly State<bool> _isLoaded = new(false);

	IBeanService? GetBeanService() =>
		ViewHandler?.MauiContext?.Services.GetService<IBeanService>();

	void LoadBeans()
	{
		var svc = GetBeanService();
		if (svc == null) return;
		_beans.Value = svc.GetAllBeans();
		_isLoaded.Value = true;
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadBeans();

		var beans = _beans.Value;
		if (beans.Count == 0)
			return new VStack(spacing: Theme.SpacingM)
			{
				FormHelpers.EmptyState("☕", "No Beans Yet",
					"Add your favorite coffee beans to track freshness and tasting notes"),
				FormHelpers.PrimaryButton("+ Add Bean", () =>
				{
					Microsoft.Maui.Controls.Shell.Current.GoToAsync("bean-detail?id=0");
				})
			}.Padding(Theme.SpacingL).Background(Theme.Background);

		return new ScrollView
		{
			new VStack(spacing: Theme.SpacingS)
			{
				FormHelpers.PrimaryButton("+ Add Bean", () =>
				{
					Microsoft.Maui.Controls.Shell.Current.GoToAsync("bean-detail?id=0");
				}),
				beans.Select(bean =>
					FormHelpers.Card(
						new VStack(spacing: 4)
						{
							new Text(bean.Name).FontSize(16).FontWeight(FontWeight.Bold).Color(Theme.TextPrimary),
							bean.Roaster != null
								? new Text(bean.Roaster).FontSize(14).Color(Theme.TextSecondary)
								: null,
							bean.Origin != null
								? new Text(bean.Origin).FontSize(12).Color(Theme.TextMuted)
								: null,
						}
					).OnTap(_ =>
					{
						Microsoft.Maui.Controls.Shell.Current.GoToAsync($"bean-detail?id={bean.Id}");
					})
				).ToArray()
			}.Padding(Theme.SpacingM)
		}.Background(Theme.Background);
	}
}
