using System;
using Comet.Styles;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Comet.Tests
{
	public class ThemeTests : TestBase
	{
		[Fact]
		public void ThemeCurrentReturnsSingleton()
		{
			var theme1 = Theme.Current;
			var theme2 = Theme.Current;

			Assert.NotNull(theme1);
			Assert.Same(theme1, theme2);
		}

		[Fact]
		public void ThemeCurrentCanBeSet()
		{
			var original = Theme.Current;
			try
			{
				var custom = new Theme();
				Theme.Current = custom;
				Assert.Same(custom, Theme.Current);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeLightPreset()
		{
			var light = Theme.Light;

			Assert.NotNull(light);
			Assert.Equal(AppTheme.Light, light.CurrentTheme);
			Assert.Equal(Colors.White, light.BackgroundColor);
			Assert.Equal(Colors.Black, light.TextColor);
		}

		[Fact]
		public void ThemeDarkPreset()
		{
			var dark = Theme.Dark;

			Assert.NotNull(dark);
			Assert.Equal(AppTheme.Dark, dark.CurrentTheme);
			Assert.NotEqual(Colors.White, dark.BackgroundColor);
			Assert.Equal(Colors.White, dark.TextColor);
		}

		[Fact]
		public void ThemeDefaultProperties()
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

		[Fact]
		public void ThemeDefaultThemeIsSystem()
		{
			var theme = new Theme();
			Assert.Equal(AppTheme.System, theme.CurrentTheme);
		}

		[Fact]
		public void ThemeLightAndDarkAreDifferent()
		{
			var light = Theme.Light;
			var dark = Theme.Dark;

			Assert.NotEqual(light.BackgroundColor, dark.BackgroundColor);
			Assert.NotEqual(light.TextColor, dark.TextColor);
			Assert.NotEqual(light.SurfaceColor, dark.SurfaceColor);
		}

		[Fact]
		public void ThemeColorExtension()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Light;
				var view = new Text("Hello").ThemeColor(t => t.PrimaryColor);

				var bg = view.GetEnvironment<Paint>(nameof(Microsoft.Maui.IView.Background));
				Assert.NotNull(bg);
				var solidPaint = Assert.IsType<SolidPaint>(bg);
				Assert.Equal(Theme.Current.PrimaryColor, solidPaint.Color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemeColorUsesCurrentTheme()
		{
			var original = Theme.Current;
			try
			{
				Theme.Current = Theme.Dark;
				var view = new Text("Hello").ThemeColor(t => t.TextColor);

				var bg = view.GetEnvironment<Paint>(nameof(Microsoft.Maui.IView.Background));
				Assert.NotNull(bg);
				var solidPaint = Assert.IsType<SolidPaint>(bg);
				Assert.Equal(Colors.White, solidPaint.Color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		[Fact]
		public void ThemePropertiesCanBeCustomized()
		{
			var theme = new Theme();
			theme.PrimaryColor = Colors.Red;
			theme.SecondaryColor = Colors.Blue;

			Assert.Equal(Colors.Red, theme.PrimaryColor);
			Assert.Equal(Colors.Blue, theme.SecondaryColor);
		}

		[Fact]
		public void ThemeLightCreatesNewInstance()
		{
			var light1 = Theme.Light;
			var light2 = Theme.Light;

			Assert.NotSame(light1, light2);
		}

		[Fact]
		public void ThemeDarkCreatesNewInstance()
		{
			var dark1 = Theme.Dark;
			var dark2 = Theme.Dark;

			Assert.NotSame(dark1, dark2);
		}
	}
}
