using System;
using Comet.Styles;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Comet.Tests
{
	public class ThemeIntegrationTests : TestBase
	{
		// ================================================================
		// ThemeExtensions — fluent theme color methods
		// ================================================================

		[Fact]
		public void ThemeBackgroundWithKeySetsBackground()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Light;
				var button = new Button("Click")
					.ThemeBackground(EnvironmentKeys.ThemeColor.Primary);

				var bg = button.GetEnvironment<Paint>(nameof(IView.Background));
				Assert.NotNull(bg);
				var solid = Assert.IsType<SolidPaint>(bg);
				Assert.Equal(Theme.Current.GetColor(EnvironmentKeys.ThemeColor.Primary), solid.Color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeBackgroundShorthandUsesBackgroundToken()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Light;
				var text = new Text("Hello").ThemeBackground();

				var bg = text.GetEnvironment<Paint>(nameof(IView.Background));
				Assert.NotNull(bg);
				var solid = Assert.IsType<SolidPaint>(bg);
				Assert.Equal(Theme.Current.GetColor(EnvironmentKeys.ThemeColor.Background), solid.Color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeForegroundWithKeySetsColor()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Light;
				var text = new Text("Hello")
					.ThemeForeground(EnvironmentKeys.ThemeColor.OnSurface);

				var color = text.GetEnvironment<Color>(EnvironmentKeys.Colors.Color);
				Assert.NotNull(color);
				Assert.Equal(Theme.Current.GetColor(EnvironmentKeys.ThemeColor.OnSurface), color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeForegroundShorthandUsesOnSurface()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Dark;
				var text = new Text("Hello").ThemeForeground();

				var color = text.GetEnvironment<Color>(EnvironmentKeys.Colors.Color);
				Assert.NotNull(color);
				Assert.Equal(Theme.Current.GetColor(EnvironmentKeys.ThemeColor.OnSurface), color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeColorsPairSetsBothBackgroundAndForeground()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Light;
				var button = new Button("Click")
					.ThemeColors(EnvironmentKeys.ThemeColor.Primary, EnvironmentKeys.ThemeColor.OnPrimary);

				var bg = button.GetEnvironment<Paint>(nameof(IView.Background));
				var solid = Assert.IsType<SolidPaint>(bg);
				Assert.Equal(Theme.Current.GetColor(EnvironmentKeys.ThemeColor.Primary), solid.Color);

				var fg = button.GetEnvironment<Color>(EnvironmentKeys.Colors.Color);
				Assert.Equal(Theme.Current.GetColor(EnvironmentKeys.ThemeColor.OnPrimary), fg);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ExplicitBackgroundOverridesThemeBackground()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Light;
				var button = new Button("Click")
					.ThemeBackground(EnvironmentKeys.ThemeColor.Primary)
					.Background(new SolidPaint(Colors.Red));

				var bg = button.GetEnvironment<Paint>(nameof(IView.Background));
				var solid = Assert.IsType<SolidPaint>(bg);
				Assert.Equal(Colors.Red, solid.Color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ExplicitColorOverridesThemeForeground()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Light;
				var text = new Text("Hello")
					.ThemeForeground(EnvironmentKeys.ThemeColor.OnSurface)
					.Color(Colors.Orange);

				var color = text.GetEnvironment<Color>(EnvironmentKeys.Colors.Color);
				Assert.Equal(Colors.Orange, color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeExtensionsWorkWithDarkTheme()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Dark;
				var button = new Button("Click")
					.ThemeBackground(EnvironmentKeys.ThemeColor.Primary);

				var bg = button.GetEnvironment<Paint>(nameof(IView.Background));
				var solid = Assert.IsType<SolidPaint>(bg);
				// Dark primary is different from light
				Assert.Equal(Theme.Current.GetColor(EnvironmentKeys.ThemeColor.Primary), solid.Color);
				Assert.NotEqual(Theme.Light.GetColor(EnvironmentKeys.ThemeColor.Primary), solid.Color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		// ================================================================
		// DefaultThemeStyles — automatic control styling
		// ================================================================

		[Fact]
		public void DefaultThemeStylesRegisterButtonStyle()
		{
			var theme = Theme.Light;
			DefaultThemeStyles.Register(theme);

			var style = theme.GetControlStyle<Button>();
			Assert.NotNull(style);
			Assert.True(style.HasProperty(EnvironmentKeys.Colors.Background));
			Assert.True(style.HasProperty(EnvironmentKeys.Colors.Color));
		}

		[Fact]
		public void DefaultThemeStylesRegisterTextStyle()
		{
			var theme = Theme.Light;
			DefaultThemeStyles.Register(theme);

			var style = theme.GetControlStyle<Text>();
			Assert.NotNull(style);
			Assert.True(style.HasProperty(EnvironmentKeys.Colors.Color));
		}

		[Fact]
		public void DefaultThemeStylesRegisterTextFieldStyle()
		{
			var theme = Theme.Light;
			DefaultThemeStyles.Register(theme);

			var style = theme.GetControlStyle<TextField>();
			Assert.NotNull(style);
			Assert.True(style.HasProperty(EnvironmentKeys.Colors.Background));
			Assert.True(style.HasProperty(EnvironmentKeys.Colors.Color));
		}

		[Fact]
		public void DefaultThemeStylesRegisterToggleStyle()
		{
			var theme = Theme.Light;
			DefaultThemeStyles.Register(theme);

			var style = theme.GetControlStyle<Toggle>();
			Assert.NotNull(style);
			Assert.True(style.HasProperty(EnvironmentKeys.Switch.OnColor));
		}

		[Fact]
		public void DefaultThemeStylesRegisterSliderStyle()
		{
			var theme = Theme.Light;
			DefaultThemeStyles.Register(theme);

			var style = theme.GetControlStyle<Slider>();
			Assert.NotNull(style);
			Assert.True(style.HasProperty(EnvironmentKeys.Slider.ProgressColor));
		}

		[Fact]
		public void DefaultStylesDoNotOverrideCustomStyles()
		{
			var theme = Theme.Light;

			// Register a custom button style first
			var customStyle = new ControlStyle<Button>()
				.Set(EnvironmentKeys.Colors.Background, new SolidPaint(Colors.HotPink));
			theme.SetControlStyle(customStyle);

			// Now register defaults — should NOT overwrite
			DefaultThemeStyles.Register(theme);

			var style = theme.GetControlStyle<Button>();
			Assert.Same(customStyle, style);
			var bg = style.Get<SolidPaint>(EnvironmentKeys.Colors.Background);
			Assert.Equal(Colors.HotPink, bg.Color);
		}

		[Fact]
		public void ThemeApplyRegistersDefaultStyles()
		{
			var theme = Theme.Light;
			theme.Apply();

			Assert.NotNull(theme.GetControlStyle<Button>());
			Assert.NotNull(theme.GetControlStyle<Text>());
			Assert.NotNull(theme.GetControlStyle<TextField>());
			Assert.NotNull(theme.GetControlStyle<Toggle>());
			Assert.NotNull(theme.GetControlStyle<Slider>());
		}

		[Fact]
		public void ButtonDefaultStyleUsesPrimaryColors()
		{
			var theme = Theme.Light;
			DefaultThemeStyles.Register(theme);

			var style = theme.GetControlStyle<Button>();
			var bg = style.Get<SolidPaint>(EnvironmentKeys.Colors.Background);
			var fg = style.Get<Color>(EnvironmentKeys.Colors.Color);

			Assert.Equal(theme.GetColor(EnvironmentKeys.ThemeColor.Primary), bg.Color);
			Assert.Equal(theme.GetColor(EnvironmentKeys.ThemeColor.OnPrimary), fg);
		}

		[Fact]
		public void TextDefaultStyleUsesOnSurface()
		{
			var theme = Theme.Light;
			DefaultThemeStyles.Register(theme);

			var style = theme.GetControlStyle<Text>();
			var fg = style.Get<Color>(EnvironmentKeys.Colors.Color);

			Assert.Equal(theme.GetColor(EnvironmentKeys.ThemeColor.OnSurface), fg);
		}

		[Fact]
		public void DarkThemeDefaultStylesUseDarkColors()
		{
			var lightTheme = Theme.Light;
			var darkTheme = Theme.Dark;
			DefaultThemeStyles.Register(lightTheme);
			DefaultThemeStyles.Register(darkTheme);

			var lightBg = lightTheme.GetControlStyle<Button>()
				.Get<SolidPaint>(EnvironmentKeys.Colors.Background);
			var darkBg = darkTheme.GetControlStyle<Button>()
				.Get<SolidPaint>(EnvironmentKeys.Colors.Background);

			Assert.NotEqual(lightBg.Color, darkBg.Color);
		}

		// ================================================================
		// Full integration — Theme.Current with default styles
		// ================================================================

		[Fact]
		public void SettingThemeCurrentAppliesDefaultStyles()
		{
			var original = Theme.Current;
			try
			{
				var theme = Theme.Light;
				Theme.Current = theme;

				// Theme.Current setter calls Apply(), which calls DefaultThemeStyles.Register
				Assert.NotNull(theme.GetControlStyle<Button>());
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ControlStyleApplyPushesToGlobalEnvironment()
		{
			ResetComet();
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Light;

				// Verify global env has typed key for Button background
				var typedKey = ContextualObject.GetTypedKey(typeof(Button), EnvironmentKeys.Colors.Background);
				var bgPaint = View.GetGlobalEnvironment<object>(typedKey);
				Assert.NotNull(bgPaint);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeSwitchUpdatesGlobalEnvironment()
		{
			ResetComet();
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Light;
				var lightPrimary = View.GetGlobalEnvironment<Color>(EnvironmentKeys.ThemeColor.Primary);

				Theme.Current = Theme.Dark;
				var darkPrimary = View.GetGlobalEnvironment<Color>(EnvironmentKeys.ThemeColor.Primary);

				Assert.NotNull(lightPrimary);
				Assert.NotNull(darkPrimary);
				Assert.NotEqual(lightPrimary, darkPrimary);
			}
			finally
			{
				Theme.Current = original;
			}
		}
	}
}
