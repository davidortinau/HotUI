using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Comet.Helpers;
using Comet.Reflection;

namespace Comet
{
	public interface IAutoImplemented
	{

	}

	public interface INotifyPropertyRead : INotifyPropertyChanged
	{
		event PropertyChangedEventHandler PropertyRead;
	}
	public class BindingObject : INotifyPropertyRead, IAutoImplemented
	{

		public event PropertyChangedEventHandler PropertyRead;
		public event PropertyChangedEventHandler PropertyChanged;

		internal protected Dictionary<string, object> dictionary = new Dictionary<string, object>();

		static readonly Dictionary<string, PropertyChangedEventArgs> _argsCache
			= new Dictionary<string, PropertyChangedEventArgs>();

		static PropertyChangedEventArgs GetCachedArgs(string propertyName)
		{
			if (!_argsCache.TryGetValue(propertyName, out var args))
			{
				args = new PropertyChangedEventArgs(propertyName);
				_argsCache[propertyName] = args;
			}
			return args;
		}

		protected T GetProperty<T>(T defaultValue = default, [CallerMemberName] string propertyName = "")
		{
			CallPropertyRead(propertyName);

			if (dictionary.TryGetValue(propertyName, out var val))
				return (T)val;
			return defaultValue;
		}

		internal virtual (bool hasValue, object value) GetValueInternal(string propertyName)
		{
			if (string.IsNullOrWhiteSpace(propertyName))
				return (false, null);
			var hasValue = dictionary.TryGetValue(propertyName, out var val);
			return (hasValue, val);
		}
		/// <summary>
		/// Returns true if the value changed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		protected bool SetProperty<T>(T value, [CallerMemberName] string propertyName = "")
		{
			if (dictionary.TryGetValue(propertyName, out object val))
			{
				if (val is T typedVal && EqualityComparer<T>.Default.Equals(typedVal, value))
					return false;
			}

			dictionary[propertyName] = value;

			CallPropertyChanged(propertyName, value);

			return true;
		}

		protected virtual void CallPropertyChanged(string propertyName, object value)
		{
			StateManager.OnPropertyChanged(this, propertyName, value);
			if (PropertyChanged != null)
				PropertyChanged.Invoke(this, GetCachedArgs(propertyName));
		}

		protected virtual void CallPropertyRead(string propertyName)
		{
			StateManager.OnPropertyRead(this, propertyName);
			if (PropertyRead != null)
				PropertyRead.Invoke(this, GetCachedArgs(propertyName));
		}

		internal bool SetPropertyInternal(object value, [CallerMemberName] string propertyName = "")
		{
			dictionary[propertyName] = value;
			CallPropertyChanged(propertyName, value);

			return true;
		}

	}


	public class BindingState
	{
		public IEnumerable<KeyValuePair<string, object>> ChangedProperties => changeDictionary;
		Dictionary<string, object> changeDictionary = new Dictionary<string, object>();

		public HashSet<(INotifyPropertyRead BindingObject, string PropertyName)> GlobalProperties { get; set; } = new HashSet<(INotifyPropertyRead BindingObject, string PropertyName)>();
		public Dictionary<(INotifyPropertyRead BindingObject, string PropertyName), HashSet<(string PropertyName, Binding Binding)>> ViewUpdateProperties = new Dictionary<(INotifyPropertyRead BindingObject, string PropertyName), HashSet<(string PropertyName, Binding Binding)>>();
		public void AddGlobalProperty((INotifyPropertyRead BindingObject, string PropertyName) property)
		{
			GlobalProperties.Add(property);
		}
		public void AddGlobalProperties(IReadOnlyList<(INotifyPropertyRead BindingObject, string PropertyName)> properties)
		{
			for (int i = 0; i < properties.Count; i++)
				AddGlobalProperty(properties[i]);
		}
		public void AddViewProperty((INotifyPropertyRead BindingObject, string PropertyName) property, string propertyName, Binding binding)
		{
			if (!ViewUpdateProperties.TryGetValue(property, out var actions))
				ViewUpdateProperties[property] = actions = new HashSet<(string PropertyName, Binding Binding)>();
			actions.Add((propertyName, binding));
		}

		public void AddViewProperty(IReadOnlyList<(INotifyPropertyRead BindingObject, string PropertyName)> properties, Binding binding, string propertyName)
		{
			foreach (var p in properties)
			{
				AddViewProperty(p, propertyName ?? p.PropertyName, binding);
			}
		}
		public void Clear()
		{
			GlobalProperties?.Clear();
			foreach (var key in ViewUpdateProperties)
			{
				key.Value.Clear();
			}
			ViewUpdateProperties.Clear();
		}

		protected void UpdatePropertyChangeProperty(View view, string fullProperty, object value)
		{
			if (view.Parent != null)
				UpdatePropertyChangeProperty(view.Parent, fullProperty, value);
			else
				view.GetState().changeDictionary[fullProperty] = value;
		}
		public bool UpdateValue(View view,(INotifyPropertyRead BindingObject, string PropertyName) property, string fullProperty, object value)
		{
			changeDictionary[fullProperty] = value;
			UpdatePropertyChangeProperty(view, fullProperty, value);
			if (ViewUpdateProperties.TryGetValue((property.BindingObject, property.PropertyName), out var bindings))
			{
				var count = bindings.Count;
				var bindingsArray = System.Buffers.ArrayPool<(string PropertyName, Binding Binding)>.Shared.Rent(count);
				bindings.CopyTo(bindingsArray);
				try
				{
					for (var i = 0; i < count; i++)
					{
						var binding = bindingsArray[i];
						binding.Binding.BindingValueChanged(property.BindingObject, binding.PropertyName, value);
					}
				}
				finally
				{
					System.Buffers.ArrayPool<(string PropertyName, Binding Binding)>.Shared.Return(bindingsArray, true);
				}
			}
			if (GlobalProperties.Contains(property))
			{
				return false;
			}
			return true;
		}
	}
}
