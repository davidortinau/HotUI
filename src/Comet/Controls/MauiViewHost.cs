using System;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Comet
{
	/// <summary>
	/// Embeds any Microsoft.Maui.Controls view (XAML or code-behind) inside a Comet MVU view tree.
	/// Uses a dedicated MauiViewHostHandler to create and host the platform view directly.
	/// 
	/// Usage:
	///   new MauiViewHost(new SfCircularChart { ... })
	///   new MauiViewHost(() => new MyExpensiveControl())
	///   new MauiViewHost(new MyChart()).Frame(width: 300, height: 200)
	/// </summary>
	public class MauiViewHost : View
	{
		private IView _hostedView;
		private Func<IView> _factory;
		private readonly object _lock = new object();

		public MauiViewHost(IView view)
		{
			_hostedView = view ?? throw new ArgumentNullException(nameof(view));
		}

		public MauiViewHost(Func<IView> factory)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}

		public IView HostedView
		{
			get
			{
				if (_hostedView == null && _factory != null)
				{
					lock (_lock)
					{
						if (_hostedView == null && _factory != null)
						{
							var factory = _factory;
							_factory = null; // Clear first to prevent retry on exception
							_hostedView = factory();
						}
					}
				}
				return _hostedView;
			}
		}

		public override Size GetDesiredSize(Size availableSize)
		{
			var frameConstraints = this.GetFrameConstraints();
			var margins = this.GetMargin();

			if (frameConstraints?.Height > 0 && frameConstraints?.Width > 0)
				return new Size(frameConstraints.Width.Value, frameConstraints.Height.Value);

			// Measure the hosted view directly — do NOT call ViewHandler.GetDesiredSize()
			// which would re-enter View.Measure → GetDesiredSize causing infinite recursion.
			Size ms;
			if (HostedView != null)
			{
				ms = HostedView.Measure(availableSize.Width, availableSize.Height);
			}
			else
			{
				ms = new Size(
					frameConstraints?.Width ?? availableSize.Width,
					frameConstraints?.Height ?? 44);
			}

			if (frameConstraints?.Width > 0)
				ms.Width = frameConstraints.Width.Value;
			if (frameConstraints?.Height > 0)
				ms.Height = frameConstraints.Height.Value;

			ms.Width += margins.HorizontalThickness;
			ms.Height += margins.VerticalThickness;
			MeasuredSize = ms;
			MeasurementValid = ViewHandler != null;
			return MeasuredSize;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_hostedView?.Handler is IElementHandler handler)
					handler.DisconnectHandler();
				if (_hostedView is IDisposable disposable)
					disposable.Dispose();
				_hostedView = null;
				_factory = null;
			}
			base.Dispose(disposing);
		}
	}
}
