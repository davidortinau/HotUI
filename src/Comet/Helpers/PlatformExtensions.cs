using System;
using System.Collections.Generic;
using Microsoft.Maui.Devices;

namespace Comet
{
	public class OnPlatform<T>
	{
		public T Default { get; set; }
		public T iOS { get; set; }
		public T Android { get; set; }
		public T WinUI { get; set; }
		public T MacCatalyst { get; set; }

		public static implicit operator T(OnPlatform<T> value)
		{
			if (value == null)
				return default;
			return value.GetValue();
		}

		public T GetValue()
		{
#if IOS
			if (!EqualityComparer<T>.Default.Equals(iOS, default))
				return iOS;
#elif ANDROID
			if (!EqualityComparer<T>.Default.Equals(Android, default))
				return Android;
#elif WINDOWS
			if (!EqualityComparer<T>.Default.Equals(WinUI, default))
				return WinUI;
#elif MACCATALYST
			if (!EqualityComparer<T>.Default.Equals(MacCatalyst, default))
				return MacCatalyst;
#endif
			return Default;
		}
	}

	public class OnIdiom<T>
	{
		public T Default { get; set; }
		public T Phone { get; set; }
		public T Tablet { get; set; }
		public T Desktop { get; set; }

		public static implicit operator T(OnIdiom<T> value)
		{
			if (value == null)
				return default;
			return value.GetValue();
		}

		public T GetValue()
		{
			var idiom = DeviceInfo.Idiom;

			if (idiom == DeviceIdiom.Phone && !EqualityComparer<T>.Default.Equals(Phone, default))
				return Phone;

			if (idiom == DeviceIdiom.Tablet && !EqualityComparer<T>.Default.Equals(Tablet, default))
				return Tablet;

			if (idiom == DeviceIdiom.Desktop && !EqualityComparer<T>.Default.Equals(Desktop, default))
				return Desktop;

			return Default;
		}
	}
}
