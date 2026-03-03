using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comet.Reflection;

namespace Comet
{

	public sealed class State<T> : BindingObject
	{
		T _value;
		bool _hasValue;
		static readonly string ValuePropertyName = "Value";

		public State(T value)
		{
			_value = value;
			_hasValue = true;
			dictionary[ValuePropertyName] = value;
		}

		public State()
		{

		}

		public T Value
		{
			get
			{
				CallPropertyRead(ValuePropertyName);
				return _hasValue ? _value : default;
			}
			set
			{
				// Fast typed equality check — no dictionary lookup, no boxing
				if (_hasValue && EqualityComparer<T>.Default.Equals(_value, value))
					return;

				_value = value;
				_hasValue = true;

				CallPropertyChanged(ValuePropertyName, value);
				ValueChanged?.Invoke(value);
			}
		}

		/// <summary>
		/// Override to return typed value without dictionary lookup.
		/// </summary>
		internal override (bool hasValue, object value) GetValueInternal(string propertyName)
		{
			if (propertyName == ValuePropertyName)
				return (_hasValue, _value);
			return base.GetValueInternal(propertyName);
		}

		public static implicit operator T(State<T> state) => state.Value;
		public static implicit operator Action<T>(State<T> state) => value => state.Value = value;
		public static implicit operator State<T>(T value) => new State<T>(value);

		public override string ToString() => Value?.ToString();

		public Action<T> ValueChanged { get; set; }

	}

	public class StateBuilder : IDisposable
	{
		public StateBuilder(View view)
		{
			View = view;
			StateManager.StartBuilding(view);
		}

		public View View { get; private set; }

		public void Dispose()
		{
			StateManager.EndBuilding(View);
			View = null;
		}
	}


	//[Serializable]
	//public class State : BindingObjectManager {
	//	public State()
	//	{

	//	}
	//	internal object GetValue (string property)
	//	{
	//		return parent?.GetPropertyValue(property) ?? this.GetPropertyValue (property);
	//	}

	//       internal void SetChildrenValue<T>(string property, T value)
	//       {
	//           parent?.SetDeepPropertyValue(property, value);
	//           parent?.BindingPropertyChanged(property, value);
	//       }
	//   }
}
