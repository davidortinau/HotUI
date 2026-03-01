using System;
using System.Collections.Generic;
using Microsoft.Maui;

namespace Comet
{
	// MenuBar is a desktop menu bar control
	// Simplified implementation - handlers will need platform-specific implementation
	public class MenuBar : View
	{
		private readonly List<MenuBarItem> _items = new List<MenuBarItem>();

		public void Add(MenuBarItem item)
		{
			_items.Add(item);
		}

		public IReadOnlyList<MenuBarItem> Items => _items;
	}

	public class MenuBarItem : View
	{
		private readonly List<MenuElement> _items = new List<MenuElement>();

		public string Text { get; set; }
		public bool IsEnabled { get; set; } = true;

		public void Add(MenuElement item)
		{
			_items.Add(item);
		}

		public IReadOnlyList<MenuElement> Items => _items;
	}

	public class MenuElement : View
	{
		public string Text { get; set; }
		public bool IsEnabled { get; set; } = true;
		public Action Clicked { get; set; }

		public void OnClicked()
		{
			Clicked?.Invoke();
		}
	}

	public class MenuFlyoutItem : View
	{
		public string Text { get; set; }
		public Action Clicked { get; set; }
		public bool IsEnabled { get; set; } = true;

		public void OnClicked()
		{
			Clicked?.Invoke();
		}
	}

	public class MenuFlyoutSubItem : MenuFlyoutItem
	{
		private readonly List<MenuElement> _items = new List<MenuElement>();

		public void Add(MenuElement item)
		{
			_items.Add(item);
		}

		public IReadOnlyList<MenuElement> Items => _items;
	}

	public class MenuFlyoutSeparator : View
	{
	}
}
