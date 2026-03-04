using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Comet.Helpers;
using Comet.Internal;
using Comet.Reflection;
using Microsoft.Maui;
using Microsoft.Maui.Devices;

namespace Comet
{
	public static class StateManager
	{
		static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
		[ThreadStatic] static WeakStack<View> _viewStack;
		[ThreadStatic] static List<(INotifyPropertyRead bindingObject, string property)> _currentReadProperties;
		static WeakStack<View> GetCurrentViewStack() => _viewStack ??= new WeakStack<View>();
		static List<(INotifyPropertyRead bindingObject, string property)> GetCurrentReadProperties()
			=> _currentReadProperties ??= new List<(INotifyPropertyRead bindingObject, string property)>();
		public static View CurrentView => GetCurrentViewStack().Peek() ?? LastView?.Target as View;

		static WeakReference LastView;
		static Dictionary<string, List<INotifyPropertyRead>> ViewObjectMappings = new Dictionary<string, List<INotifyPropertyRead>>();
		static Dictionary<INotifyPropertyRead, HashSet<View>> NotifyToViewMappings = new Dictionary<INotifyPropertyRead, HashSet<View>>();
		static Dictionary<INotifyPropertyChanged, Dictionary<string, string>> ChildPropertyNamesMapping = new Dictionary<INotifyPropertyChanged, Dictionary<string, string>>();
		public static IMauiContext CurrentContext { get; private set; }

		static List<INotifyPropertyRead> MonitoredObjects = new List<INotifyPropertyRead>();

		// --- State batching ---
		static int _batchDepth;
		static readonly List<Binding> _dirtyBindings = new List<Binding>();
		static readonly HashSet<View> _viewsNeedingReload = new HashSet<View>();

		/// <summary>
		/// Returns true if state changes are being batched.
		/// During batching, Binding Func re-evaluations are deferred until EndBatch().
		/// </summary>
		public static bool IsBatching => _batchDepth > 0;

		/// <summary>
		/// Begins batching state changes. Multiple state mutations within a batch
		/// trigger only a single Binding re-evaluation per affected binding.
		/// Batches can be nested; only the outermost EndBatch() flushes.
		/// </summary>
		public static void BeginBatch()
		{
			_batchDepth++;
		}

		/// <summary>
		/// Ends the current batch. When the outermost batch ends, all deferred
		/// Binding re-evaluations execute and pending view reloads fire.
		/// </summary>
		public static void EndBatch()
		{
			if (--_batchDepth <= 0)
			{
				_batchDepth = 0;
				FlushBatch();
			}
		}

		internal static void AddDirtyBinding(Binding binding)
		{
			_dirtyBindings.Add(binding);
		}

		internal static void AddViewNeedingReload(View view)
		{
			_viewsNeedingReload.Add(view);
		}

		static void FlushBatch()
		{
			// Flush dirty bindings first (iterate then clear - no concurrent modification possible)
			if (_dirtyBindings.Count > 0)
			{
				for (int i = 0; i < _dirtyBindings.Count; i++)
					_dirtyBindings[i].Flush();
				_dirtyBindings.Clear();
			}

			// Then reload views that had global property changes
			if (_viewsNeedingReload.Count > 0)
			{
				foreach (var v in _viewsNeedingReload)
				{
					if (!v.IsDisposed)
						v.Reload();
				}
				_viewsNeedingReload.Clear();
			}
		}

		static T GetCurrent<T>(this Dictionary<Thread,T> dictionary) where T : new ()
		{
			_rwLock.EnterWriteLock();
			try
			{
				var thread = Thread.CurrentThread;
				if (dictionary.TryGetValue(thread, out var item))
					return item;
				return dictionary[thread] = new T();
			}
			finally
			{
				_rwLock.ExitWriteLock();
			}
		}

		[ThreadStatic] static bool _isTrackingProperties;
		public static bool IsBuilding => GetCurrentViewStack().Count > 0 || _isTrackingProperties;
		public static void ConstructingView(View view)
		{
			LastView = new WeakReference(view);

			var mappings = CheckForStateAttributes(view, view).ToList();
			if (mappings.Count > 0)
			{
				_rwLock.EnterWriteLock();
				try
				{
					ViewObjectMappings[view.Id] = mappings;
					foreach (var obj in mappings)
					{
						NotifyToViewMappings.GetOrCreateForKey(obj).Add(view);
					}
				}
				finally
				{
					_rwLock.ExitWriteLock();
				}
			}
			var currentReadProperies = GetCurrentReadProperties();
			if (currentReadProperies.Count > 0)
			{
				CurrentView.GetState().AddGlobalProperties(currentReadProperies);
			}
			currentReadProperies.Clear();

		}

