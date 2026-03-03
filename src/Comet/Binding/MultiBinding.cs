using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Comet
{
	public class MultiBinding<TValue> : Binding<TValue>
	{
		private readonly Binding[] _bindings;
		private readonly Func<object[], TValue> _converter;
		private readonly IMultiValueConverter _multiValueConverter;
		private readonly object _converterParameter;

		public MultiBinding(
			Func<object[], TValue> converter,
			params Binding[] bindings)
		{
			_bindings = bindings ?? throw new ArgumentNullException(nameof(bindings));
			_converter = converter ?? throw new ArgumentNullException(nameof(converter));
			Initialize();
		}

		public MultiBinding(
			IMultiValueConverter converter,
			object converterParameter = null,
			params Binding[] bindings)
		{
			_bindings = bindings ?? throw new ArgumentNullException(nameof(bindings));
			_multiValueConverter = converter ?? throw new ArgumentNullException(nameof(converter));
			_converterParameter = converterParameter;
			Initialize();
		}

		private void Initialize()
		{
			IsFunc = true;
			CurrentValue = Evaluate();

			var allProps = new List<(INotifyPropertyRead BindingObject, string PropertyName)>();
			foreach (var binding in _bindings)
			{
				if (binding.BoundProperties != null)
				{
					foreach (var prop in binding.BoundProperties)
					{
						if (!allProps.Contains(prop))
							allProps.Add(prop);
					}
				}
			}
			BoundProperties = allProps;
		}

		private new TValue CurrentValue
		{
			get => Value == null ? default : (TValue)Value;
			set => Value = value;
		}

		private TValue Evaluate()
		{
			var values = _bindings.Select(b => b.Value).ToArray();

			if (_converter != null)
				return _converter(values);

			var result = _multiValueConverter.Convert(values, typeof(TValue), _converterParameter, CultureInfo.CurrentUICulture);
			if (result == null)
				return default;
			return (TValue)result;
		}

		public override void BindingValueChanged(INotifyPropertyRead bindingObject, string propertyName, object value)
		{
			// Update the source binding that changed
			foreach (var binding in _bindings)
			{
				if (binding.BoundProperties == null)
					continue;
				foreach (var prop in binding.BoundProperties)
				{
					if (prop.BindingObject == bindingObject && prop.PropertyName == propertyName)
					{
						binding.BindingValueChanged(bindingObject, propertyName, value);
						break;
					}
				}
			}

			// Re-evaluate the combined value
			var newValue = Evaluate();
			base.BindingValueChanged(bindingObject, propertyName, newValue);
		}
	}
}
