using System;
using Microsoft.Maui.Graphics;

namespace Comet.Styles
{
	/// <summary>
	/// Filled button style — solid primary background with state-aware color shifts.
	/// References ColorTokens (provided by Holden's token layer) for theme-aware resolution.
	/// </summary>
	public class FilledButtonStyle : IControlStyle<Button, ButtonConfiguration>
	{
		public ViewModifier Resolve(ButtonConfiguration config)
		{
			if (!config.IsEnabled)
			{
				return new FilledButtonAppearance(
					background: Colors.Grey.WithAlpha(0.12f),
					foreground: Colors.Grey.WithAlpha(0.38f),
					opacity: 0.38);
			}

			Color bg, fg;
			double opacity = 1.0;

			if (config.IsPressed)
			{
				bg = ColorTokens.Primary.Resolve(config.TargetView).WithAlpha(0.88f);
				fg = ColorTokens.OnPrimary.Resolve(config.TargetView);
			}
			else if (config.IsHovered)
			{
				bg = ColorTokens.Primary.Resolve(config.TargetView).WithAlpha(0.92f);
				fg = ColorTokens.OnPrimary.Resolve(config.TargetView);
			}
			else
			{
				bg = ColorTokens.Primary.Resolve(config.TargetView);
				fg = ColorTokens.OnPrimary.Resolve(config.TargetView);
			}

			return new FilledButtonAppearance(bg, fg, opacity);
		}
	}

	sealed class FilledButtonAppearance : ViewModifier<Button>
	{
		readonly Color _background;
		readonly Color _foreground;
		readonly double _opacity;

		public FilledButtonAppearance(Color background, Color foreground, double opacity)
		{
			_background = background;
			_foreground = foreground;
			_opacity = opacity;
		}

		public override Button Apply(Button view) => view
			.Background(new SolidPaint(_background))
			.Color(_foreground)
			.Opacity(_opacity)
			.ClipShape(new RoundedRectangle(20))
			.Padding(new Thickness(24, 12));
	}

	/// <summary>
	/// Outlined button style — transparent background with primary-colored border.
	/// </summary>
	public class OutlinedButtonStyle : IControlStyle<Button, ButtonConfiguration>
	{
		public ViewModifier Resolve(ButtonConfiguration config)
		{
			if (!config.IsEnabled)
			{
				return new OutlinedButtonAppearance(
					foreground: Colors.Grey.WithAlpha(0.38f),
					borderColor: Colors.Grey.WithAlpha(0.12f),
					opacity: 0.38);
			}

			Color fg, border;
			double opacity = 1.0;

			if (config.IsPressed)
			{
				fg = ColorTokens.Primary.Resolve(config.TargetView);
				border = ColorTokens.Primary.Resolve(config.TargetView).WithAlpha(0.88f);
			}
			else if (config.IsHovered)
			{
				fg = ColorTokens.Primary.Resolve(config.TargetView);
				border = ColorTokens.Primary.Resolve(config.TargetView).WithAlpha(0.92f);
			}
			else
			{
				fg = ColorTokens.Primary.Resolve(config.TargetView);
				border = ColorTokens.Outline.Resolve(config.TargetView);
			}

			return new OutlinedButtonAppearance(fg, border, opacity);
		}
	}

	sealed class OutlinedButtonAppearance : ViewModifier<Button>
	{
		readonly Color _foreground;
		readonly Color _borderColor;
		readonly double _opacity;

		public OutlinedButtonAppearance(Color foreground, Color borderColor, double opacity)
		{
			_foreground = foreground;
			_borderColor = borderColor;
			_opacity = opacity;
		}

		public override Button Apply(Button view) => view
			.Background(Colors.Transparent)
			.Color(_foreground)
			.Opacity(_opacity)
			.RoundedBorder(radius: 20, color: _borderColor, strokeSize: 1)
			.Padding(new Thickness(24, 12));
	}

