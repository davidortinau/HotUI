using System;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Comet
{
	public class BoxView : View
	{
		public BoxView()
		{
		}

		public BoxView(Binding<Color> color)
		{
			Color = color;
		}

		public BoxView(Func<Color> color) : this((Binding<Color>)color)
		{
		}

		private Binding<Color> _color;
		public Binding<Color> Color
		{
			get => _color;
			set => this.SetBindingValue(ref _color, value);
		}

		private Binding<CornerRadius> _cornerRadius;
		public Binding<CornerRadius> CornerRadius
		{
			get => _cornerRadius;
			set => this.SetBindingValue(ref _cornerRadius, value);
		}
	}
}
