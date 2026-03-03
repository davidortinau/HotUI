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

public class BagFormPage : Comet.View
{
	readonly int _beanId;

	[State] readonly State<string> _roastDate = new(DateTime.Now.ToString("yyyy-MM-dd"));
	[State] readonly State<string> _notes = new("");
	[State] readonly State<string> _error = new("");
	[State] readonly State<string> _beanName = new("");
	[State] readonly State<bool> _isLoaded = new(false);

	public BagFormPage(int beanId = 0) { _beanId = beanId; }

	IBagService? GetBagService() =>
		ViewHandler?.MauiContext?.Services.GetService<IBagService>();
	IBeanService? GetBeanService() =>
		ViewHandler?.MauiContext?.Services.GetService<IBeanService>();

	void LoadBeanName()
	{
		var svc = GetBeanService();
		if (svc != null)
		{
			var bean = svc.GetBean(_beanId);
			_beanName.Value = bean?.Name ?? "Unknown Bean";
		}
		_isLoaded.Value = true;
	}

	void Save()
	{
		if (!DateTime.TryParse(_roastDate.Value, out var roastDate))
		{
			_error.Value = "Please enter a valid date (yyyy-MM-dd)";
			return;
		}

		if (roastDate.Date > DateTime.Now.Date)
		{
			_error.Value = "Roast date cannot be in the future";
			return;
		}

		_error.Value = "";

		var svc = GetBagService();
		if (svc == null) return;

		svc.CreateBag(new Bag
		{
			BeanId = _beanId,
			RoastDate = roastDate,
			Notes = string.IsNullOrWhiteSpace(_notes.Value) ? null : _notes.Value,
		});

		Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
	}

	[Body]
	Comet.View body()
	{
		if (!_isLoaded.Value)
			LoadBeanName();

		return new ScrollView
		{
			new VStack(spacing: 12)
			{
				FormHelpers.SectionHeader($"Add Bag for {_beanName.Value}"),

				// Bean (read-only)
				new VStack(spacing: 4)
				{
					new Text("Bean").FontSize(12).Color(Colors.Gray),
					new Text(_beanName.Value).FontSize(16).FontWeight(FontWeight.Semibold),
				}.Padding(12).Background(Color.FromArgb("#F5F5F5")).ClipShape(new RoundedRectangle(8)),

				FormHelpers.FormEntry("Roast Date", _roastDate, "yyyy-MM-dd"),
				FormHelpers.FormEntry("Notes (optional)", _notes, "e.g., From Trader Joe's, Gift from friend"),

				!string.IsNullOrEmpty(_error.Value)
					? new Text(_error.Value).Color(Colors.Red).FontSize(13)
					: null,

				new Button("Add Bag", Save),
			}.Padding(16)
		};
	}
}
