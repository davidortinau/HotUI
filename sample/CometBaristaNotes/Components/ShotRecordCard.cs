using Comet;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;

namespace CometBaristaNotes.Components;

public class ShotRecordCard : Comet.View
{
	readonly ShotRecord _shot;
	readonly Action? _onTap;

	public ShotRecordCard(ShotRecord shot, Action? onTap = null)
	{
		_shot = shot;
		_onTap = onTap;
	}

	[Body]
	Comet.View body() =>
		new VStack(spacing: 8)
		{
			new HStack
			{
				new VStack(spacing: 2)
				{
					new Text(_shot.BagDisplayName ?? "Unknown Bean")
						.FontSize(16).FontWeight(FontWeight.Bold).Color(Theme.TextPrimary),
					new Text(_shot.DrinkType)
						.FontSize(14).Color(Theme.TextSecondary),
				},
				new Spacer(),
				new VStack
				{
					_shot.Rating.HasValue
						? new Text($"★ {_shot.Rating.Value}")
							.FontSize(16).FontWeight(FontWeight.Bold)
							.Color(_shot.Rating.Value >= 4 ? Theme.Warning : Theme.TextMuted)
						: new Text("—").FontSize(16).Color(Theme.TextMuted)
				}
			},
			new HStack(spacing: 16)
			{
				InfoPill("Dose", $"{_shot.DoseIn}g"),
				InfoPill("Time", _shot.ActualTime.HasValue ? $"{_shot.ActualTime:F0}s" : "—"),
				InfoPill("Out", _shot.ActualOutput.HasValue ? $"{_shot.ActualOutput:F0}g" : "—"),
				InfoPill("Grind", _shot.GrindSetting),
			},
			_shot.TastingNotes != null
				? new Text(_shot.TastingNotes).FontSize(12).Color(Theme.TextSecondary)
				: null,
			new HStack(spacing: 8)
			{
				new Text(_shot.Timestamp.ToString("MMM d, h:mm tt"))
					.FontSize(12).Color(Theme.TextMuted),
				new Spacer(),
				_shot.MachineName != null ? new Text(_shot.MachineName).FontSize(12).Color(Theme.TextMuted) : null
			}
		}
		.Padding(12)
		.Margin(new Thickness(0, Theme.SpacingXS))
		.Background(Theme.Surface)
		.RoundedBorder(radius: Theme.RadiusCard, color: Theme.Outline, strokeSize: 1);

	static Comet.View InfoPill(string label, string value) =>
		new VStack(spacing: 2)
		{
			new Text(label).FontSize(12).Color(Theme.TextMuted),
			new Text(value).FontSize(14).FontWeight(FontWeight.Bold).Color(Theme.TextPrimary)
		};
}
