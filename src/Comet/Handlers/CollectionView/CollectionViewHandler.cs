using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace Comet.Handlers
{
	public partial class CollectionViewHandler
	{
		public static readonly PropertyMapper<IListView, CollectionViewHandler> Mapper =
			new PropertyMapper<IListView, CollectionViewHandler>(ViewHandler.ViewMapper)
			{
				["ListView"] = MapListViewProperty,
			};

		public static readonly CommandMapper<IListView, CollectionViewHandler> ActionMapper =
			new CommandMapper<IListView, CollectionViewHandler>
			{
				[nameof(ListView.ReloadData)] = MapReloadData,
			};

		public CollectionViewHandler() : base(Mapper, ActionMapper) { }

		Microsoft.Maui.Controls.CollectionView _mauiCollectionView;

		/// <summary>
		/// Creates and fully configures a MAUI CollectionView from the Comet IListView.
		/// </summary>
		static Microsoft.Maui.Controls.CollectionView CreateAndConfigureMauiCollectionView(IListView listView)
		{
			var cv = new Microsoft.Maui.Controls.CollectionView();
			ConfigureMauiCollectionView(cv, listView);
			RefreshItemsSource(cv, listView);
			return cv;
		}

		/// <summary>
		/// One-time configuration: layout, selection, empty view, header/footer, template.
		/// </summary>
		static void ConfigureMauiCollectionView(Microsoft.Maui.Controls.CollectionView cv, IListView listView)
		{
			MapCometItemsLayout(cv, listView);
			MapCometSelectionMode(cv, listView);
			MapCometEmptyView(cv, listView);
			MapCometHeaderFooter(cv, listView);

			// Store reference to current listView that can be updated when VirtualView changes
			var listViewRef = new WeakReference<IListView>(listView);
			cv.ItemTemplate = new Microsoft.Maui.Controls.DataTemplate(() =>
			{
				var container = new Microsoft.Maui.Controls.ContentView();
				container.BindingContextChanged += (s, e) =>
				{
					if (container.BindingContext is not CollectionViewItemProxy proxy)
						return;
					if (!listViewRef.TryGetTarget(out var currentListView))
						return;
					var cometView = currentListView.ViewFor(proxy.Section, proxy.Row);
					container.Content = cometView != null ? new CometHost(cometView) : null;
				};
				return container;
			});

			cv.SelectionChanged += (s, e) =>
			{
				var selected = e.CurrentSelection?.FirstOrDefault();
				if (selected is CollectionViewItemProxy proxy)
				{
					if (listViewRef.TryGetTarget(out var currentListView))
						currentListView.OnSelected(proxy.Section, proxy.Row);
				}
			};
		}

		/// <summary>
		/// Rebuilds the items list from IListView sections/rows.
		/// </summary>
		static void RefreshItemsSource(Microsoft.Maui.Controls.CollectionView cv, IListView listView)
		{
			var items = new List<CollectionViewItemProxy>();
			var sections = listView.Sections();
			for (int s = 0; s < sections; s++)
			{
				var rows = listView.Rows(s);
				for (int r = 0; r < rows; r++)
					items.Add(new CollectionViewItemProxy(s, r));
			}
			cv.ItemsSource = items;
		}

		static void MapCometItemsLayout(Microsoft.Maui.Controls.CollectionView cv, IListView listView)
		{
			var cometLayout = GetPropertyValue<Comet.ItemsLayout>(listView, nameof(Comet.CollectionView.ItemsLayout));
			if (cometLayout == null)
				return;

			if (cometLayout is Comet.GridItemsLayout gridLayout)
			{
				var orientation = gridLayout.Orientation == ItemsLayoutOrientation.Vertical
					? Microsoft.Maui.Controls.ItemsLayoutOrientation.Vertical
					: Microsoft.Maui.Controls.ItemsLayoutOrientation.Horizontal;

				cv.ItemsLayout = new Microsoft.Maui.Controls.GridItemsLayout(gridLayout.Span, orientation)
				{
					HorizontalItemSpacing = gridLayout.ItemSpacing,
					VerticalItemSpacing = gridLayout.ItemSpacing
				};
			}
			else
			{
				var orientation = cometLayout.Orientation == ItemsLayoutOrientation.Vertical
					? Microsoft.Maui.Controls.ItemsLayoutOrientation.Vertical
					: Microsoft.Maui.Controls.ItemsLayoutOrientation.Horizontal;

				cv.ItemsLayout = new Microsoft.Maui.Controls.LinearItemsLayout(orientation)
				{
					ItemSpacing = cometLayout.ItemSpacing
				};
			}
		}

		static void MapCometSelectionMode(Microsoft.Maui.Controls.CollectionView cv, IListView listView)
		{
			var mode = GetPropertyValue<Comet.SelectionMode?>(listView, nameof(Comet.CollectionView.SelectionMode));

			cv.SelectionMode = mode switch
			{
				Comet.SelectionMode.None => Microsoft.Maui.Controls.SelectionMode.None,
				Comet.SelectionMode.Multiple => Microsoft.Maui.Controls.SelectionMode.Multiple,
				_ => Microsoft.Maui.Controls.SelectionMode.Single,
			};
		}

		static void MapCometEmptyView(Microsoft.Maui.Controls.CollectionView cv, IListView listView)
		{
			var emptyView = GetPropertyValue<Comet.View>(listView, nameof(Comet.CollectionView.EmptyView));
			cv.EmptyView = emptyView != null ? new CometHost(emptyView) : null;
		}

		static void MapCometHeaderFooter(Microsoft.Maui.Controls.CollectionView cv, IListView listView)
		{
			var header = listView.HeaderView();
			cv.Header = header != null ? new CometHost(header) : null;

			var footer = listView.FooterView();
			cv.Footer = footer != null ? new CometHost(footer) : null;
		}

		/// <summary>
		/// Gets a property value from a view by name, supporting both generic and non-generic CollectionView types.
		/// </summary>
		static T GetPropertyValue<T>(IListView listView, string propertyName)
		{
			var prop = listView.GetType().GetProperty(propertyName);
			if (prop != null)
			{
				var value = prop.GetValue(listView);
				if (value is T typed)
					return typed;
			}
			return default;
		}
	}

	/// <summary>
	/// Lightweight proxy that holds section/row indices for DataTemplate binding.
	/// </summary>
	sealed class CollectionViewItemProxy
	{
		public int Section { get; }
		public int Row { get; }

		public CollectionViewItemProxy(int section, int row)
		{
			Section = section;
			Row = row;
		}
	}
}
