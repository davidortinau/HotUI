using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Comet.Internal;
using Microsoft.Maui;

namespace Comet
{
	public enum ItemsLayoutOrientation
	{
		Vertical,
		Horizontal
	}

	public enum ItemSizingStrategy
	{
		MeasureAllItems,
		MeasureFirstItem
	}

	public class ItemsLayout
	{
		public ItemsLayoutOrientation Orientation { get; set; }
		public double ItemSpacing { get; set; }

		public static ItemsLayout Vertical(double spacing = 0) =>
			new ItemsLayout { Orientation = ItemsLayoutOrientation.Vertical, ItemSpacing = spacing };

		public static ItemsLayout Horizontal(double spacing = 0) =>
			new ItemsLayout { Orientation = ItemsLayoutOrientation.Horizontal, ItemSpacing = spacing };
	}

	public class GridItemsLayout : ItemsLayout
	{
		public int Span { get; set; } = 1;

		public static GridItemsLayout Vertical(int span, double spacing = 0) =>
			new GridItemsLayout { Orientation = ItemsLayoutOrientation.Vertical, Span = span, ItemSpacing = spacing };

		public static GridItemsLayout Horizontal(int span, double spacing = 0) =>
			new GridItemsLayout { Orientation = ItemsLayoutOrientation.Horizontal, Span = span, ItemSpacing = spacing };
	}

	/// <summary>
	/// Modern collection view that wraps ListView with additional features.
	/// Supports vertical/horizontal layouts, grouping, empty views, and selection modes.
	/// </summary>
	public class CollectionView<T> : ListView<T>
	{
		public CollectionView() : base() { }

		public CollectionView(Binding<IReadOnlyList<T>> items) : base(items) { }

		public CollectionView(Func<IReadOnlyList<T>> items) : base(items) { }

		public ItemsLayout ItemsLayout { get; set; } = ItemsLayout.Vertical();

		public View EmptyView { get; set; }

		public SelectionMode SelectionMode { get; set; } = SelectionMode.Single;

		Binding<T> _selectedItem;
		public Binding<T> SelectedItem
		{
			get => _selectedItem;
			set => this.SetBindingValue(ref _selectedItem, value);
		}

		Binding<IReadOnlyList<T>> _selectedItems;
		public Binding<IReadOnlyList<T>> SelectedItems
		{
			get => _selectedItems;
			set => this.SetBindingValue(ref _selectedItems, value);
		}

		public View GroupHeaderTemplate { get; set; }
		public View GroupFooterTemplate { get; set; }
		public bool IsGrouped { get; set; }

		public Func<T, View> GroupHeaderViewFor { get; set; }
		public Func<T, View> GroupFooterViewFor { get; set; }

		public ItemSizingStrategy ItemSizingStrategy { get; set; } = ItemSizingStrategy.MeasureAllItems;

		// ScrollTo support
		public Action<int, bool> ScrollToRequested { get; set; }

		public void ScrollTo(int index, bool animate = true)
		{
			ScrollToRequested?.Invoke(index, animate);
		}
	}

	public enum SelectionMode
	{
		None,
		Single,
		Multiple
	}

	/// <summary>
	/// Non-generic CollectionView for simple scenarios.
	/// </summary>
	public class CollectionView : ListView
	{
		public ItemsLayout ItemsLayout { get; set; } = ItemsLayout.Vertical();

		public View EmptyView { get; set; }

		public SelectionMode SelectionMode { get; set; } = SelectionMode.Single;

		public ItemSizingStrategy ItemSizingStrategy { get; set; } = ItemSizingStrategy.MeasureAllItems;
	}
}
