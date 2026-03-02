using System;
using Xunit;

namespace Comet.Tests
{
	public class AccessibilityTests : TestBase
	{
		[Fact]
		public void SemanticDescriptionSetsDescription()
		{
			var view = new Text("Hello").SemanticDescription("A greeting label");

			var semantics = ((Microsoft.Maui.IView)view).Semantics;
			Assert.NotNull(semantics);
			Assert.Equal("A greeting label", semantics.Description);
		}

		[Fact]
		public void SemanticHintSetsHint()
		{
			var view = new Text("Hello").SemanticHint("Double tap to activate");

			var semantics = ((Microsoft.Maui.IView)view).Semantics;
			Assert.NotNull(semantics);
			Assert.Equal("Double tap to activate", semantics.Hint);
		}

		[Fact]
		public void SemanticHeadingLevelSetsHeading()
		{
			var view = new Text("Title").SemanticHeadingLevel(Microsoft.Maui.SemanticHeadingLevel.Level1);

			var semantics = ((Microsoft.Maui.IView)view).Semantics;
			Assert.NotNull(semantics);
			Assert.Equal(Microsoft.Maui.SemanticHeadingLevel.Level1, semantics.HeadingLevel);
		}

		[Fact]
		public void SemanticPropertiesCanBeChained()
		{
			var view = new Text("Hello")
				.SemanticDescription("Description")
				.SemanticHint("Hint")
				.SemanticHeadingLevel(Microsoft.Maui.SemanticHeadingLevel.Level2);

			var semantics = ((Microsoft.Maui.IView)view).Semantics;
			Assert.NotNull(semantics);
			Assert.Equal("Description", semantics.Description);
			Assert.Equal("Hint", semantics.Hint);
			Assert.Equal(Microsoft.Maui.SemanticHeadingLevel.Level2, semantics.HeadingLevel);
		}

		[Fact]
		public void SetAutomationIdSetsValue()
		{
			var view = new Text("Hello");
			view.SetAutomationId("myTextId");

			Assert.Equal("myTextId", view.GetAutomationId());
		}

		[Fact]
		public void GetAutomationIdReturnsNullWhenNotSet()
		{
			var view = new Text("Hello");
			Assert.Null(view.GetAutomationId());
		}

		[Fact]
		public void AutomationIdViaIView()
		{
			var view = new Text("Hello");
			view.SetAutomationId("testId");

			Microsoft.Maui.IView iview = view;
			Assert.Equal("testId", iview.AutomationId);
		}

		[Fact]
		public void IsReadOnlySetsProperty()
		{
			var view = new Text("Hello").IsReadOnly();

			var isReadOnly = view.GetEnvironment<bool>("View.IsReadOnly");
			Assert.True(isReadOnly);
		}

		[Fact]
		public void IsReadOnlyFalse()
		{
			var view = new Text("Hello").IsReadOnly(false);

			var isReadOnly = view.GetEnvironment<bool>("View.IsReadOnly");
			Assert.False(isReadOnly);
		}

		[Fact]
		public void SemanticDescriptionOverwritesPrevious()
		{
			var view = new Text("Hello")
				.SemanticDescription("First")
				.SemanticDescription("Second");

			var semantics = ((Microsoft.Maui.IView)view).Semantics;
			Assert.Equal("Second", semantics.Description);
		}

		[Fact]
		public void DifferentHeadingLevels()
		{
			foreach (var level in new[] { Microsoft.Maui.SemanticHeadingLevel.None, Microsoft.Maui.SemanticHeadingLevel.Level1, Microsoft.Maui.SemanticHeadingLevel.Level2, Microsoft.Maui.SemanticHeadingLevel.Level3 })
			{
				var view = new Text("Test").SemanticHeadingLevel(level);
				var semantics = ((Microsoft.Maui.IView)view).Semantics;
				Assert.Equal(level, semantics.HeadingLevel);
			}
		}
	}
}
