using System;
using System.Collections.Generic;
using Microsoft.Maui;

namespace Comet
{
	/// <summary>
	/// A horizontally-scrolling collection view for displaying items in a carousel pattern.
	/// Extends CollectionView with position tracking, peek, loop, and swipe behavior.
	/// </summary>
	public class CarouselView<T> : CollectionView<T>
	{
		public CarouselView() : base()
		{
			ItemsLayout = ItemsLayout.Horizontal();
		}

		public CarouselView(Binding<IReadOnlyList<T>> items) : base(items)
		{
			ItemsLayout = ItemsLayout.Horizontal();
		}

		public CarouselView(Func<IReadOnlyList<T>> items) : base(items)
		{
			ItemsLayout = ItemsLayout.Horizontal();
		}

		Binding<int> _position;
		public Binding<int> Position
		{
			get => _position;
			set => this.SetBindingValue(ref _position, value);
		}

		Binding<T> _currentItem;
		public Binding<T> CurrentItem
		{
			get => _currentItem;
			set => this.SetBindingValue(ref _currentItem, value);
		}

		public bool IsBounceEnabled { get; set; } = true;
		public bool IsSwipeEnabled { get; set; } = true;
		public bool IsScrollAnimated { get; set; } = true;
		public bool Loop { get; set; }
		public double PeekAreaInsets { get; set; }

		public Action<int> PositionChanged { get; set; }
		public Action<T> CurrentItemChanged { get; set; }

		public void ScrollTo(int position, bool animate = true)
		{
			ScrollToRequested?.Invoke(position, animate);
		}
	}

	/// <summary>
	/// Non-generic CarouselView for simple scenarios.
	/// </summary>
	public class CarouselView : CollectionView
	{
		Binding<int> _position;
		public Binding<int> Position
		{
			get => _position;
			set => this.SetBindingValue(ref _position, value);
		}

		public bool IsBounceEnabled { get; set; } = true;
		public bool IsSwipeEnabled { get; set; } = true;
		public bool IsScrollAnimated { get; set; } = true;
		public bool Loop { get; set; }
		public double PeekAreaInsets { get; set; }
	}
}
