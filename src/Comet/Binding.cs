using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Comet.Reflection;

namespace Comet
{
	public class Binding
	{
		public object Value { get; protected set; }

		public bool IsValue { get; internal set; }
		public bool IsFunc { get; internal set; }
		WeakReference _view;
		internal View View
		{
			get => _view?.Target as View;
			set => _view = new WeakReference(value);
		}
		WeakReference _boundFromView;
		internal View BoundFromView
		{
			get => _boundFromView?.Target as View;
			set => _boundFromView = new WeakReference(value);
		}

		public IReadOnlyList<(INotifyPropertyRead BindingObject, string PropertyName)> BoundProperties { get; protected set; }
		protected string PropertyName;
		internal bool IsDirty { get; set; }
		public virtual void BindingValueChanged(INotifyPropertyRead bindingObject, string propertyName, object value)
		{
			Value = value;
			View?.ViewPropertyChanged(propertyName, value);
		}
		/// <summary>
		/// Flushes a deferred binding update. Override in Binding&lt;T&gt; for Func re-evaluation.
		/// </summary>
		internal virtual void Flush() { }

	}

	public class Binding<T> : Binding
	{
		public Binding()
		{

		}
		public Binding(Func<T> getValue, Action<T> setValue)
		{
			Get = getValue;
			ProcessGetFunc();
			Set = setValue;
		}

		Func<T> Get { get; set; }
		Action<T> _set;
		bool _bindingStable;
		public Action<T> Set
		{
			get => _set ?? (_set = (v)=>CurrentValue = v);
			internal set => _set = value;
		}

		public T CurrentValue { get => Value == null ? default :  (T)Value; private set => Value = value; }

		public static implicit operator Binding<T>(T value)
		{
			var props = StateManager.EndProperty();
			if (props?.Count > 1)
			{
				StateManager.CurrentView.GetState().AddGlobalProperties(props);
			}

			else if (props?.Count == 1 && props[0].BindingObject is State<T> state)
			{
				return state;
			}
			return new Binding<T>()
			{
				IsValue = true,
				CurrentValue = value,
				BoundProperties = props,
				BoundFromView = StateManager.CurrentView
			};
		}

		public static implicit operator Binding<T>(Func<T> value)
			=> new Binding<T>(
				getValue: value,
				setValue: null);
		protected void ProcessGetFunc()
		{
			StateManager.StartProperty();
			var result = Get == null ? default : Get.Invoke();
			var props = StateManager.EndProperty();
			IsFunc = true;
			CurrentValue = result;
			BoundProperties = props;

			BoundFromView = StateManager.CurrentView;

		}


		public static implicit operator Binding<T>(State<T> state)
		{
			StateManager.StartProperty();
			var result = state.Value;
			var props = StateManager.EndProperty();


			var binding = new Binding<T>(
				getValue: () => state.Value,
				setValue: (v) => {
					state.Value = v;
				})
			{
				CurrentValue = result,
				BoundProperties = props,
				IsFunc = true,
			};
			return binding;
		}

		public static implicit operator T(Binding<T> value)
			=> value == null
			? default : value.CurrentValue;



		private static Func<object> ToGenericGetter(Func<T> getValue)
		{
			if (getValue != null)
				return () => getValue.Invoke();

			return null;
		}

		private static Action<object> ToGenericSetter(Action<T> setValue)
		{
			if (setValue != null)
				return (v) => setValue.Invoke((T)v);

			return null;
		}

		public void BindToProperty(View view, string property)
		{
			PropertyName = property;
			View = view;
			if (IsFunc && BoundProperties?.Count > 0)
			{
				StateManager.UpdateBinding(this, view);
				view.GetState().AddViewProperty(BoundProperties, this, property);
				return;
			}

			if (IsValue)
			{

				bool isGlobal = BoundProperties?.Count > 1;
				var propCount = BoundProperties?.Count ?? 0;
				if (propCount == 0)
					return;

				var prop = BoundProperties[0];
				if (BoundProperties?.Count == 1)
				{


					var stateValue = prop.BindingObject.GetPropertyValue(prop.PropertyName).Cast<T>();
					var old = StateManager.EndProperty();
					//1 to 1 binding!
					if (EqualityComparer<T>.Default.Equals(stateValue, CurrentValue))
					{
						Set = (v) => {
							prop.BindingObject.SetPropertyValue(prop.PropertyName, v);
							CurrentValue = v;
							//view?.BindingPropertyChanged(property, v);
						};
						StateManager.UpdateBinding(this, view);
						view.GetState().AddViewProperty(prop, property, this);
						Debug.WriteLine($"Databinding: {property} to {prop}");
					}
					else
					{
						var errorMessage = $"Warning: {property} is using formated Text. For performance reasons, please switch to a Lambda. i.e new Text(()=> \"Hello\")";
						if (Debugger.IsAttached)
						{
							Logger.Fatal(errorMessage);
						}

						Debug.WriteLine(errorMessage);
						isGlobal = true;
					}
				}
				else
				{
					var errorMessage = $"Warning: {property} is using Multiple state Variables. For performance reasons, please switch to a Lambda.";
					//if (Debugger.IsAttached)
					//{
					//    throw new Exception(errorMessage);
					//}
					isGlobal = true;
					Debug.WriteLine(errorMessage);
				}

				if (isGlobal)
				{
					StateManager.UpdateBinding(this, BoundFromView);
					BoundFromView.GetState().AddGlobalProperties(BoundProperties);
				}
				else
				{
					StateManager.UpdateBinding(this, BoundFromView);
				}
			}
		}
		public override void BindingValueChanged(INotifyPropertyRead bindingObject, string propertyName, object value)
		{
			// When batching, defer Func re-evaluation to avoid redundant work
			if (IsFunc && StateManager.IsBatching)
			{
				if (!IsDirty)
				{
					IsDirty = true;
					StateManager.AddDirtyBinding(this);
				}
				return;
			}
			EvaluateAndNotify(bindingObject, propertyName, value);
		}

