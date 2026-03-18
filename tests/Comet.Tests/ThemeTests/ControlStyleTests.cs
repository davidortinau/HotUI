using System;
using Comet.Styles;
using Comet.Styles.Material;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Comet.Tests
{
	public class ControlStyleTests : TestBase
	{
		// ================================================================
		// Style<T> — typed functional styles
		// ================================================================

		[Fact]
		public void StyleOfTAppliesAction()
		{
			var applied = false;
			var style = new Style<Text>(_ => applied = true);
			var text = new Text("Hello");
			style.Apply(text);
			Assert.True(applied);
		}

		[Fact]
		public void StyleOfTReturnsSameView()
		{
			var style = new Style<Text>(t => t.FontSize(24));
			var text = new Text("Hello");
			var result = style.Apply(text);
			Assert.Same(text, result);
		}

		[Fact]
		public void StyleOfTAppliesFontSize()
		{
			var style = new Style<Text>(t => t.FontSize(24));
			var text = new Text("Hello");
			style.Apply(text);

			var size = text.GetEnvironment<double>(EnvironmentKeys.Fonts.Size);
			Assert.Equal(24.0, size);
		}

		[Fact]
		public void StyleOfTAppliesColor()
		{
			var style = new Style<Text>(t => t.Color(Colors.Red));
			var text = new Text("Hello");
			style.Apply(text);

			var color = text.GetEnvironment<Color>(EnvironmentKeys.Colors.Color);
			Assert.Equal(Colors.Red, color);
		}

		[Fact]
		public void StyleOfTAppliesBackground()
		{
			var style = new Style<Button>(b => b.Background(new SolidPaint(Colors.Blue)));
			var button = new Button("Click");
			style.Apply(button);

			var bg = button.GetEnvironment<Paint>(nameof(IView.Background));
			Assert.NotNull(bg);
			var solid = Assert.IsType<SolidPaint>(bg);
			Assert.Equal(Colors.Blue, solid.Color);
		}

		[Fact]
		public void StyleOfTAppliesMultipleProperties()
		{
			var style = new Style<Text>(t => t
				.FontSize(18)
				.Color(Colors.Navy)
			);
			var text = new Text("Hello");
			style.Apply(text);

			var size = text.GetEnvironment<double>(EnvironmentKeys.Fonts.Size);
			var color = text.GetEnvironment<Color>(EnvironmentKeys.Colors.Color);
			Assert.Equal(18.0, size);
			Assert.Equal(Colors.Navy, color);
		}

		[Fact]
		public void StyleOfTNullApplyThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new Style<Text>(null));
		}

		// ================================================================
		// StyleApply extension method
		// ================================================================

		[Fact]
		public void StyleApplyExtensionAppliesStyle()
		{
			var style = new Style<Text>(t => t.FontSize(32));
			var text = new Text("Hello").StyleApply(style);

			var size = text.GetEnvironment<double>(EnvironmentKeys.Fonts.Size);
			Assert.Equal(32.0, size);
		}

		[Fact]
		public void StyleApplyExtensionReturnsSameView()
		{
			var style = new Style<Text>(t => t.FontSize(32));
			var text = new Text("Hello");
			var result = text.StyleApply(style);
			Assert.Same(text, result);
		}

		// ================================================================
		// Style<T>.RegisterImplicit / ApplyImplicit
		// ================================================================

		[Fact]
		public void ImplicitStyleApplies()
		{
			Style<Text>.ClearImplicit();
			try
			{
				var style = new Style<Text>(t => t.FontSize(16));
				style.RegisterImplicit();

				var text = new Text("Hello");
				Style<Text>.ApplyImplicit(text);

				var size = text.GetEnvironment<double>(EnvironmentKeys.Fonts.Size);
				Assert.Equal(16.0, size);
			}
			finally
			{
				Style<Text>.ClearImplicit();
			}
		}

		[Fact]
		public void ImplicitStyleDoesNotAffectOtherTypes()
		{
			Style<Text>.ClearImplicit();
			Style<Button>.ClearImplicit();
			try
			{
				var textStyle = new Style<Text>(t => t.FontSize(16));
				textStyle.RegisterImplicit();

				var button = new Button("Click");
				Style<Button>.ApplyImplicit(button);

				// Button should NOT have font size set by Text's implicit style
				var size = button.GetEnvironment<double>(EnvironmentKeys.Fonts.Size);
				Assert.Equal(0.0, size);
			}
			finally
			{
				Style<Text>.ClearImplicit();
				Style<Button>.ClearImplicit();
			}
		}

		[Fact]
		public void ClearImplicitRemovesStyles()
		{
			Style<Text>.ClearImplicit();
			try
			{
				var style = new Style<Text>(t => t.FontSize(99));
				style.RegisterImplicit();
				Style<Text>.ClearImplicit();

				var text = new Text("Hello");
				Style<Text>.ApplyImplicit(text);

				var size = text.GetEnvironment<double>(EnvironmentKeys.Fonts.Size);
				Assert.Equal(0.0, size);
			}
			finally
			{
				Style<Text>.ClearImplicit();
			}
		}

		[Fact]
		public void MultipleImplicitStylesApplyInOrder()
		{
			Style<Text>.ClearImplicit();
			try
			{
				var style1 = new Style<Text>(t => t.FontSize(12));
				var style2 = new Style<Text>(t => t.FontSize(24));

				style1.RegisterImplicit();
				style2.RegisterImplicit();

				var text = new Text("Hello");
				Style<Text>.ApplyImplicit(text);

				// Last registered wins (overwrites)
				var size = text.GetEnvironment<double>(EnvironmentKeys.Fonts.Size);
				Assert.Equal(24.0, size);
			}
			finally
			{
				Style<Text>.ClearImplicit();
			}
		}

		// ================================================================
		// Classic Style (non-generic) — environment-based styling
		// ================================================================

		[Fact]
		public void ClassicStyleApplySetsGlobalEnvironment()
		{
			ResetComet();
			var style = new Style();
			style.Apply();

			// After Apply(), global env should have FlowDirection set
			var fd = View.GetGlobalEnvironment<Microsoft.Maui.FlowDirection>(
				nameof(IView.FlowDirection));
			Assert.True(Enum.IsDefined(typeof(Microsoft.Maui.FlowDirection), fd));
		}

		[Fact]
		public void ClassicStyleApplyToViewSetsLocalEnvironment()
		{
			ResetComet();
			var style = new Style();
			var view = new View();
			style.Apply(view);

			var fd = view.GetEnvironment<Microsoft.Maui.FlowDirection>(
				nameof(IView.FlowDirection));
			Assert.True(Enum.IsDefined(typeof(Microsoft.Maui.FlowDirection), fd));
		}

		[Fact]
		public void MaterialStyleInheritsFromStyle()
		{
			var material = new MaterialStyle(ColorPalette.Blue);
			Assert.IsAssignableFrom<Style>(material);
		}

		// ================================================================
		// Explicit override beats style — environment precedence
		// ================================================================

		[Fact]
		public void ExplicitBackgroundOverridesStyle()
		{
			var style = new Style<Text>(t => t.Background(new SolidPaint(Colors.Gray)));
			var text = new Text("Hello");

			// Apply style first, then explicit
			style.Apply(text);
			text.Background(new SolidPaint(Colors.Red));

			var bg = text.GetEnvironment<Paint>(nameof(IView.Background));
			Assert.NotNull(bg);
			var solid = Assert.IsType<SolidPaint>(bg);
			Assert.Equal(Colors.Red, solid.Color);
		}

		[Fact]
		public void ExplicitFontSizeOverridesStyle()
		{
			var style = new Style<Text>(t => t.FontSize(12));
			var text = new Text("Hello");

			style.Apply(text);
			text.FontSize(48);

			var size = text.GetEnvironment<double>(EnvironmentKeys.Fonts.Size);
			Assert.Equal(48.0, size);
		}

		[Fact]
		public void ExplicitColorOverridesThemeColor()
		{
			var original = Theme.Current;
			try
			{
				var custom = new Theme { PrimaryColor = Colors.Green };
				Theme.Current = custom;

				// ThemeColor sets Background, then explicit Background overrides
				var text = new Text("Hello")
					.ThemeColor(t => t.PrimaryColor)
					.Background(new SolidPaint(Colors.Yellow));

				var bg = text.GetEnvironment<Paint>(nameof(IView.Background));
				var solid = Assert.IsType<SolidPaint>(bg);
				Assert.Equal(Colors.Yellow, solid.Color);
			}
			finally
			{
				Theme.Current = original;
			}
		}

		// ================================================================
		// Environment cascading with styles
		// ================================================================

		[Fact]
		public void StyleAppliedToParentCascadesToChild()
		{
			ResetComet();
			Text text = null;

			var view = new View
			{
				Body = () => (text = new Text("Hello"))
			}.SetEnvironment(EnvironmentKeys.Fonts.Size, 20.0, cascades: true);

			var handler = view.SetViewHandlerToGeneric();

			var size = text.GetEnvironment<double>(EnvironmentKeys.Fonts.Size);
			Assert.Equal(20.0, size);
		}

		[Fact]
		public void ChildLocalValueOverridesCascadedParent()
		{
			ResetComet();
			const string key = "TestFontSize";
			Text text = null;

			var view = new View
			{
				Body = () => new VStack {
					(text = new Text("Hello").SetEnvironment(key, 14.0))
				}.SetEnvironment(key, 20.0, cascades: true)
			};

			var handler = view.SetViewHandlerToGeneric();

			var size = text.GetEnvironment<double>(key);
			Assert.Equal(14.0, size);
		}

		[Fact]
		public void NonCascadingStyleDoesNotReachChild()
		{
			ResetComet();
			const string key = "TestNonCascade";
			const string globalValue = "global";
			const string localValue = "local";

			View.SetGlobalEnvironment(key, globalValue);

			Text text = null;

			var view = new View
			{
				Body = () => new VStack {
					(text = new Text("Hello"))
				}.SetEnvironment(key, localValue, cascades: false)
			};

			var handler = view.SetViewHandlerToGeneric();

			// Child should get global value since parent's is non-cascading
			var result = text.GetEnvironment<string>(key);
			Assert.Equal(globalValue, result);
		}

		// ================================================================
		// ControlStyle<T> — now implemented (Phase 3.1 complete)
		// ================================================================

		[Fact]
		public void ControlStyleCreation()
		{
			var style = new ControlStyle<Button>()
				.Set(EnvironmentKeys.Colors.Background, new SolidPaint(Colors.Blue))
				.Set(EnvironmentKeys.Colors.Color, Colors.White);
			Assert.NotNull(style);
			Assert.True(style.HasProperty(EnvironmentKeys.Colors.Background));
			Assert.True(style.HasProperty(EnvironmentKeys.Colors.Color));
		}

		[Fact]
		public void ControlStyleAppliesProperties()
		{
			ResetComet();
			var style = new ControlStyle<Button>()
				.Set(EnvironmentKeys.Colors.Background, new SolidPaint(Colors.Blue))
				.Set(EnvironmentKeys.Colors.Color, Colors.White);

			// Apply globally
			style.Apply();

			// Create a button and verify the typed global env is set
			var bgPaint = View.GetGlobalEnvironment<object>(
				ContextualObject.GetTypedKey(typeof(Button), EnvironmentKeys.Colors.Background));
			Assert.NotNull(bgPaint);
			var solid = Assert.IsType<SolidPaint>(bgPaint);
			Assert.Equal(Colors.Blue, solid.Color);
		}

		[Fact]
		public void ControlStyleCascadesThroughViewTree()
		{
			ResetComet();
			var style = new ControlStyle<Button>()
				.Set(EnvironmentKeys.Colors.Background, new SolidPaint(Colors.Purple));

			style.Apply();

			Button btn = null;
			var view = new View
			{
				Body = () => new VStack { (btn = new Button("Click")) }
			};

			var handler = view.SetViewHandlerToGeneric();
			var bg = btn.GetBackground(typeof(Button));
			Assert.NotNull(bg);
		}

		[Fact]
		public void ThemeProvidesDefaultControlStyles()
		{
			var theme = Theme.Light;
			theme.Apply();

			// DefaultThemeStyles.Register is called by Apply, so styles should exist
			var buttonStyle = theme.GetControlStyle<Button>();
			Assert.NotNull(buttonStyle);
			Assert.True(buttonStyle.HasProperty(EnvironmentKeys.Colors.Background));
		}

		[Fact]
		public void ExplicitOverridesControlStyle()
		{
			ResetComet();
			var style = new ControlStyle<Button>()
				.Set(EnvironmentKeys.Colors.Background, new SolidPaint(Colors.Blue));

			// Apply typed style globally
			style.Apply();

			// Create a button with an explicit background override
			var button = new Button("Click")
				.Background(new SolidPaint(Colors.Red));

			var bg = button.GetEnvironment<Paint>(nameof(IView.Background));
			Assert.NotNull(bg);
			var solid = Assert.IsType<SolidPaint>(bg);
			Assert.Equal(Colors.Red, solid.Color);
		}

		[Fact]
		public void ThemeValuesFlowThroughEnvironmentSystem()
		{
			ResetComet();
			var original = Theme.Current;
			try
			{
				var theme = new Theme { PrimaryColor = Colors.Teal };
				Theme.Current = theme;

				var view = new Text("Hello")
					.ThemeColor(t => t.PrimaryColor);

				var bg = view.GetEnvironment<Paint>(nameof(IView.Background));
				var solid = Assert.IsType<SolidPaint>(bg);
				Assert.Equal(Colors.Teal, solid.Color);
			}
			finally
			{
				Theme.Current = original;
			}
		}
	}
}