		public static void MonitorListViewObject(View view, INotifyPropertyRead obj)
		{
			_rwLock.EnterWriteLock();
			try
			{
				ViewObjectMappings.GetOrCreateForKey(view.Id).Add(obj);
				NotifyToViewMappings.GetOrCreateForKey(obj).Add(view);
			}
			finally
			{
				_rwLock.ExitWriteLock();
			}
		}

		public static void Disposing(View view)
		{
			List<INotifyPropertyRead> toStopMonitoring = null;
			_rwLock.EnterWriteLock();
			try
			{
				if (ViewObjectMappings.TryGetValue(view.Id, out var mappings))
				{
					foreach (var obj in mappings)
					{
						if (NotifyToViewMappings.TryGetValue(obj, out var views))
						{
							views.Remove(view);
							if (views.Count == 0)
							{
								NotifyToViewMappings.Remove(obj);
								// Can't call StopMonitoring here (would recurse on lock)
								if (MonitoredObjects.Remove(obj))
								{
									toStopMonitoring ??= new List<INotifyPropertyRead>();
									toStopMonitoring.Add(obj);
								}
								ChildPropertyNamesMapping.Remove(obj);
							}
						}
					}
					ViewObjectMappings.Remove(view.Id);
				}
			}
			finally
			{
				_rwLock.ExitWriteLock();
			}
			// Unsubscribe events outside lock
			if (toStopMonitoring != null)
			{
				foreach (var obj in toStopMonitoring)
				{
					if (!(obj is IAutoImplemented))
					{
						obj.PropertyChanged -= Obj_PropertyChanged;
						obj.PropertyRead -= Obj_PropertyRead;
					}
				}
			}
		}

		public static void StartBuilding(View view)
		{
			if (view.ViewHandler?.MauiContext != null)
				CurrentContext = view.ViewHandler?.MauiContext;
			else if (view is IMauiContextHolder imvc && CurrentContext != imvc.MauiContext)
				CurrentContext = imvc.MauiContext;

			//TODO: Grab objects and add them to previous views globals
			var currentBuildingView = GetCurrentViewStack();
			currentBuildingView.Push(view);
			var currentReadProperies = GetCurrentReadProperties();
			if (currentReadProperies.Count > 0)
			{
				CurrentView.GetState().AddGlobalProperties(currentReadProperies);
			}
			currentReadProperies.Clear();
		}
		public static void EndBuilding(View view)
		{
			var currentBuildingView = GetCurrentViewStack();
			var v = currentBuildingView.Pop();
			Debug.Assert(v == view);
			if (currentBuildingView.Count == 0)
			{
				GetCurrentReadProperties().Clear();
			}
		}


		static Assembly CometAssembly = typeof(BindingObject).Assembly;
		public static void CheckBody(View view)
		{
			CheckForStateAttributes(view, view).ToList();
		}

		static IEnumerable<INotifyPropertyRead> CheckForStateAttributes(object obj, View view)
		{
			//if (hasChecked && obj == this)
			//    return;
			//hasChecked = obj == this;
			var type = obj.GetType();

			var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).
				Where(x => (x.FieldType.Assembly == CometAssembly && x.FieldType.Name == "State`1") || Attribute.IsDefined(x, typeof(StateAttribute))).ToList();


			if (fields.Any())
			{
				foreach (var field in fields)
				{
					if (!field.IsInitOnly)
					{
						throw new ReadonlyRequiresException(field.DeclaringType?.FullName, field.Name);
					}
					var fieldValue = field.GetValue(obj);
					var child = fieldValue as INotifyPropertyRead;
					if (child != null)
					{
						//If the view is null, this is a child propety for a binding object.
						//We will need to send its notification out for each view that monitors it.
						RegisterChild(view, child, field.Name);
						yield return child;
					}
				}
			}
		}

