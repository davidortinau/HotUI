using System;
using System.Collections.Generic;

namespace Comet
{
	public class DataTrigger
	{
		public string Property { get; set; }
		public object Value { get; set; }
		public List<Setter> Setters { get; } = new List<Setter>();

		internal View AssociatedObject { get; private set; }

		internal void Attach(View view)
		{
			AssociatedObject = view;
		}

		internal void Detach()
		{
			AssociatedObject = null;
		}
	}

	public class EventTrigger
	{
		public string Event { get; set; }
		public Action Action { get; set; }

		internal View AssociatedObject { get; private set; }

		internal void Attach(View view)
		{
			AssociatedObject = view;
		}

		internal void Detach()
		{
			AssociatedObject = null;
		}
	}
}
