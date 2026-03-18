using System;
using Comet.Styles;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Comet.Tests
{
	/// <summary>
	/// Tests for Theme record, token sets, and theme composition (§5).
	/// Written TDD-style against the Style/Theme Spec.
	/// </summary>
	public class NewThemeTests : TestBase
	{
		// ================================================================
		// Helpers
		// ================================================================

		static Theme CreateTestTheme(string name = "Test", Color primary = null)
		{
			return new Theme
			{
				Name = name,
				Colors = new ColorTokenSet
				{
					Primary = primary ?? Colors.Purple,
					OnPrimary = Colors.White,
					Surface = Colors.White,
					OnSurface = Colors.Black,
					Background = Colors.White,
					OnBackground = Colors.Black,
					Error = Colors.Red,
					OnError = Colors.White,
				},
				Typography = new TypographyTokenSet
				{
					BodyMedium = new FontSpec(14, FontWeight.Regular),
					TitleLarge = new FontSpec(22, FontWeight.Bold),
				},
				Spacing = new SpacingTokenSet
				{
					None = 0,
					ExtraSmall = 4,
					Small = 8,
					Medium = 16,
					Large = 24,
					ExtraLarge = 32,
				},
				Shapes = new ShapeTokenSet
				{
					None = 0,
					ExtraSmall = 4,
					Small = 8,
					Medium = 12,
					Large = 16,
					ExtraLarge = 28,
					Full = 9999,
				},
			};
		}

		// ================================================================
		// Theme creation with all required token sets (§5.2)
		// ================================================================

		[Fact]
		public void Theme_Creation_HasRequiredProperties()
		{
			var theme = CreateTestTheme("MyTheme");

			Assert.Equal("MyTheme", theme.Name);
			Assert.NotNull(theme.Colors);
			Assert.NotNull(theme.Typography);
			Assert.NotNull(theme.Spacing);
			Assert.NotNull(theme.Shapes);
		}

		[Fact]
		public void Theme_Colors_StoresCorrectValues()
		{
			var theme = CreateTestTheme(primary: Colors.Teal);

			Assert.Equal(Colors.Teal, theme.Colors.Primary);
			Assert.Equal(Colors.White, theme.Colors.OnPrimary);
		}

		[Fact]
		public void Theme_Typography_StoresCorrectValues()
		{
			var theme = CreateTestTheme();

			Assert.Equal(14.0, theme.Typography.BodyMedium.Size);
			Assert.Equal(FontWeight.Regular, theme.Typography.BodyMedium.Weight);
			Assert.Equal(22.0, theme.Typography.TitleLarge.Size);
			Assert.Equal(FontWeight.Bold, theme.Typography.TitleLarge.Weight);
		}

		[Fact]
		public void Theme_Spacing_StoresCorrectValues()
		{
			var theme = CreateTestTheme();

			Assert.Equal(0.0, theme.Spacing.None);
			Assert.Equal(4.0, theme.Spacing.ExtraSmall);
			Assert.Equal(8.0, theme.Spacing.Small);
			Assert.Equal(16.0, theme.Spacing.Medium);
			Assert.Equal(24.0, theme.Spacing.Large);
			Assert.Equal(32.0, theme.Spacing.ExtraLarge);
		}

		[Fact]
		public void Theme_Shapes_StoresCorrectValues()
		{
			var theme = CreateTestTheme();

			Assert.Equal(0.0, theme.Shapes.None);
			Assert.Equal(12.0, theme.Shapes.Medium);
			Assert.Equal(9999.0, theme.Shapes.Full);
		}

		// ================================================================
		// Theme.SetControlStyle / GetControlStyle (§5.2)
		// ================================================================

		[Fact]
		public void Theme_SetControlStyle_StoresAndRetrievesStyle()
		{
			var theme = CreateTestTheme();
			var style = new TestButtonStyle();

			theme.SetControlStyle<Button, ButtonConfiguration>(style);

			var retrieved = theme.GetNewControlStyle<Button>();
			Assert.Same(style, retrieved);
		}

		[Fact]
		public void Theme_GetControlStyle_ReturnsNullWhenNotSet()
		{
			var theme = CreateTestTheme();

			var result = theme.GetNewControlStyle<Button>();
			Assert.Null(result);
		}

		[Fact]
		public void Theme_SetControlStyle_OverwritesPreviousStyle()
		{
			var theme = CreateTestTheme();
			var style1 = new TestButtonStyle();
			var style2 = new TestButtonStyle();

			theme.SetControlStyle<Button, ButtonConfiguration>(style1);
			theme.SetControlStyle<Button, ButtonConfiguration>(style2);

			var retrieved = theme.GetNewControlStyle<Button>();
			Assert.Same(style2, retrieved);
		}

		[Fact]
		public void Theme_SetControlStyle_DifferentTypesAreIndependent()
		{
			var theme = CreateTestTheme();
			var buttonStyle = new TestButtonStyle();
			var toggleStyle = new TestToggleStyle();

			theme.SetControlStyle<Button, ButtonConfiguration>(buttonStyle);
			theme.SetControlStyle<Toggle, ToggleConfiguration>(toggleStyle);

			Assert.Same(buttonStyle, theme.GetNewControlStyle<Button>());
			Assert.Same(toggleStyle, theme.GetNewControlStyle<Toggle>());
		}

		// ================================================================
		// Theme `with` expression — aliasing safety (§5.2, §10.2)
		// ================================================================

		[Fact]
		public void Theme_WithExpression_CreatesIndependentCopy()
		{
			var baseTheme = CreateTestTheme("Base", Colors.Purple);

			var derived = new Theme
			{
				Name = "Derived",
				Colors = baseTheme.Colors with
				{
					Primary = Colors.Teal,
				},
				Typography = baseTheme.Typography,
				Spacing = baseTheme.Spacing,
				Shapes = baseTheme.Shapes,
			};

			Assert.Equal("Derived", derived.Name);
			Assert.Equal(Colors.Teal, derived.Colors.Primary);
			// Base should be unchanged
			Assert.Equal("Base", baseTheme.Name);
			Assert.Equal(Colors.Purple, baseTheme.Colors.Primary);
		}

		[Fact]
		public void Theme_WithExpression_DerivedDoesNotShareMutableState()
		{
			var baseTheme = CreateTestTheme("Light");
			var derived = new Theme
			{
				Name = "Dark",
				Colors = baseTheme.Colors,
				Typography = baseTheme.Typography,
				Spacing = baseTheme.Spacing,
				Shapes = baseTheme.Shapes,
			};

			// Setting a control style on derived should not affect base
			derived.SetControlStyle<Button, ButtonConfiguration>(new TestButtonStyle());

			Assert.Null(baseTheme.GetNewControlStyle<Button>());
			Assert.NotNull(derived.GetNewControlStyle<Button>());
		}

		[Fact]
		public void Theme_WithExpression_SharesUnchangedTokenSets()
		{
			var baseTheme = CreateTestTheme("Base");
			var derived = new Theme
			{
				Name = "Derived",
				Colors = baseTheme.Colors,
				Typography = baseTheme.Typography,
				Spacing = baseTheme.Spacing,
				Shapes = baseTheme.Shapes,
			};

			// Typography, Spacing, Shapes should be the same reference
			Assert.Same(baseTheme.Typography, derived.Typography);
			Assert.Same(baseTheme.Spacing, derived.Spacing);
			Assert.Same(baseTheme.Shapes, derived.Shapes);
		}

		// ================================================================
		// ColorTokenSet record — with expression (§5.3)
		// ================================================================

		[Fact]
		public void ColorTokenSet_WithExpression_OverridesOnlySpecifiedColors()
		{
			var original = new ColorTokenSet
			{
				Primary = Colors.Purple,
				OnPrimary = Colors.White,
				Surface = Colors.White,
			};

			var modified = original with
			{
				Primary = Colors.Teal,
			};

			Assert.Equal(Colors.Teal, modified.Primary);
			Assert.Equal(Colors.White, modified.OnPrimary); // unchanged
			Assert.Equal(Colors.White, modified.Surface); // unchanged
		}

		// ================================================================
		// Default themes — Defaults.Light, Defaults.Dark (§6.2)
		// ================================================================

		[Fact]
		public void Defaults_Light_IsNotNull()
		{
			Assert.NotNull(Defaults.Light);
			Assert.Equal("Light", Defaults.Light.Name);
		}

		[Fact]
		public void Defaults_Dark_IsNotNull()
		{
			Assert.NotNull(Defaults.Dark);
			Assert.Equal("Dark", Defaults.Dark.Name);
		}

		[Fact]
		public void Defaults_Light_HasValidColors()
		{
			var theme = Defaults.Light;
			Assert.NotNull(theme.Colors);
			Assert.NotNull(theme.Colors.Primary);
			Assert.NotNull(theme.Colors.OnPrimary);
			Assert.NotNull(theme.Colors.Surface);
			Assert.NotNull(theme.Colors.Background);
		}

		[Fact]
		public void Defaults_Dark_HasDifferentColorsFromLight()
		{
			var light = Defaults.Light;
			var dark = Defaults.Dark;

			// Dark and light themes should have different surface/background colors
			Assert.NotEqual(light.Colors.Surface, dark.Colors.Surface);
		}

		[Fact]
		public void Defaults_Light_HasValidTypography()
		{
			var theme = Defaults.Light;
			Assert.NotNull(theme.Typography);
			Assert.True(theme.Typography.BodyMedium.Size > 0);
		}

		[Fact]
		public void Defaults_Light_HasValidSpacing()
		{
			var theme = Defaults.Light;
			Assert.NotNull(theme.Spacing);
			Assert.Equal(0.0, theme.Spacing.None);
			Assert.True(theme.Spacing.Medium > 0);
		}

		[Fact]
		public void Defaults_Light_HasValidShapes()
		{
			var theme = Defaults.Light;
			Assert.NotNull(theme.Shapes);
			Assert.Equal(0.0, theme.Shapes.None);
			Assert.Equal(9999.0, theme.Shapes.Full);
		}

		// ================================================================
		// Test helpers — style implementations
		// ================================================================

		class TestButtonStyle : IControlStyle<Button, ButtonConfiguration>
		{
			public ViewModifier Resolve(ButtonConfiguration configuration)
				=> ViewModifier.Empty;
		}

		class TestToggleStyle : IControlStyle<Toggle, ToggleConfiguration>
		{
			public ViewModifier Resolve(ToggleConfiguration configuration)
				=> ViewModifier.Empty;
		}
	}
}
