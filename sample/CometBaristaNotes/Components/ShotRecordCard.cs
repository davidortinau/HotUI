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
						.FontSize(16).FontWeight(FontWeight.Semibold),
					new Text(_shot.DrinkType)
						.FontSize(12).Color(Colors.Gray),
				},
				new Spacer(),
				new VStack
				{
					_shot.Rating.HasValue
						? new Text($"★ {_shot.Rating.Value}")
							.FontSize(16).FontWeight(FontWeight.Bold)
							.Color(_shot.Rating.Value >= 4 ? Colors.Orange : Colors.Gray)
						: new Text("—").FontSize(16).Color(Colors.LightGray)
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
				? new Text(_shot.TastingNotes).FontSize(12).Color(Colors.DimGray)
				: null,
			new HStack(spacing: 8)
			{
				new Text(_shot.Timestamp.ToString("MMM d, h:mm tt"))
					.FontSize(11).Color(Colors.Gray),
				new Spacer(),
				_shot.MachineName != null ? new Text(_shot.MachineName).FontSize(11).Color(Colors.Gray) : null
			}
		}
		.Padding(16)
		.Background(Colors.White)
		.ClipShape(new RoundedRectangle(12));

	static Comet.View InfoPill(string label, string value) =>
		new VStack(spacing: 2)
		{
			new Text(label).FontSize(10).Color(Colors.Gray),
			new Text(value).FontSize(13).FontWeight(FontWeight.Semibold)
		};
}
