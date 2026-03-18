using System;
using Comet.Styles;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Comet.Tests
{
	public class ThemeBaseTests : TestBase
	{
		// ================================================================
		// Theme.Current — singleton management
		// ================================================================

		[Fact]
		public void ThemeCurrentIsNotNull()
		{
			Assert.NotNull(Theme.Current);
		}

		[Fact]
		public void ThemeCurrentReturnsSameInstance()
		{
			var a = Theme.Current;
			var b = Theme.Current;
			Assert.Same(a, b);
		}

		[Fact]
		public void ThemeCurrentCanBeReplaced()
		{
			var original = Theme.Current;
			try
			{
				var custom = new Theme { PrimaryColor = Colors.Fuchsia };
				Theme.Current = custom;

				Assert.Same(custom, Theme.Current);
				Assert.Equal(Colors.Fuchsia, Theme.Current.PrimaryColor);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeCurrentRestoredAfterSwap()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Dark;
				Assert.Equal(AppTheme.Dark, Theme.Current.CurrentTheme);
			}
			finally
			{
				Theme.Current = original;
			}
			Assert.Same(original, Theme.Current);
		}

		// ================================================================
		// Theme default constructor
		// ================================================================

		[Fact]
		public void NewThemeDefaultsToSystemTheme()
		{
			var theme = new Theme();
			Assert.Equal(AppTheme.System, theme.CurrentTheme);
		}

		[Fact]
		public void NewThemeHasNonNullColorTokens()
		{
			var theme = new Theme();
			Assert.NotNull(theme.PrimaryColor);
			Assert.NotNull(theme.SecondaryColor);
			Assert.NotNull(theme.BackgroundColor);
			Assert.NotNull(theme.SurfaceColor);
			Assert.NotNull(theme.TextColor);
			Assert.NotNull(theme.SecondaryTextColor);
			Assert.NotNull(theme.ErrorColor);
		}

		// ================================================================
		// Light / Dark presets
		// ================================================================

		[Fact]
		public void LightPresetHasCorrectTheme()
		{
			var light = Theme.Light;
			Assert.Equal(AppTheme.Light, light.CurrentTheme);
		}

		[Fact]
		public void DarkPresetHasCorrectTheme()
		{
			var dark = Theme.Dark;
			Assert.Equal(AppTheme.Dark, dark.CurrentTheme);
		}

		[Fact]
		public void LightPresetHasWhiteBackground()
		{
			var light = Theme.Light;
			Assert.Equal(Colors.White, light.BackgroundColor);
			Assert.Equal(Colors.Black, light.TextColor);
		}

		[Fact]
		public void DarkPresetHasNonWhiteBackground()
		{
			var dark = Theme.Dark;
			Assert.NotEqual(Colors.White, dark.BackgroundColor);
			Assert.Equal(Colors.White, dark.TextColor);
		}

		[Fact]
		public void LightAndDarkPresetsAreDifferent()
		{
			var light = Theme.Light;
			var dark = Theme.Dark;

			Assert.NotEqual(light.BackgroundColor, dark.BackgroundColor);
			Assert.NotEqual(light.TextColor, dark.TextColor);
			Assert.NotEqual(light.SurfaceColor, dark.SurfaceColor);
		}

		[Fact]
		public void LightPresetReturnsNewInstanceEachTime()
		{
			var a = Theme.Light;
			var b = Theme.Light;
			Assert.NotSame(a, b);
		}

		[Fact]
		public void DarkPresetReturnsNewInstanceEachTime()
		{
			var a = Theme.Dark;
			var b = Theme.Dark;
			Assert.NotSame(a, b);
		}

		// ================================================================
		// ThemeColor / ThemeTextColor extension methods
		// ================================================================

		[Fact]
		public void ThemeColorAppliesPrimaryColor()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Light;
				var view = new Text("Hello").ThemeColor(t => t.PrimaryColor);

				var bg = view.GetEnvironment<Paint>(nameof(IView.Background));
				Assert.NotNull(bg);
				var solid = Assert.IsType<SolidPaint>(bg);
				Assert.Equal(Theme.Current.PrimaryColor, solid.Color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeColorUsesCurrentThemeAtCallTime()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Dark;
				var view = new Text("Hello").ThemeColor(t => t.TextColor);

				var bg = view.GetEnvironment<Paint>(nameof(IView.Background));
				var solid = Assert.IsType<SolidPaint>(bg);
				Assert.Equal(Colors.White, solid.Color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeTextColorAppliesTextColor()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Light;
				var view = new Text("Hello").ThemeTextColor(t => t.TextColor);

				var color = view.GetEnvironment<Color>(nameof(ITextStyle.TextColor));
				Assert.Equal(Colors.Black, color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeTextColorSwitchesToDark()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Dark;
				var view = new Text("Hello").ThemeTextColor(t => t.TextColor);

				var color = view.GetEnvironment<Color>(nameof(ITextStyle.TextColor));
				Assert.Equal(Colors.White, color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeColorRespectsCustomTheme()
		{
			var original = Theme.Current;
			try
			{
				var custom = new Theme { PrimaryColor = Colors.HotPink };
				Theme.Current = custom;

				var view = new Text("Hello").ThemeColor(t => t.PrimaryColor);
				var bg = view.GetEnvironment<Paint>(nameof(IView.Background));
				var solid = Assert.IsType<SolidPaint>(bg);
				Assert.Equal(Colors.HotPink, solid.Color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		// ================================================================
		// AppThemeValue — light/dark value resolution
		// ================================================================

		[Fact]
		public void AppThemeValueGetReturnsLightInLightMode()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Light;
				var result = AppThemeValue.Get(light: "LightValue", dark: "DarkValue");
				Assert.Equal("LightValue", result);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void AppThemeValueGetReturnsDarkInDarkMode()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Dark;
				var result = AppThemeValue.Get(light: "LightValue", dark: "DarkValue");
				Assert.Equal("DarkValue", result);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void AppThemeValueGetWorksWithColors()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Light;
				var color = AppThemeValue.Get(light: Colors.White, dark: Colors.Black);
				Assert.Equal(Colors.White, color);

				Theme.Current = Theme.Dark;
				color = AppThemeValue.Get(light: Colors.White, dark: Colors.Black);
				Assert.Equal(Colors.Black, color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void AppThemeValueNotifyThemeChangedFiresEvent()
		{
			var fired = false;
			void handler() => fired = true;

			AppThemeValue.ThemeChanged += handler;
			try
			{
				AppThemeValue.NotifyThemeChanged();
				Assert.True(fired);
			}
			finally
			{
				AppThemeValue.ThemeChanged -= handler;
			}
		}

		// ================================================================
		// Phase 3.1 complete — Theme change notification, IThemeable
		// ================================================================

		[Fact]
		public void ThemeCurrentChangedRaisesEvent()
		{
			var original = Theme.Current;
			try
			{
				Theme fired = null;
				void handler(Theme t) => fired = t;
				Theme.ThemeChanged += handler;
				try
				{
					Theme.Current = Theme.Dark;
					Assert.NotNull(fired);
					Assert.Same(Theme.Current, fired);
				}
				finally
				{
					Theme.ThemeChanged -= handler;
				}
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeableViewReceivesThemeOnChange()
		{
			var original = Theme.Current;
			try
			{
				var view = new ThemeableTestView();
				view.SetViewHandlerToGeneric();
				Theme.Current = Theme.Dark;
				Assert.True(view.ThemeWasApplied);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void CustomThemeSubclassCanOverrideDefaults()
		{
			var custom = new BrandTheme();
			Assert.Equal(BrandTheme.BrandPrimary, custom.PrimaryColor);
		}
	}

	// Test helpers for IThemeable
	public class ThemeableTestView : View, IThemeable
	{
		public bool ThemeWasApplied { get; private set; }

		public void ApplyTheme(Theme theme)
		{
			ThemeWasApplied = true;
		}

		[Body]
		View body() => new Text("Themeable");
	}

	public class BrandTheme : Theme
	{
		public static readonly Color BrandPrimary = Microsoft.Maui.Graphics.Colors.DeepPink;

		public BrandTheme()
		{
			PrimaryColor = BrandPrimary;
		}
	}
}
