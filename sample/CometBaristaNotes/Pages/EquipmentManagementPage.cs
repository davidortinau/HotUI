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

public class EquipmentManagementPage : Comet.View
{
	[State] readonly State<List<Equipment>> _equipment = new(new());
	[State] readonly State<bool> _isLoaded = new(false);

	IEquipmentService? GetEquipmentService() =>
		ViewHandler?.MauiContext?.Services.GetService<IEquipmentService>();

	void LoadEquipment()
	{
		var svc = GetEquipmentService();
		if (svc == null) return;
		_equipment.Value = svc.GetAllEquipment();
		_isLoaded.Value = true;
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadEquipment();

		var items = _equipment.Value;
		if (items.Count == 0)
			return new VStack(spacing: Theme.SpacingM)
			{
				FormHelpers.EmptyState("⚙️", "No Equipment Yet",
					"Add your coffee machines, grinders, and accessories"),
				FormHelpers.PrimaryButton("+ Add Equipment", () =>
				{
					Microsoft.Maui.Controls.Shell.Current.GoToAsync("equipment-detail?id=0");
				})
			}.Padding(Theme.SpacingL).Background(Theme.Background);

		return new ScrollView
		{
			new VStack(spacing: Theme.SpacingS)
			{
				FormHelpers.PrimaryButton("+ Add Equipment", () =>
				{
					Microsoft.Maui.Controls.Shell.Current.GoToAsync("equipment-detail?id=0");
				}),
				items.Select(eq =>
					FormHelpers.Card(
						new VStack(spacing: 4)
						{
							new Text(eq.Name).FontSize(16).FontWeight(FontWeight.Bold).Color(Theme.TextPrimary),
							new Text(eq.Type.ToString()).FontSize(14).Color(Theme.TextSecondary),
						}
					).OnTap(_ =>
					{
						Microsoft.Maui.Controls.Shell.Current.GoToAsync($"equipment-detail?id={eq.Id}");
					})
				).ToArray()
			}.Padding(Theme.SpacingM)
		}.Background(Theme.Background);
	}
}
