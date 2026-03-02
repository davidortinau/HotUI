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

		[Fact]
		public void MauiViewHost_WrapsIView()
		{
			var mockView = new TestIViewImpl();
			var host = new MauiViewHost(mockView);

			Assert.Same(mockView, host.HostedView);
			Assert.Same(mockView, ((IContentView)host).PresentedContent);
		}

		[Fact]
		public void MauiViewHost_LazyFactory()
		{
			var created = false;
			var host = new MauiViewHost(() =>
			{
				created = true;
				return new TestIViewImpl();
			});

			Assert.False(created);
			var view = host.HostedView;
			Assert.True(created);
			Assert.NotNull(view);
			Assert.Same(view, host.HostedView);
		}

		[Fact]
		public void MauiViewHost_MeasureDelegatesToHostedView()
		{
			var mockView = new TestIViewImpl { DesiredSizeValue = new Size(100, 50) };
			var host = new MauiViewHost(mockView);
			var measured = host.GetDesiredSize(new Size(200, 200));
			Assert.Equal(100, measured.Width);
			Assert.Equal(50, measured.Height);
		}

		[Fact]
		public void MauiViewHost_FactoryThreadSafety()
		{
			int callCount = 0;
			var host = new MauiViewHost(() =>
			{
				System.Threading.Interlocked.Increment(ref callCount);
				System.Threading.Thread.Sleep(10);
				return new TestIViewImpl();
			});

			var tasks = new System.Threading.Tasks.Task[10];
			for (int i = 0; i < tasks.Length; i++)
				tasks[i] = System.Threading.Tasks.Task.Run(() => { var _ = host.HostedView; });
			System.Threading.Tasks.Task.WaitAll(tasks);

			Assert.Equal(1, callCount);
			Assert.NotNull(host.HostedView);
		}

		[Fact]
		public void MauiViewHost_DisposeNullsHostedView()
		{
			var mockView = new TestIViewImpl();
			var host = new MauiViewHost(mockView);
			Assert.NotNull(host.HostedView);

			host.Dispose();
			Assert.Null(host.HostedView);
		}

		[Fact]
		public void MauiViewHost_ContentViewContent()
		{
			var mockView = new TestIViewImpl();
			var host = new MauiViewHost(mockView);
			var cv = (IContentView)host;

			Assert.Same(mockView, cv.Content);
			Assert.Same(mockView, cv.PresentedContent);
		}

		[Fact]
		public void ContainerView_AcceptsIViewDirectly()
		{
			var container = new VStack();
			var mauiView = new TestIViewImpl();
			((ContainerView)container).Add((IView)mauiView);

			Assert.Equal(1, container.Count);
			var child = container[0];
			Assert.IsAssignableFrom<IReplaceableView>(child);
		}

		private class TestIViewImpl : IView
		{
			public Size DesiredSizeValue { get; set; } = new Size(50, 50);
			public string AutomationId => "";
			public FlowDirection FlowDirection => FlowDirection.LeftToRight;
			Microsoft.Maui.Primitives.LayoutAlignment IView.HorizontalLayoutAlignment => Microsoft.Maui.Primitives.LayoutAlignment.Fill;
			Microsoft.Maui.Primitives.LayoutAlignment IView.VerticalLayoutAlignment => Microsoft.Maui.Primitives.LayoutAlignment.Fill;
			public Semantics Semantics => null;
			public IShape Clip => null;
			public IShadow Shadow => null;
			public bool IsEnabled => true;
			public bool IsFocused { get; set; }
			public Visibility Visibility => Visibility.Visible;
			public double Opacity => 1;
			public Paint Background => null;
			public Rect Frame { get; set; }
			public double Width => -1;
			public double MinimumWidth => -1;
			public double MaximumWidth => -1;
			public double Height => -1;
			public double MinimumHeight => -1;
			public double MaximumHeight => -1;
			public Thickness Margin => Thickness.Zero;
			public Size DesiredSize => DesiredSizeValue;
			public int ZIndex => 0;
			public bool InputTransparent => false;
			public double TranslationX => 0;
			public double TranslationY => 0;
			public double Scale => 1;
			public double ScaleX => 1;
			public double ScaleY => 1;
			public double Rotation => 0;
			public double RotationX => 0;
			public double RotationY => 0;
			public double AnchorX => 0.5;
			public double AnchorY => 0.5;
			public IViewHandler Handler { get; set; }
			IElementHandler IElement.Handler { get; set; }
			public IElement Parent => null;
			public Size Arrange(Rect bounds) { Frame = bounds; return DesiredSizeValue; }
			public bool Focus() => false;
			public void Unfocus() { }
			public void InvalidateArrange() { }
			public void InvalidateMeasure() { }
			public Size Measure(double widthConstraint, double heightConstraint) => DesiredSizeValue;
		}
	}
}
