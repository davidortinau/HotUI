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
		static readonly object _lock = new object();
		static readonly Dictionary<Thread, WeakStack<View>> ViewsByThread = new Dictionary<Thread, WeakStack<View>>();
		static readonly Dictionary<Thread, List<(INotifyPropertyRead bindingObject, string property)>> CurrentReadProperiesByThread = new Dictionary<Thread, List<(INotifyPropertyRead bindingObject, string property)>>();
		public static View CurrentView => ViewsByThread.GetCurrent().Peek() ?? LastView?.Target as View;

		static WeakReference LastView;
		static Dictionary<string, List<INotifyPropertyRead>> ViewObjectMappings = new Dictionary<string, List<INotifyPropertyRead>>();
		static Dictionary<INotifyPropertyRead, HashSet<View>> NotifyToViewMappings = new Dictionary<INotifyPropertyRead, HashSet<View>>();
		static Dictionary<INotifyPropertyChanged, Dictionary<string, string>> ChildPropertyNamesMapping = new Dictionary<INotifyPropertyChanged, Dictionary<string, string>>();
		public static IMauiContext CurrentContext { get; private set; }

		static List<INotifyPropertyRead> MonitoredObjects = new List<INotifyPropertyRead>();

		static T GetCurrent<T>(this Dictionary<Thread,T> dictionary) where T : new ()
		{
			lock (_lock)
			{
				var thread = Thread.CurrentThread;
				if (dictionary.TryGetValue(thread, out var item))
					return item;
				return dictionary[thread] = new T();
			}
		}

		public static bool IsBuilding => isBuilding;
		static bool isBuilding = false;
		public static void ConstructingView(View view)
		{
			LastView = new WeakReference(view);

			var mappings = CheckForStateAttributes(view, view).ToList();
			if (mappings.Any())
			{
				lock (_lock)
				{
					ViewObjectMappings[view.Id] = mappings;
					foreach (var obj in mappings)
					{
						NotifyToViewMappings.GetOrCreateForKey(obj).Add(view);
					}
				}
			}
			var currentReadProperies = CurrentReadProperiesByThread.GetCurrent();
			if (currentReadProperies.Any())
			{
				//TODO: Change this to object and property!!!
				CurrentView.GetState().AddGlobalProperties(currentReadProperies);
			}
			currentReadProperies.Clear();

		}

		public static void MonitorListViewObject(View view, INotifyPropertyRead obj)
		{
			lock (_lock)
			{
				ViewObjectMappings.GetOrCreateForKey(view.Id).Add(obj);
				NotifyToViewMappings.GetOrCreateForKey(obj).Add(view);
			}
		}

		public static void Disposing(View view)
		{
			lock (_lock)
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
								StopMonitoring(obj);
							}
						}
					}
					ViewObjectMappings.Remove(view.Id);
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
			var currentBuildingView = ViewsByThread.GetCurrent();
			currentBuildingView.Push(view);
			isBuilding = true;
			var currentReadProperies = CurrentReadProperiesByThread.GetCurrent();
			if (currentReadProperies.Any())
			{
				//TODO: Change this to object and property!!!
				CurrentView.GetState().AddGlobalProperties(currentReadProperies);
			}
			currentReadProperies.Clear();
		}
		public static void EndBuilding(View view)
		{
			var currentBuildingView = ViewsByThread.GetCurrent();
			var v = currentBuildingView.Pop();
			Debug.Assert(v == view);
			isBuilding = currentBuildingView.Count != 0;
			if (!isBuilding)
			{
				var thread = Thread.CurrentThread;
				ViewsByThread.Remove(thread);
				CurrentReadProperiesByThread.Remove(thread);
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
			lock (_lock)
			{
				ChildPropertyNamesMapping.GetOrCreateForKey(value)[view?.Id ?? ""] = fieldName;
				if (!MonitoredObjects.Contains(value))
				{
					StartMonitoring(value);
				}
			}
		}

		static public void StartMonitoring(INotifyPropertyRead obj)
		{
			lock (_lock)
			{
				if (MonitoredObjects.Contains(obj))
					return;
				MonitoredObjects.Add(obj);
			}
			CheckForStateAttributes(obj, null).ToList();

			if (!(obj is IAutoImplemented))
			{
				obj.PropertyChanged += Obj_PropertyChanged;
				obj.PropertyRead += Obj_PropertyRead;
			}
		}
		public static void StopMonitoring(INotifyPropertyRead obj)
		{
			lock (_lock)
			{
				if (!MonitoredObjects.Contains(obj))
					return;
				MonitoredObjects.Remove(obj);
			}
			if (!(obj is IAutoImplemented))
			{
				obj.PropertyChanged -= Obj_PropertyChanged;
				obj.PropertyRead -= Obj_PropertyRead;
			}
			lock (_lock)
			{
				NotifyToViewMappings.Remove(obj);
				ChildPropertyNamesMapping.Remove(obj);
			}
		}

		static void Obj_PropertyRead(object sender, PropertyChangedEventArgs e) => OnPropertyRead(sender, e.PropertyName);

		static void Obj_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (sender is BindingObject b)
			{
				OnPropertyChanged(sender, e.PropertyName, b.GetValueInternal(e.PropertyName));
				return;
			}

			var value = sender.GetPropertyValue(e.PropertyName);
			OnPropertyChanged(sender, e.PropertyName, value);
		}
		public static void OnPropertyRead(object sender, string propertyName)
		{
			if (!isBuilding)
				return;
			var currentReadProperies = CurrentReadProperiesByThread.GetCurrent();
			currentReadProperies.Add((sender as INotifyPropertyRead, propertyName));
		}
		public static  void OnPropertyChanged(object sender, string propertyName, object value)
		{
			if (value?.GetType() == typeof(View))
				return;
			if (value is INotifyPropertyRead iNotify)
				StartMonitoring(iNotify);
			var notify = sender as INotifyPropertyRead;
			if (notify == null)
				throw new Exception("Error, this is null!!!");

			HashSet<View> views;
			lock (_lock)
			{
				if (!NotifyToViewMappings.TryGetValue(notify, out views))
					return;
			}

			List<View> viewsCopy;
			lock (_lock)
			{
				if (!views.Any())
				{
					Console.WriteLine("I think this means it is a child BindingObject");
					return;
				}
				viewsCopy = views.ToList();
			}

			Dictionary<string, string> mappings;
			lock (_lock)
			{
				ChildPropertyNamesMapping.TryGetValue(notify, out mappings);
			}
			List<View> disposedViews = new List<View>();
			viewsCopy.ForEach((view) => {
				if (view == null || view.IsDisposed)
				{
					disposedViews.Add(view);
					//Cleanup this View
					return;
				}
				string parentproperty = null;
				if (!mappings?.TryGetValue(view.Id, out parentproperty) ?? false && (mappings?.Count ?? 0) > 0)
				{
					parentproperty ??= mappings?.First().Value;
				}
				var prop = string.IsNullOrWhiteSpace(parentproperty) ? propertyName : $"{parentproperty}.{propertyName}";
				//TODO: Change this to use notify and property name
				ThreadHelper.RunOnMainThread(()=>
				view.BindingPropertyChanged(notify, propertyName, prop, value));

				//TODO: Make sure we handle nested binding objects

				/*
                 public class Foo : BindingObject
                 {
                     public Bar Bar {get;set;}
                 }

                 public class Bar : BindingObject
                 {
                     public int Count{get;set;}
                 }

                 //Binding to foo.Bar.Count works when foo.Bar.Count ++;

                */

			});

			foreach (var view in disposedViews)
			{
				lock (_lock)
				{
					views.Remove(view);
				}
			}

			//var first = childrenProperty.FirstOrDefault(x => x.Key.Target == sender);
			//string parentproperty = first.Value;
			//var prop = string.IsNullOrWhiteSpace(parentproperty) ? propertyName : $"{parentproperty}.{propertyName}";
			//changeDictionary[prop] = value;
			//pendingUpdates.Add((prop, value));
			//if (!isUpdating)
			//{
			//    EndUpdate();
			//}

		}




		internal static IReadOnlyList<(INotifyPropertyRead BindingObject, string PropertyName)> EndProperty()
		{

			var currentReadProperies = CurrentReadProperiesByThread.GetCurrent();
			var changed = currentReadProperies.ToList().Distinct().ToList();
			currentReadProperies.Clear();
			return changed;

		}


		internal static void StartProperty()
		{
			isBuilding = true;
			var currentReadProperies = CurrentReadProperiesByThread.GetCurrent();
			if (currentReadProperies.Any())
			{
				CurrentView.GetState()?.AddGlobalProperties(currentReadProperies);
			}
			currentReadProperies.Clear();
		}


		internal static void UpdateBinding(Binding binding, View view)
		{
			lock (_lock)
			{
				foreach (var prop in binding.BoundProperties)
				{
					NotifyToViewMappings.GetOrCreateForKey(prop.BindingObject).Add(view);
				}
			}
		}

		internal static void ListenToEnvironment(View view)
		{
			lock (_lock)
			{
				NotifyToViewMappings.GetOrCreateForKey(View.Environment).Add(view);
			}
		}
	}
}
