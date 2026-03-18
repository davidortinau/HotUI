using System;
using Comet.Styles;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Comet.Tests
{
	/// <summary>
	/// Tests for IControlStyle, ButtonConfiguration, control style resolution,
	/// StyleToken&lt;T&gt;, and token override (§4, §7.5).
	/// Written TDD-style against the Style/Theme Spec.
	/// </summary>
	public class NewControlStyleTests : TestBase
	{
		// ================================================================
		// Test helpers
		// ================================================================

		sealed class TestFilledButtonStyle : IControlStyle<Button, ButtonConfiguration>
		{
			public ViewModifier Resolve(ButtonConfiguration config)
			{
				if (config.IsPressed)
					return new SetBackgroundModifier(Colors.DarkBlue);
				return new SetBackgroundModifier(Colors.Blue);
			}
		}

		sealed class TestOutlinedButtonStyle : IControlStyle<Button, ButtonConfiguration>
		{
			public ViewModifier Resolve(ButtonConfiguration config)
				=> new SetBackgroundModifier(Colors.Transparent);
		}

		sealed class TestToggleStyle : IControlStyle<Toggle, ToggleConfiguration>
		{
			public ViewModifier Resolve(ToggleConfiguration config)
				=> ViewModifier.Empty;
		}

		sealed class SetBackgroundModifier : ViewModifier
		{
			readonly Color _color;

			public SetBackgroundModifier(Color color) => _color = color;

			public override View Apply(View view)
			{
				view.Background(new SolidPaint(_color));
				return view;
			}
		}

		static Theme CreateTestTheme()
		{
			return new Theme
			{
				Name = "Test",
				Colors = new ColorTokenSet
				{
					Primary = Colors.Purple,
					OnPrimary = Colors.White,
					Surface = Colors.White,
					OnSurface = Colors.Black,
				},
				Typography = new TypographyTokenSet(),
				Spacing = new SpacingTokenSet(),
				Shapes = new ShapeTokenSet(),
			};
		}

		// ================================================================
		// IControlStyle<T,TConfig>.Resolve (§4.2)
		// ================================================================

		[Fact]
		public void IControlStyle_Resolve_ReturnsViewModifier()
		{
			var style = new TestFilledButtonStyle();
			var config = new ButtonConfiguration
			{
				IsPressed = false,
				IsEnabled = true,
			};

			var modifier = style.Resolve(config);
			Assert.NotNull(modifier);
			Assert.IsAssignableFrom<ViewModifier>(modifier);
		}

		[Fact]
		public void IControlStyle_Resolve_DifferentModifierForDifferentState()
		{
			var style = new TestFilledButtonStyle();

			var normalConfig = new ButtonConfiguration
			{
				IsPressed = false,
				IsEnabled = true,
			};
			var pressedConfig = new ButtonConfiguration
			{
				IsPressed = true,
				IsEnabled = true,
			};

			var normalMod = style.Resolve(normalConfig);
			var pressedMod = style.Resolve(pressedConfig);

			// Apply both to different buttons and check they have different backgrounds
			var normalButton = new Button("Normal");
			var pressedButton = new Button("Pressed");

			normalMod.Apply(normalButton);
			pressedMod.Apply(pressedButton);

			var normalBg = normalButton.GetEnvironment<Paint>(nameof(Microsoft.Maui.IView.Background));
			var pressedBg = pressedButton.GetEnvironment<Paint>(nameof(Microsoft.Maui.IView.Background));

			var normalSolid = Assert.IsType<SolidPaint>(normalBg);
			var pressedSolid = Assert.IsType<SolidPaint>(pressedBg);

			Assert.Equal(Colors.Blue, normalSolid.Color);
			Assert.Equal(Colors.DarkBlue, pressedSolid.Color);
		}

		// ================================================================
		// ButtonConfiguration struct (§4.3)
		// ================================================================

		[Fact]
		public void ButtonConfiguration_Creation_AllProperties()
		{
			var config = new ButtonConfiguration
			{
				IsPressed = true,
				IsHovered = true,
				IsEnabled = false,
				IsFocused = true,
				Label = "Click Me",
			};

			Assert.True(config.IsPressed);
			Assert.True(config.IsHovered);
			Assert.False(config.IsEnabled);
			Assert.True(config.IsFocused);
			Assert.Equal("Click Me", config.Label);
		}

		[Fact]
		public void ButtonConfiguration_Defaults()
		{
			var config = new ButtonConfiguration();

			Assert.False(config.IsPressed);
			Assert.False(config.IsHovered);
			Assert.False(config.IsEnabled);
			Assert.False(config.IsFocused);
			Assert.Null(config.Label);
		}

		[Fact]
		public void ButtonConfiguration_IsValueType()
		{
			Assert.True(typeof(ButtonConfiguration).IsValueType);
		}

		// ================================================================
		// ToggleConfiguration struct (§4.3)
		// ================================================================

		[Fact]
		public void ToggleConfiguration_Creation()
		{
			var config = new ToggleConfiguration
			{
				IsOn = true,
				IsEnabled = true,
				IsFocused = false,
			};

			Assert.True(config.IsOn);
			Assert.True(config.IsEnabled);
			Assert.False(config.IsFocused);
		}

		[Fact]
		public void ToggleConfiguration_IsValueType()
		{
			Assert.True(typeof(ToggleConfiguration).IsValueType);
		}

		// ================================================================
		// TextFieldConfiguration struct (§4.3)
		// ================================================================

		[Fact]
		public void TextFieldConfiguration_Creation()
		{
			var config = new TextFieldConfiguration
			{
				IsEditing = true,
				IsEnabled = true,
				IsFocused = true,
				Placeholder = "Enter text...",
			};

			Assert.True(config.IsEditing);
			Assert.True(config.IsEnabled);
			Assert.True(config.IsFocused);
			Assert.Equal("Enter text...", config.Placeholder);
		}

		[Fact]
		public void TextFieldConfiguration_IsValueType()
		{
			Assert.True(typeof(TextFieldConfiguration).IsValueType);
		}

		// ================================================================
		// SliderConfiguration struct (§4.3)
		// ================================================================

		[Fact]
		public void SliderConfiguration_Creation()
		{
			var config = new SliderConfiguration
			{
				Value = 0.5,
				Minimum = 0,
				Maximum = 1,
				IsEnabled = true,
				IsDragging = true,
			};

			Assert.Equal(0.5, config.Value);
			Assert.Equal(0.0, config.Minimum);
			Assert.Equal(1.0, config.Maximum);
			Assert.True(config.IsEnabled);
			Assert.True(config.IsDragging);
		}

		[Fact]
		public void SliderConfiguration_IsValueType()
		{
			Assert.True(typeof(SliderConfiguration).IsValueType);
		}

		// ================================================================
		// StyleToken<T>.Key (§12.2)
		// ================================================================

		[Fact]
		public void StyleToken_Key_IsDeterministic()
		{
			var key1 = StyleToken<Button>.Key;
			var key2 = StyleToken<Button>.Key;

			Assert.Equal(key1, key2);
		}

		[Fact]
		public void StyleToken_Key_ContainsControlTypeName()
		{
			var key = StyleToken<Button>.Key;
			Assert.Contains("Button", key);
		}

		[Fact]
		public void StyleToken_Key_DiffersPerControlType()
		{
			Assert.NotEqual(StyleToken<Button>.Key, StyleToken<Toggle>.Key);
			Assert.NotEqual(StyleToken<Button>.Key, StyleToken<Text>.Key);
		}

		// ================================================================
		// Control style via environment extension (§4.6)
		// ================================================================

		[Fact]
		public void ButtonStyle_Extension_StoresInEnvironment()
		{
			var style = new TestFilledButtonStyle();
			var view = new VStack { new Button("Click") };

			view.ButtonStyle(style);

			var retrieved = view.GetEnvironment<object>(StyleToken<Button>.Key);
			Assert.NotNull(retrieved);
		}

		[Fact]
		public void ButtonStyle_Extension_IsFluent()
		{
			var style = new TestFilledButtonStyle();
			var view = new VStack { new Button("Click") }.ButtonStyle(style);

			Assert.IsType<VStack>(view);
		}

		// ================================================================
		// Token Override (§7.5)
		// ================================================================

		[Fact]
		public void OverrideToken_SetsTokenInEnvironment()
		{
			var view = new VStack { new Text("Hello") };
			var token = new Token<Color>("theme.color.primary", "Primary");

			view.OverrideToken(token, Colors.Red);

			var value = view.GetEnvironment<Color>(token.Key);
			Assert.Equal(Colors.Red, value);
		}

		[Fact]
		public void OverrideToken_DoesNotAffectOtherTokens()
		{
			var primaryToken = new Token<Color>("theme.color.primary", "Primary");
			var secondaryToken = new Token<Color>("theme.color.secondary", "Secondary");

			var view = new VStack { new Text("Hello") };
			view.OverrideToken(primaryToken, Colors.Red);

			// Secondary should not be set
			var secondaryValue = view.GetEnvironment<Color>(secondaryToken.Key);
			Assert.Equal(default(Color), secondaryValue);
		}

		[Fact]
		public void OverrideToken_IsScopedToSubtree()
		{
			ResetComet();
			var token = new Token<Color>("theme.color.primary", "Primary");

			Text scopedChild = null;
			Text unscopedSibling = null;

			var root = new View
			{
				Body = () => new VStack
				{
					new VStack
					{
						(scopedChild = new Text("Scoped"))
					}.OverrideToken(token, Colors.Red),
					(unscopedSibling = new Text("Unscoped"))
				}
			};

			var handler = root.SetViewHandlerToGeneric();

			// Scoped child should see the override
			var scopedValue = scopedChild.GetEnvironment<Color>(token.Key);
			Assert.Equal(Colors.Red, scopedValue);

			// Unscoped sibling should NOT see the override
			var unscopedValue = unscopedSibling.GetEnvironment<Color>(token.Key);
			Assert.NotEqual(Colors.Red, unscopedValue);
		}

		[Fact]
		public void OverrideToken_IsFluent()
		{
			var token = new Token<Color>("theme.color.primary", "Primary");
			var stack = new VStack { new Text("Hello") }
				.OverrideToken(token, Colors.Red);

			Assert.IsType<VStack>(stack);
		}

		// ================================================================
		// FilledButtonStyle — state-aware resolution (§4.4)
		// ================================================================

		[Fact]
		public void FilledButtonStyle_Normal_ReturnsDefaultBackground()
		{
			var style = new TestFilledButtonStyle();
			var config = new ButtonConfiguration
			{
				IsPressed = false,
				IsEnabled = true,
			};

			var modifier = style.Resolve(config);
			var button = new Button("Click");
			modifier.Apply(button);

			var bg = button.GetEnvironment<Paint>(nameof(Microsoft.Maui.IView.Background));
			var solid = Assert.IsType<SolidPaint>(bg);
			Assert.Equal(Colors.Blue, solid.Color);
		}

		[Fact]
		public void FilledButtonStyle_Pressed_ReturnsPressedBackground()
		{
			var style = new TestFilledButtonStyle();
			var config = new ButtonConfiguration
			{
				IsPressed = true,
				IsEnabled = true,
			};

			var modifier = style.Resolve(config);
			var button = new Button("Click");
			modifier.Apply(button);

			var bg = button.GetEnvironment<Paint>(nameof(Microsoft.Maui.IView.Background));
			var solid = Assert.IsType<SolidPaint>(bg);
			Assert.Equal(Colors.DarkBlue, solid.Color);
		}
	}
}
