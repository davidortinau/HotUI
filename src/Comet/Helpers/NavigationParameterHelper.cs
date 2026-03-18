using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Comet
{
	internal static class NavigationParameterHelper
	{
		public static void Apply(View view, object parameters, Dictionary<string, string> queryParameters = null)
		{
			if (view == null)
				return;

			if (parameters != null)
				TryApplyProps(view, parameters);

			if (view is IQueryAttributable queryAttributable)
			{
				var values = queryParameters;
				if ((values == null || values.Count == 0) && parameters != null)
					values = ToDictionary(parameters);

				if (values != null && values.Count > 0)
					queryAttributable.ApplyQueryAttributes(values);
			}
		}

		public static string BuildRoute(string route, object parameters)
		{
			if (string.IsNullOrWhiteSpace(route) || parameters == null)
				return route;

			var queryString = ToQueryString(parameters);
			if (string.IsNullOrEmpty(queryString))
				return route;

			var separator = route.Contains("?") ? "&" : "?";
			return $"{route}{separator}{queryString}";
		}

		public static Dictionary<string, string> ToDictionary(object parameters)
		{
			var values = new Dictionary<string, string>(StringComparer.Ordinal);

			if (parameters == null)
				return values;

			if (parameters is IEnumerable<KeyValuePair<string, string>> stringPairs)
			{
				foreach (var pair in stringPairs)
					Add(values, pair.Key, pair.Value);
				return values;
			}

			if (parameters is IEnumerable<KeyValuePair<string, object>> objectPairs)
			{
				foreach (var pair in objectPairs)
					Add(values, pair.Key, pair.Value);
				return values;
			}

			var parameterType = parameters.GetType();
			foreach (var property in parameterType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				if (!property.CanRead || property.GetIndexParameters().Length > 0)
					continue;

				Add(values, property.Name, property.GetValue(parameters));
			}

			return values;
		}

		public static string ToQueryString(object parameters)
		{
			var values = ToDictionary(parameters);
			if (values.Count == 0)
				return string.Empty;

			var encoded = new List<string>(values.Count);
			foreach (var pair in values)
			{
				encoded.Add($"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}");
			}

			return string.Join("&", encoded);
		}

		static void Add(IDictionary<string, string> values, string key, object value)
		{
			if (string.IsNullOrWhiteSpace(key) || value == null)
				return;

			var stringValue = ConvertToString(value);
			if (stringValue == null)
				return;

			values[key] = stringValue;
		}

		static string ConvertToString(object value)
		{
			if (value == null)
				return null;

			return value switch
			{
				string stringValue => stringValue,
				DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
				DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
				IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
				_ => value.ToString(),
			};
		}

		static bool TryApplyProps(View view, object parameters)
		{
			var propsProperty = view.GetType().GetProperty("Props", BindingFlags.Instance | BindingFlags.Public);
			if (propsProperty == null || !propsProperty.CanWrite)
				return false;

			if (!propsProperty.PropertyType.IsInstanceOfType(parameters))
				return false;

			propsProperty.SetValue(view, parameters);
			return true;
		}
	}
}
