using System;
using System.Collections.Generic;

namespace Comet
{
	public class ResourceDictionary : Dictionary<string, object>
	{
		public ResourceDictionary MergedWith { get; set; }
		public IList<ResourceDictionary> MergedDictionaries { get; } = new List<ResourceDictionary>();

		public bool TryGetResource(string key, out object value)
		{
			if (TryGetValue(key, out value))
				return true;

			foreach (var merged in MergedDictionaries)
			{
				if (merged.TryGetResource(key, out value))
					return true;
			}

			if (MergedWith != null)
				return MergedWith.TryGetResource(key, out value);

			value = null;
			return false;
		}
	}
}