		public static void RegisterChild(View view, INotifyPropertyRead value, string fieldName)
		{
			bool needsMonitoring;
			_rwLock.EnterWriteLock();
			try
			{
				ChildPropertyNamesMapping.GetOrCreateForKey(value)[view?.Id ?? ""] = fieldName;
				needsMonitoring = !MonitoredObjects.Contains(value);
				if (needsMonitoring)
					MonitoredObjects.Add(value);
			}
			finally
			{
				_rwLock.ExitWriteLock();
			}
			if (needsMonitoring)
				StartMonitoringCore(value);
		}

		static public void StartMonitoring(INotifyPropertyRead obj)
		{
			_rwLock.EnterWriteLock();
			try
			{
				if (MonitoredObjects.Contains(obj))
					return;
				MonitoredObjects.Add(obj);
			}
			finally
			{
				_rwLock.ExitWriteLock();
			}
			StartMonitoringCore(obj);
		}

		static void StartMonitoringCore(INotifyPropertyRead obj)
		{
			CheckForStateAttributes(obj, null).ToList();

			if (!(obj is IAutoImplemented))
			{
				obj.PropertyChanged += Obj_PropertyChanged;
				obj.PropertyRead += Obj_PropertyRead;
			}
		}
		public static void StopMonitoring(INotifyPropertyRead obj)
		{
			bool wasTracked;
			_rwLock.EnterWriteLock();
			try
			{
				wasTracked = MonitoredObjects.Remove(obj);
			}
			finally
			{
				_rwLock.ExitWriteLock();
			}
			if (!wasTracked)
				return;

			if (!(obj is IAutoImplemented))
			{
				obj.PropertyChanged -= Obj_PropertyChanged;
				obj.PropertyRead -= Obj_PropertyRead;
			}
			_rwLock.EnterWriteLock();
			try
			{
				NotifyToViewMappings.Remove(obj);
				ChildPropertyNamesMapping.Remove(obj);
			}
			finally
			{
				_rwLock.ExitWriteLock();
			}
		}

		static void Obj_PropertyRead(object sender, PropertyChangedEventArgs e) => OnPropertyRead(sender, e.PropertyName);

		static void Obj_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (sender is BindingObject b)
			{
				// GetValueInternal returns (bool hasValue, object value) — unpack the tuple
				// instead of boxing it as the value parameter.
				var (_, value) = b.GetValueInternal(e.PropertyName);
				OnPropertyChanged(sender, e.PropertyName, value);
				return;
			}

