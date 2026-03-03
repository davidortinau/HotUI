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

	IBeanService? GetBeanService() =>
		ViewHandler?.MauiContext?.Services.GetService<IBeanService>();
	IBagService? GetBagService() =>
		ViewHandler?.MauiContext?.Services.GetService<IBagService>();
	IRatingService? GetRatingService() =>
		ViewHandler?.MauiContext?.Services.GetService<IRatingService>();

	void LoadBean()
	{
		if (_beanId <= 0) { _isLoaded.Value = true; return; }

		var svc = GetBeanService();
		var bagSvc = GetBagService();
		var ratingSvc = GetRatingService();
		if (svc == null) return;

		var bean = svc.GetBean(_beanId);
		if (bean == null) { _error.Value = "Bean not found"; _isLoaded.Value = true; return; }

		_name.Value = bean.Name;
		_roaster.Value = bean.Roaster ?? "";
		_origin.Value = bean.Origin ?? "";
		_notes.Value = bean.Notes ?? "";

		if (bagSvc != null) _bags.Value = bagSvc.GetBagsForBean(_beanId);
		if (ratingSvc != null) _rating.Value = ratingSvc.GetBeanRating(_beanId);

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

		var svc = GetBeanService();
		if (svc == null) return;

		if (_beanId > 0)
		{
			svc.UpdateBean(new Bean
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
			svc.CreateBean(new Bean
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

		return new ScrollView
		{
			new VStack(spacing: Theme.SpacingS)
			{
				FormHelpers.SectionHeader(isEdit ? "EDIT BEAN" : "NEW BEAN"),

				FormHelpers.FormEntry("Name *", _name, "Bean name"),
				FormHelpers.FormEntry("Roaster", _roaster, "Roaster name"),
				FormHelpers.FormEntry("Origin", _origin, "Country or region"),
				FormHelpers.FormEntry("Notes", _notes, "Tasting notes, processing, etc."),

				!string.IsNullOrEmpty(_error.Value)
					? new Text(_error.Value).Color(Theme.Error).FontSize(14)
					: null,

				FormHelpers.PrimaryButton(isEdit ? "Save Changes" : "Create Bean", Save),

				isEdit ? FormHelpers.SectionHeader("RATINGS") : null,
				isEdit ? new RatingDisplay(_rating.Value) : null,

				isEdit ? FormHelpers.SectionHeader("BAGS") : null,
				isEdit && _bags.Value.Count == 0
					? new Text("No bags added yet").FontSize(14).Color(Theme.TextSecondary)
					: null,
				isEdit ? FormHelpers.PrimaryButton("+ Add Bag", () =>
				{
					Microsoft.Maui.Controls.Shell.Current.GoToAsync($"bag-detail?id=0&beanId={_beanId}");
				}) : null,
				isEdit ? RenderBags() : null,
			}.Padding(Theme.SpacingM)
		}.Background(Theme.Background);
	}

	Comet.View? RenderBags()
	{
		var bags = _bags.Value;
		if (bags.Count == 0) return null;

		return new VStack(spacing: Theme.SpacingS)
		{
			bags.Select(bag =>
				FormHelpers.Card(
					new VStack(spacing: 4)
					{
						new Text($"Roasted {bag.RoastDate:MMM d, yyyy}")
							.FontSize(14).FontWeight(FontWeight.Semibold).Color(Theme.TextPrimary),
						bag.Notes != null
							? new Text(bag.Notes).FontSize(12).Color(Theme.TextSecondary)
							: null,
						new HStack(spacing: 16)
						{
							new Text($"{bag.ShotCount} shots").FontSize(12).Color(Theme.TextMuted),
							bag.AverageRating.HasValue
								? new Text($"★ {bag.AverageRating.Value:F1}").FontSize(12).Color(Theme.Warning)
								: new Text("No ratings").FontSize(12).Color(Theme.TextMuted),
							bag.IsComplete
								? new Text("Complete").FontSize(12).Color(Theme.Success)
								: new Text("Active").FontSize(12).Color(Theme.Primary),
						}
					}
				).OnTap(_ =>
				{
					Microsoft.Maui.Controls.Shell.Current.GoToAsync($"bag-detail?id={bag.Id}");
				})
			).ToArray()
		};
	}
}
