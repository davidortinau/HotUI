using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Comet
{
	public class CometShell : View
	{
		private static CometShell _current;
		private static readonly Dictionary<string, Type> _routes = new Dictionary<string, Type>();
		private readonly Stack<string> _navigationStack = new Stack<string>();

		public static CometShell Current
		{
			get => _current;
			set => _current = value;
		}

		public List<ShellItem> Items { get; set; } = new List<ShellItem>();
		public ShellItem CurrentItem { get; set; }
		public ShellItem FlyoutHeader { get; set; }
		public bool FlyoutIsPresented { get; set; }

		public CometShell()
		{
			_current = this;
		}

		public static void RegisterRoute(string route, Type type)
		{
			if (!typeof(View).IsAssignableFrom(type))
				throw new ArgumentException($"Route type must inherit from View. Type: {type.Name}");

			_routes[route] = type;
		}

		public static void UnregisterRoute(string route)
		{
			_routes.Remove(route);
		}

		public static bool HasRoute(string route) => _routes.ContainsKey(route);

		public static Dictionary<string, string> ParseQueryString(string route)
		{
			var parts = route.Split('?');
			var queryParams = new Dictionary<string, string>();
			if (parts.Length > 1)
			{
				foreach (var param in parts[1].Split('&'))
				{
					var keyValue = param.Split('=');
					if (keyValue.Length == 2)
						queryParams[Uri.UnescapeDataString(keyValue[0])] = Uri.UnescapeDataString(keyValue[1]);
				}
			}
			return queryParams;
		}

		public async Task GoToAsync(string route)
		{
			// Handle back navigation
			if (route == "..")
			{
				if (_navigationStack.Count > 0)
				{
					_navigationStack.Pop();
					// Trigger navigation back
					await NavigateBack();
				}
				return;
			}

			// Parse query parameters
			var (routePath, queryParams) = ParseRouteInternal(route);

			if (!_routes.TryGetValue(routePath, out var pageType))
			{
				throw new InvalidOperationException($"Route '{routePath}' is not registered. Use CometShell.RegisterRoute() to register it.");
			}

			// Create instance
			var page = Activator.CreateInstance(pageType) as View;
			if (page == null)
				throw new InvalidOperationException($"Failed to create instance of {pageType.Name}");

			// Apply query parameters if the page implements IQueryAttributable
			if (page is IQueryAttributable queryAttributable && queryParams.Count > 0)
			{
				queryAttributable.ApplyQueryAttributes(queryParams);
			}

			_navigationStack.Push(routePath);

			// Perform the actual navigation
			await NavigateTo(page);
		}

		internal (string route, Dictionary<string, string> queryParams) ParseRouteInternal(string route)
		{
			var parts = route.Split('?');
			var routePath = parts[0];
			var queryParams = new Dictionary<string, string>();

			if (parts.Length > 1)
			{
				var query = parts[1];
				foreach (var param in query.Split('&'))
				{
					var keyValue = param.Split('=');
					if (keyValue.Length == 2)
					{
						queryParams[Uri.UnescapeDataString(keyValue[0])] = Uri.UnescapeDataString(keyValue[1]);
					}
				}
			}

			return (routePath, queryParams);
		}

		private async Task NavigateTo(View page)
		{
			// Use existing navigation infrastructure
			if (Navigation != null)
			{
				Navigation.Navigate(page);
			}
			else
			{
				// Fallback to modal presentation
				ModalView.Present(page);
			}

			await Task.CompletedTask;
		}

		private async Task NavigateBack()
		{
			if (Navigation != null)
			{
				Navigation.Pop();
			}
			else
			{
				ModalView.Dismiss();
			}

			await Task.CompletedTask;
		}
	}

	public interface IQueryAttributable
	{
		void ApplyQueryAttributes(Dictionary<string, string> query);
	}

	public class ShellItem
	{
		public string Title { get; set; }
		public string Route { get; set; }
		public List<ShellSection> Items { get; set; } = new List<ShellSection>();
		public View Icon { get; set; }
	}

	public class ShellSection
	{
		public string Title { get; set; }
		public string Route { get; set; }
		public List<ShellContent> Items { get; set; } = new List<ShellContent>();
		public View Icon { get; set; }
	}

	public class ShellContent
	{
		public string Title { get; set; }
		public string Route { get; set; }
		public View Content { get; set; }
		public Func<View> ContentTemplate { get; set; }
		public View Icon { get; set; }

		public View GetContent()
		{
			return Content ?? ContentTemplate?.Invoke();
		}
	}

	public static class ShellExtensions
	{
		public static Task GoToAsync(this View view, string route)
		{
			if (CometShell.Current != null)
				return CometShell.Current.GoToAsync(route);

			throw new InvalidOperationException("No Shell instance is currently active. Ensure a CometShell is set as Current.");
		}

		public static Task GoBackAsync(this View view)
		{
			if (CometShell.Current != null)
				return CometShell.Current.GoToAsync("..");

			throw new InvalidOperationException("No Shell instance is currently active.");
		}
	}
}
