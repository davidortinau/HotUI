using System;
using Microsoft.Maui.Graphics;

namespace Comet.Styles
{
	public enum AppTheme
	{
		Light,
		Dark,
		System
	}

	public class Theme
	{
		private static Theme _current;
		public static Theme Current
		{
			get => _current ??= new Theme();
			set => _current = value;
		}

		public AppTheme CurrentTheme { get; set; } = AppTheme.System;

		public Color PrimaryColor { get; set; } = new Color(0.32f, 0.17f, 0.83f); // #512BD4
		public Color SecondaryColor { get; set; } = new Color(0.87f, 0.32f, 0.15f);
		public Color BackgroundColor { get; set; } = Colors.White;
		public Color SurfaceColor { get; set; } = new Color(0.96f, 0.96f, 0.96f);
		public Color TextColor { get; set; } = Colors.Black;
		public Color SecondaryTextColor { get; set; } = new Color(0.4f, 0.4f, 0.4f);
		public Color ErrorColor { get; set; } = new Color(0.7f, 0.11f, 0.11f);

		public static Theme Light => new Theme
		{
			CurrentTheme = AppTheme.Light,
			BackgroundColor = Colors.White,
			SurfaceColor = new Color(0.96f, 0.96f, 0.96f),
			TextColor = Colors.Black,
			SecondaryTextColor = new Color(0.4f, 0.4f, 0.4f),
		};

		public static Theme Dark => new Theme
		{
			CurrentTheme = AppTheme.Dark,
			BackgroundColor = new Color(0.07f, 0.07f, 0.07f),
			SurfaceColor = new Color(0.15f, 0.15f, 0.15f),
			TextColor = Colors.White,
			SecondaryTextColor = new Color(0.7f, 0.7f, 0.7f),
		};
	}
}
