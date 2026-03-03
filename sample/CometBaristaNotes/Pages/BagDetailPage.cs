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

public class BagDetailPage : Comet.View
{
	readonly int _bagId;

	[State] readonly State<string> _beanName = new("");
	[State] readonly State<int> _beanId = new(0);
	[State] readonly State<string> _roastDate = new("");
	[State] readonly State<string> _notes = new("");
	[State] readonly State<bool> _isComplete = new(false);
	[State] readonly State<int> _shotCount = new(0);
	[State] readonly State<bool> _isLoaded = new(false);
	[State] readonly State<string> _error = new("");
	[State] readonly State<RatingAggregate> _rating = new(new());

	public BagDetailPage(int bagId = 0) { _bagId = bagId; }

	IBagService? GetBagService() =>
		ViewHandler?.MauiContext?.Services.GetService<IBagService>();
	IRatingService? GetRatingService() =>
		ViewHandler?.MauiContext?.Services.GetService<IRatingService>();

	void LoadBag()
	{
		if (_bagId <= 0) { _isLoaded.Value = true; return; }

		var svc = GetBagService();
		var ratingSvc = GetRatingService();
		if (svc == null) return;

		var bag = svc.GetBag(_bagId);
		if (bag == null) { _error.Value = "Bag not found"; _isLoaded.Value = true; return; }

		_beanName.Value = bag.BeanName ?? "";
		_beanId.Value = bag.BeanId;
		_roastDate.Value = bag.RoastDate.ToString("yyyy-MM-dd");
		_notes.Value = bag.Notes ?? "";
		_isComplete.Value = bag.IsComplete;
		_shotCount.Value = bag.ShotCount;

		if (ratingSvc != null) _rating.Value = ratingSvc.GetBagRating(_bagId);

		_isLoaded.Value = true;
	}

	void Save()
	{
		_error.Value = "";
		var svc = GetBagService();
		if (svc == null) return;

		if (_bagId > 0)
		{
			svc.UpdateBag(new Bag
			{
				Id = _bagId,
				BeanId = _beanId.Value,
				RoastDate = DateTime.TryParse(_roastDate.Value, out var d) ? d : DateTime.Now,
				Notes = string.IsNullOrWhiteSpace(_notes.Value) ? null : _notes.Value,
				IsComplete = _isComplete.Value,
				IsActive = true
			});
		}
		Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
	}

	void ToggleComplete()
	{
		if (_bagId <= 0) return;
		var svc = GetBagService();
		if (svc == null) return;

		if (!_isComplete.Value)
		{
			svc.MarkComplete(_bagId);
			_isComplete.Value = true;
		}
		else
		{
			// Reactivate by updating with IsComplete=false
			svc.UpdateBag(new Bag
			{
				Id = _bagId,
				BeanId = _beanId.Value,
				RoastDate = DateTime.TryParse(_roastDate.Value, out var d) ? d : DateTime.Now,
				Notes = string.IsNullOrWhiteSpace(_notes.Value) ? null : _notes.Value,
				IsComplete = false,
				IsActive = true
			});
			_isComplete.Value = false;
		}
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadBag();

		if (_bagId <= 0)
			return new VStack { new Text("Bag not found").Color(Colors.Gray) }.Padding(16);

		return new ScrollView
		{
			new VStack(spacing: 12)
			{
				FormHelpers.SectionHeader("Bag Details"),

				// Bean name (read-only)
				new VStack(spacing: 4)
				{
					new Text("Bean").FontSize(12).Color(Colors.Gray),
					new Text(_beanName.Value).FontSize(16).FontWeight(FontWeight.Semibold),
				}.Padding(12).Background(Color.FromArgb("#F5F5F5")).ClipShape(new RoundedRectangle(8)),

				// Roast date (read-only display)
				new VStack(spacing: 4)
				{
					new Text("Roast Date").FontSize(12).Color(Colors.Gray),
					new Text(_roastDate.Value).FontSize(16),
				}.Padding(12).Background(Color.FromArgb("#F5F5F5")).ClipShape(new RoundedRectangle(8)),

				FormHelpers.FormEntry("Notes", _notes, "Bag notes"),

				// Shot count
				new VStack(spacing: 4)
				{
					new Text("Shots Logged").FontSize(12).Color(Colors.Gray),
					new Text($"{_shotCount.Value}").FontSize(20).FontWeight(FontWeight.Bold),
				}.Padding(12).Background(Color.FromArgb("#F5F5F5")).ClipShape(new RoundedRectangle(8)),

				// Status toggle
				new HStack(spacing: 12)
				{
					new Text(_isComplete.Value ? "Status: Complete" : "Status: Active")
						.FontSize(14).FontWeight(FontWeight.Semibold),
					new Spacer(),
					new Toggle(_isComplete),
				}.Padding(12).Background(Color.FromArgb("#F5F5F5")).ClipShape(new RoundedRectangle(8)),

				!string.IsNullOrEmpty(_error.Value)
					? new Text(_error.Value).Color(Colors.Red).FontSize(13)
					: null,

				new Button("Save Changes", Save),

				// Rating section
				FormHelpers.SectionHeader("Ratings"),
				new RatingDisplay(_rating.Value),
			}.Padding(16)
		};
	}
}
