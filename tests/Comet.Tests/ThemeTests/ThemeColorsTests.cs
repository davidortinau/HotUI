using System;
using Comet.Styles;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Comet.Tests
{
	public class ThemeColorsTests : TestBase
	{
		// ================================================================
		// Default theme color token values
		// ================================================================

		[Fact]
		public void DefaultPrimaryColorIsNotTransparent()
		{
			var theme = new Theme();
			Assert.NotEqual(Colors.Transparent, theme.PrimaryColor);
		}

		[Fact]
		public void DefaultSecondaryColorIsNotTransparent()
		{
			var theme = new Theme();
			Assert.NotEqual(Colors.Transparent, theme.SecondaryColor);
		}

		[Fact]
		public void DefaultBackgroundColorIsWhite()
		{
			var theme = new Theme();
			Assert.Equal(Colors.White, theme.BackgroundColor);
		}

		[Fact]
		public void DefaultTextColorIsBlack()
		{
			var theme = new Theme();
			Assert.Equal(Colors.Black, theme.TextColor);
		}

		[Fact]
		public void DefaultErrorColorIsNotNull()
		{
			var theme = new Theme();
			Assert.NotNull(theme.ErrorColor);
			Assert.NotEqual(Colors.Transparent, theme.ErrorColor);
		}

		[Fact]
		public void DefaultSurfaceColorIsDistinctFromBackground()
		{
			var theme = new Theme();
			Assert.NotEqual(theme.BackgroundColor, theme.SurfaceColor);
		}

		[Fact]
		public void DefaultSecondaryTextIsDistinctFromPrimary()
		{
			var theme = new Theme();
			Assert.NotEqual(theme.TextColor, theme.SecondaryTextColor);
		}

		// ================================================================
		// Custom color assignment
		// ================================================================

		[Fact]
		public void CanSetPrimaryColor()
		{
			var theme = new Theme();
			theme.PrimaryColor = Colors.Red;
			Assert.Equal(Colors.Red, theme.PrimaryColor);
		}

		[Fact]
		public void CanSetSecondaryColor()
		{
			var theme = new Theme();
			theme.SecondaryColor = Colors.Blue;
			Assert.Equal(Colors.Blue, theme.SecondaryColor);
		}

		[Fact]
		public void CanSetBackgroundColor()
		{
			var theme = new Theme();
			theme.BackgroundColor = Colors.DarkSlateGray;
			Assert.Equal(Colors.DarkSlateGray, theme.BackgroundColor);
		}

		[Fact]
		public void CanSetTextColor()
		{
			var theme = new Theme();
			theme.TextColor = Colors.LimeGreen;
			Assert.Equal(Colors.LimeGreen, theme.TextColor);
		}

		[Fact]
		public void CanSetErrorColor()
		{
			var theme = new Theme();
			theme.ErrorColor = Colors.OrangeRed;
			Assert.Equal(Colors.OrangeRed, theme.ErrorColor);
		}

		[Fact]
		public void CanSetSurfaceColor()
		{
			var theme = new Theme();
			theme.SurfaceColor = Colors.AliceBlue;
			Assert.Equal(Colors.AliceBlue, theme.SurfaceColor);
		}

		[Fact]
		public void CanSetSecondaryTextColor()
		{
			var theme = new Theme();
			theme.SecondaryTextColor = Colors.DarkGray;
			Assert.Equal(Colors.DarkGray, theme.SecondaryTextColor);
		}

		// ================================================================
		// Light preset specific colors
		// ================================================================

		[Fact]
		public void LightPresetBackgroundIsWhite()
		{
			var light = Theme.Light;
			Assert.Equal(Colors.White, light.BackgroundColor);
		}

		[Fact]
		public void LightPresetTextIsBlack()
		{
			var light = Theme.Light;
			Assert.Equal(Colors.Black, light.TextColor);
		}

		[Fact]
		public void LightPresetSurfaceIsLightGray()
		{
			var light = Theme.Light;
			// Surface should be a very light gray
			Assert.True(light.SurfaceColor.Red > 0.9f);
			Assert.True(light.SurfaceColor.Green > 0.9f);
			Assert.True(light.SurfaceColor.Blue > 0.9f);
		}

		[Fact]
		public void LightPresetSecondaryTextIsMediumGray()
		{
			var light = Theme.Light;
			// Secondary text should be grayish (mid-range)
			Assert.True(light.SecondaryTextColor.Red > 0.2f);
			Assert.True(light.SecondaryTextColor.Red < 0.8f);
		}

		// ================================================================
		// Dark preset specific colors
		// ================================================================

		[Fact]
		public void DarkPresetBackgroundIsDark()
		{
			var dark = Theme.Dark;
			Assert.True(dark.BackgroundColor.Red < 0.15f);
			Assert.True(dark.BackgroundColor.Green < 0.15f);
			Assert.True(dark.BackgroundColor.Blue < 0.15f);
		}

		[Fact]
		public void DarkPresetTextIsWhite()
		{
			var dark = Theme.Dark;
			Assert.Equal(Colors.White, dark.TextColor);
		}

		[Fact]
		public void DarkPresetSurfaceIsDarkerThanLight()
		{
			var dark = Theme.Dark;
			Assert.True(dark.SurfaceColor.Red < 0.3f);
		}

		[Fact]
		public void DarkPresetSecondaryTextIsLighter()
		{
			var dark = Theme.Dark;
			Assert.True(dark.SecondaryTextColor.Red > 0.5f);
		}

		// ================================================================
		// Light vs Dark color contrast
		// ================================================================

		[Fact]
		public void LightAndDarkHaveDifferentBackgrounds()
		{
			Assert.NotEqual(Theme.Light.BackgroundColor, Theme.Dark.BackgroundColor);
		}

		[Fact]
		public void LightAndDarkHaveDifferentTextColors()
		{
			Assert.NotEqual(Theme.Light.TextColor, Theme.Dark.TextColor);
		}

		[Fact]
		public void LightAndDarkHaveDifferentSurfaces()
		{
			Assert.NotEqual(Theme.Light.SurfaceColor, Theme.Dark.SurfaceColor);
		}

		[Fact]
		public void LightAndDarkHaveDifferentSecondaryText()
		{
			Assert.NotEqual(Theme.Light.SecondaryTextColor, Theme.Dark.SecondaryTextColor);
		}

		// ================================================================
		// Theme colors flow through environment
		// ================================================================

		[Fact]
		public void ThemeColorSetsBackgroundEnvironment()
		{
			var original = Theme.Current;
			try
			{
				var custom = new Theme { PrimaryColor = Colors.Indigo };
				Theme.Current = custom;

				var view = new Text("Hello").ThemeColor(t => t.PrimaryColor);
				var bg = view.GetEnvironment<Paint>(nameof(IView.Background));
				Assert.NotNull(bg);
				var solid = Assert.IsType<SolidPaint>(bg);
				Assert.Equal(Colors.Indigo, solid.Color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeTextColorSetsTextColorEnvironment()
		{
			var original = Theme.Current;
			try
			{
				var custom = new Theme { SecondaryTextColor = Colors.Purple };
				Theme.Current = custom;

				var view = new Text("Hello").ThemeTextColor(t => t.SecondaryTextColor);
				var color = view.GetEnvironment<Color>(nameof(ITextStyle.TextColor));
				Assert.Equal(Colors.Purple, color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		// ================================================================
		// Phase 3.1 complete — ThemeColors class, semantic tokens
		// ================================================================

		[Fact]
		public void ThemeColorsHasSemanticTokens()
		{
			var colors = ThemeColors.LightScheme;
			Assert.NotNull(colors.Primary);
			Assert.NotNull(colors.Secondary);
			Assert.NotNull(colors.Background);
			Assert.NotNull(colors.Surface);
			Assert.NotNull(colors.OnPrimary);
			Assert.NotNull(colors.OnSecondary);
			Assert.NotNull(colors.OnBackground);
			Assert.NotNull(colors.OnSurface);
			Assert.NotNull(colors.Error);
			Assert.NotNull(colors.OnError);
		}

		[Fact]
		public void ThemeColorsLightAndDarkVariants()
		{
			var lightColors = ThemeColors.LightScheme;
			var darkColors = ThemeColors.DarkScheme;
			Assert.NotEqual(lightColors.Background, darkColors.Background);
			Assert.NotEqual(lightColors.Surface, darkColors.Surface);
		}

		[Fact]
		public void ThemeColorsPropagateThroughEnvironment()
		{
			ResetComet();
			var original = Theme.Current;
			try
			{
				var colors = new ThemeColors { Primary = Colors.Teal };
				var theme = new Theme { ColorScheme = colors };
				Theme.Current = theme;

				var resolved = View.GetGlobalEnvironment<Color>(EnvironmentKeys.ThemeColor.Primary);
				Assert.Equal(Colors.Teal, resolved);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeColorsCustomPalette()
		{
			var colors = new ThemeColors
			{
				Primary = Colors.DeepPink,
				Secondary = Colors.Cyan,
				Background = Colors.White,
				Surface = Colors.LightGray
			};
			Assert.Equal(Colors.DeepPink, colors.Primary);
			Assert.Equal(Colors.Cyan, colors.Secondary);
			Assert.Equal(Colors.White, colors.Background);
			Assert.Equal(Colors.LightGray, colors.Surface);
		}
	}
}