	/// <summary>
	/// Text button style — no background, no border, primary-colored text only.
	/// </summary>
	public class TextButtonStyle : IControlStyle<Button, ButtonConfiguration>
	{
		public ViewModifier Resolve(ButtonConfiguration config)
		{
			if (!config.IsEnabled)
			{
				return new TextButtonAppearance(
					foreground: Colors.Grey.WithAlpha(0.38f),
					opacity: 0.38);
			}

			Color fg;
			double opacity = 1.0;

			if (config.IsPressed)
			{
				fg = ColorTokens.Primary.Resolve(config.TargetView).WithAlpha(0.88f);
			}
			else if (config.IsHovered)
			{
				fg = ColorTokens.Primary.Resolve(config.TargetView).WithAlpha(0.92f);
			}
			else
			{
				fg = ColorTokens.Primary.Resolve(config.TargetView);
			}

			return new TextButtonAppearance(fg, opacity);
		}
	}

	sealed class TextButtonAppearance : ViewModifier<Button>
	{
		readonly Color _foreground;
		readonly double _opacity;

		public TextButtonAppearance(Color foreground, double opacity)
		{
			_foreground = foreground;
			_opacity = opacity;
		}

		public override Button Apply(Button view) => view
			.Background(Colors.Transparent)
			.Color(_foreground)
			.Opacity(_opacity)
			.Padding(new Thickness(12, 8));
	}

	/// <summary>
	/// Elevated button style — surface-colored background with elevation shadow.
	/// </summary>
	public class ElevatedButtonStyle : IControlStyle<Button, ButtonConfiguration>
	{
		public ViewModifier Resolve(ButtonConfiguration config)
		{
			if (!config.IsEnabled)
			{
				return new ElevatedButtonAppearance(
					background: Colors.Grey.WithAlpha(0.12f),
					foreground: Colors.Grey.WithAlpha(0.38f),
					shadowRadius: 0,
					opacity: 0.38);
			}

			Color bg, fg;
			float shadowRadius;
			double opacity = 1.0;

			if (config.IsPressed)
			{
				bg = ColorTokens.SurfaceContainer.Resolve(config.TargetView);
				fg = ColorTokens.Primary.Resolve(config.TargetView);
				shadowRadius = 1;
			}
			else if (config.IsHovered)
			{
				bg = ColorTokens.SurfaceContainer.Resolve(config.TargetView);
				fg = ColorTokens.Primary.Resolve(config.TargetView);
				shadowRadius = 4;
			}
			else
			{
				bg = ColorTokens.SurfaceContainer.Resolve(config.TargetView);
				fg = ColorTokens.Primary.Resolve(config.TargetView);
				shadowRadius = 2;
			}

			return new ElevatedButtonAppearance(bg, fg, shadowRadius, opacity);
		}
	}

	sealed class ElevatedButtonAppearance : ViewModifier<Button>
	{
		readonly Color _background;
		readonly Color _foreground;
		readonly float _shadowRadius;
		readonly double _opacity;

		public ElevatedButtonAppearance(Color background, Color foreground, float shadowRadius, double opacity)
		{
			_background = background;
			_foreground = foreground;
			_shadowRadius = shadowRadius;
			_opacity = opacity;
		}

		public override Button Apply(Button view) => view
			.Background(new SolidPaint(_background))
			.Color(_foreground)
			.Opacity(_opacity)
			.ClipShape(new RoundedRectangle(20))
			.Shadow(Colors.Black.WithAlpha(0.15f), radius: _shadowRadius, x: 0, y: _shadowRadius / 2)
			.Padding(new Thickness(24, 12));
	}

	/// <summary>
	/// Static instances of built-in button styles.
	/// </summary>
	public static class ButtonStyles
	{
		public static readonly IControlStyle<Button, ButtonConfiguration> Filled
			= new FilledButtonStyle();
		public static readonly IControlStyle<Button, ButtonConfiguration> Outlined
			= new OutlinedButtonStyle();
		public static readonly IControlStyle<Button, ButtonConfiguration> Text
			= new TextButtonStyle();
		public static readonly IControlStyle<Button, ButtonConfiguration> Elevated
			= new ElevatedButtonStyle();
	}
}