			var propValue = sender.GetPropertyValue(e.PropertyName);
			OnPropertyChanged(sender, e.PropertyName, propValue);
		}
		public static void OnPropertyRead(object sender, string propertyName)
		{
			if (!IsBuilding)
				return;
			var currentReadProperies = GetCurrentReadProperties();
			currentReadProperies.Add((sender as INotifyPropertyRead, propertyName));
		}
		static readonly Dictionary<(string parent, string child), string> _propertyNameCache
			= new Dictionary<(string, string), string>();

		public static void OnPropertyChanged(object sender, string propertyName, object value)
		{
			OnPropertyChanged<object>(sender, propertyName, value);
		}

		public static void OnPropertyChanged<T>(object sender, string propertyName, T value)
		{
			if (value?.GetType() == typeof(View))
				return;
			if (value is INotifyPropertyRead iNotify)
				StartMonitoring(iNotify);
			var notify = sender as INotifyPropertyRead;
			if (notify == null)
				//throw new Exception("Error, this is null!!!");
				return;

			HashSet<View> views;
			Dictionary<string, string> mappings;
			View singleView = null;

			_rwLock.EnterReadLock();
			try
			{
				if (!NotifyToViewMappings.TryGetValue(notify, out views))
					return;
				if (views.Count == 0)
					return;
				ChildPropertyNamesMapping.TryGetValue(notify, out mappings);

				// Fast path for single-view: extract view inside same lock
				if (views.Count == 1)
				{
					using var enumerator = views.GetEnumerator();
					singleView = enumerator.MoveNext() ? enumerator.Current : null;
				}
			}
			finally
			{
				_rwLock.ExitReadLock();
			}

			if (singleView != null)
			{
				if (singleView.IsDisposed)
				{
					_rwLock.EnterWriteLock();
					try { views.Remove(singleView); }
					finally { _rwLock.ExitWriteLock(); }
					return;
				}
				string parentproperty = null;
				if (mappings != null && mappings.Count > 0 && !mappings.TryGetValue(singleView.Id, out parentproperty))
					parentproperty = mappings.First().Value;
				var prop = ResolvePropertyName(parentproperty, propertyName);
				singleView.BindingPropertyChanged(notify, propertyName, prop, value);
				return;
			}

			// Multi-view path: use ArrayPool
			View[] viewsCopy = null;
			int viewCount;

			_rwLock.EnterReadLock();
			try
			{
				viewCount = views.Count;
				viewsCopy = System.Buffers.ArrayPool<View>.Shared.Rent(viewCount);
				views.CopyTo(viewsCopy);
			}
			finally
			{
				_rwLock.ExitReadLock();
			}

			List<View> disposedViews = null;
			try
			{
				for (int i = 0; i < viewCount; i++)
				{
					var view = viewsCopy[i];
					if (view == null || view.IsDisposed)
					{
						disposedViews ??= new List<View>();
						disposedViews.Add(view);
						continue;
					}
					string parentproperty = null;
					if (mappings != null && mappings.Count > 0 && !mappings.TryGetValue(view.Id, out parentproperty))
					{
						parentproperty = mappings.First().Value;
					}
					var prop = ResolvePropertyName(parentproperty, propertyName);
					view.BindingPropertyChanged(notify, propertyName, prop, value);
				}
			}
			finally
			{
				if (viewsCopy != null)
					System.Buffers.ArrayPool<View>.Shared.Return(viewsCopy, true);
			}

			if (disposedViews?.Count > 0)
			{
				_rwLock.EnterWriteLock();
				try
				{
					if (NotifyToViewMappings.TryGetValue(notify, out var viewsForCleanup))
					{
						foreach (var view in disposedViews)
							viewsForCleanup.Remove(view);
					}
				}
				finally
				{
					_rwLock.ExitWriteLock();
				}
			}
		}

		static string ResolvePropertyName(string parentProperty, string propertyName)
		{
			if (string.IsNullOrWhiteSpace(parentProperty))
				return propertyName;
			var cacheKey = (parentProperty, propertyName);
			if (!_propertyNameCache.TryGetValue(cacheKey, out var prop))
			{
				prop = string.Concat(parentProperty, ".", propertyName);
				_propertyNameCache[cacheKey] = prop;
			}
			return prop;
		}




		internal static IReadOnlyList<(INotifyPropertyRead BindingObject, string PropertyName)> EndProperty()
		{
			_isTrackingProperties = false;
			var currentReadProperies = GetCurrentReadProperties();
			var count = currentReadProperies.Count;
			if (count == 0)
				return Array.Empty<(INotifyPropertyRead, string)>();

			if (count == 1)
			{
				var single = currentReadProperies[0];
				currentReadProperies.Clear();
				// Reuse a thread-static buffer for the single-element case
				var buf = _endPropertyBuffer ??= new List<(INotifyPropertyRead, string)>(1);
				buf.Clear();
				buf.Add(single);
				return buf;
			}

			// Multi-property: deduplicate in-place
			var seen = _endPropertySeen ??= new HashSet<(INotifyPropertyRead, string)>();
			seen.Clear();
			var result = new List<(INotifyPropertyRead, string)>(count);
			foreach (var prop in currentReadProperies)
			{
				if (seen.Add(prop))
					result.Add(prop);
			}
			currentReadProperies.Clear();
			return result;
		}

		[ThreadStatic] static List<(INotifyPropertyRead, string)> _endPropertyBuffer;
		[ThreadStatic] static HashSet<(INotifyPropertyRead, string)> _endPropertySeen;


		internal static void StartProperty()
		{
			_isTrackingProperties = true;
			var currentReadProperies = GetCurrentReadProperties();
			if (currentReadProperies.Count > 0)
			{
				CurrentView.GetState()?.AddGlobalProperties(currentReadProperies);
			}
			currentReadProperies.Clear();
		}


		internal static void UpdateBinding(Binding binding, View view)
		{
			_rwLock.EnterWriteLock();
			try
			{
				foreach (var prop in binding.BoundProperties)
				{
					NotifyToViewMappings.GetOrCreateForKey(prop.BindingObject).Add(view);
				}
			}
			finally
			{
				_rwLock.ExitWriteLock();
			}
		}

		internal static void ListenToEnvironment(View view)
		{
			_rwLock.EnterWriteLock();
			try
			{
				NotifyToViewMappings.GetOrCreateForKey(View.Environment).Add(view);
			}
			finally
			{
				_rwLock.ExitWriteLock();
			}
		}
	}
}