		private void EvaluateAndNotify(INotifyPropertyRead bindingObject, string propertyName, object value)
		{
			var oldValue = CurrentValue;
			if (IsFunc)
			{
				if (_bindingStable)
				{
					// Fast path: skip property tracking — bindings haven't changed
					CurrentValue = Get == null ? default : Get.Invoke();
				}
				else
				{
					var oldProps = BoundProperties;
					StateManager.StartProperty();
					var result = Get == null ? default : Get.Invoke();
					var props = StateManager.EndProperty();
					CurrentValue = result;
					BoundProperties = props;
					if (ArePropertiesDifferent(BoundProperties, oldProps))
						BindToProperty(View, PropertyName);
					else
						_bindingStable = true;
				}
			}
			else
			{
				CurrentValue = Cast(value);
			}
			if (!(oldValue?.Equals(CurrentValue) ?? false))
				View?.ViewPropertyChanged(propertyName, CurrentValue);
		}

		private static bool ArePropertiesDifferent(IReadOnlyList<(INotifyPropertyRead BindingObject, string PropertyName)> props1, IReadOnlyList<(INotifyPropertyRead BindingObject, string PropertyName)> props2)
		{
			if (props1 == props2) return false;
			if (props1 == null || props2 == null) return true;
			if (props1.Count != props2.Count) return true;

			for (int i = 0; i < props1.Count; i++)
			{
				if (!props1[i].Equals(props2[i]))
					return true;
			}
			return false;
		}

		internal override void Flush()
		{
			if (!IsDirty) return;
			IsDirty = false;
			EvaluateAndNotify(null, PropertyName, null);
		}

		static T Cast(object value)
		{
			if (value is T v)
				return v;
			if (typeof(T) == typeof(string))
				return (T)(object)value?.ToString();
			var error = new InvalidCastException()
			{
				Data =
				{
					["Value"] = value,
					["T Type"] = typeof(T),
					["Value Type"] = value?.GetType(),
				}
			};
			Logger.Error(error, typeof(T), value);
			throw error;
		}
	}

	public static class BindingExtensions
	{
		public static T GetValueOrDefault<T>(this Binding<T> binding, T defaultValue = default)
		{
			if (binding.Value == null)
				return defaultValue;
			return binding.CurrentValue;
		}

		public static Binding<TTarget> Convert<TSource, TTarget>(this Binding<TSource> binding, IValueConverter converter, object parameter = null)
		{
			if (binding == null)
				throw new ArgumentNullException(nameof(binding));
			if (converter == null)
				throw new ArgumentNullException(nameof(converter));

			return new Binding<TTarget>(
				getValue: () => (TTarget)converter.Convert(binding.CurrentValue, typeof(TTarget), parameter, System.Globalization.CultureInfo.CurrentCulture),
				setValue: (v) => binding.Set((TSource)converter.ConvertBack(v, typeof(TSource), parameter, System.Globalization.CultureInfo.CurrentCulture))
			);
		}

		public static Binding<TTarget> Convert<TSource, TTarget>(this Binding<TSource> binding, Func<TSource, TTarget> convert, Func<TTarget, TSource> convertBack = null)
		{
			if (binding == null)
				throw new ArgumentNullException(nameof(binding));
			if (convert == null)
				throw new ArgumentNullException(nameof(convert));

			return new Binding<TTarget>(
				getValue: () => convert(binding.CurrentValue),
				setValue: convertBack != null ? (v) => binding.Set(convertBack(v)) : null
			);
		}
	}
}
