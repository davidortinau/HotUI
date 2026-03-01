using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Comet.Tests
{
	public class NewControlsTests : TestBase
	{
		// ---- Border Tests ----

		[Fact]
		public void BorderCreation()
		{
			var border = new Border();
			border.Add(new Text("Hello"));
			Assert.NotNull(border);
			Assert.NotNull(border.Content);
		}

		// ---- BoxView Tests ----

		[Fact]
		public void BoxViewCreation()
		{
			var box = new BoxView();
			Assert.NotNull(box);
		}

		[Fact]
		public void BoxViewWithColor()
		{
			var box = new BoxView { Color = Colors.Blue };
			Assert.Equal(Colors.Blue, box.Color?.CurrentValue);
		}

		// ---- Frame Tests ----

		[Fact]
		public void FrameCreation()
		{
			var frame = new Frame();
			frame.Add(new Text("Hello"));
			Assert.NotNull(frame);
			Assert.NotNull(frame.Content);
		}

		[Fact]
		public void FrameHasBorderAndShadow()
		{
			var frame = new Frame();
			frame.BorderColor = Colors.Green;
			frame.CornerRadius = 5;
			frame.HasShadow = true;
			Assert.Equal(Colors.Green, frame.BorderColor?.CurrentValue);
			Assert.Equal(5f, frame.CornerRadius?.CurrentValue);
			Assert.True(frame.HasShadow?.CurrentValue);
		}

		// ---- SwipeView Tests ----

		[Fact]
		public void SwipeViewCreation()
		{
			var swipe = new SwipeView();
			swipe.Add(new Text("Swipeable"));
			Assert.NotNull(swipe);
			Assert.NotNull(swipe.Content);
		}

		// ---- CollectionView Tests ----

		[Fact]
		public void CollectionViewCreation()
		{
			var items = new List<string> { "A", "B", "C" };
			var cv = new CollectionView<string>(items.AsReadOnly());
			cv.ViewFor = item => new Text(item);
			Assert.NotNull(cv);
			Assert.Equal(ItemsLayoutOrientation.Vertical, cv.ItemsLayout.Orientation);
		}

		[Fact]
		public void CollectionViewHorizontal()
		{
			var cv = new CollectionView<string>();
			cv.ItemsLayout = ItemsLayout.Horizontal(10);
			Assert.Equal(ItemsLayoutOrientation.Horizontal, cv.ItemsLayout.Orientation);
			Assert.Equal(10, cv.ItemsLayout.ItemSpacing);
		}

		[Fact]
		public void CollectionViewGrid()
		{
			var cv = new CollectionView<string>();
			cv.ItemsLayout = GridItemsLayout.Vertical(2, 5);
			var grid = cv.ItemsLayout as GridItemsLayout;
			Assert.NotNull(grid);
			Assert.Equal(2, grid.Span);
			Assert.Equal(5, grid.ItemSpacing);
		}

		[Fact]
		public void CollectionViewSelectionModes()
		{
			var cv = new CollectionView<string>();
			Assert.Equal(SelectionMode.Single, cv.SelectionMode);
			cv.SelectionMode = SelectionMode.Multiple;
			Assert.Equal(SelectionMode.Multiple, cv.SelectionMode);
			cv.SelectionMode = SelectionMode.None;
			Assert.Equal(SelectionMode.None, cv.SelectionMode);
		}

		[Fact]
		public void CollectionViewEmptyView()
		{
			var cv = new CollectionView<string>();
			cv.EmptyView = new Text("No items");
			Assert.NotNull(cv.EmptyView);
		}

		// ---- CarouselView Tests ----

		[Fact]
		public void CarouselViewCreation()
		{
			var items = new List<string> { "Slide 1", "Slide 2", "Slide 3" };
			var cv = new CarouselView<string>(items.AsReadOnly());
			cv.ViewFor = item => new Text(item);
			Assert.Equal(ItemsLayoutOrientation.Horizontal, cv.ItemsLayout.Orientation);
		}

		[Fact]
		public void CarouselViewProperties()
		{
			var cv = new CarouselView<string>();
			cv.Loop = true;
			cv.PeekAreaInsets = 20;
			cv.IsBounceEnabled = false;
			Assert.True(cv.Loop);
			Assert.Equal(20, cv.PeekAreaInsets);
			Assert.False(cv.IsBounceEnabled);
		}

		// ---- WebView Tests ----

		[Fact]
		public void WebViewCreation()
		{
			var wv = new WebView();
			wv.Source = "https://example.com";
			Assert.Equal("https://example.com", wv.Source?.CurrentValue);
		}

		[Fact]
		public void WebViewHtml()
		{
			var wv = new WebView();
			wv.Html = "<h1>Hello</h1>";
			Assert.Equal("<h1>Hello</h1>", wv.Html?.CurrentValue);
		}
	}
}

