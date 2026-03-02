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
	public class MauiViewHost : View, IReplaceableView, IContentView
	{
		private IView _hostedView;
		private Func<IView> _factory;

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
					_hostedView = _factory();
					_factory = null;
				}
				return _hostedView;
			}
		}

		IView IReplaceableView.ReplacedView => HostedView ?? this;

		// IContentView implementation
		object IContentView.Content => HostedView;
		IView IContentView.PresentedContent => HostedView;
		Size IContentView.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
			=> HostedView?.Measure(widthConstraint, heightConstraint) ?? Size.Zero;
		Size IContentView.CrossPlatformArrange(Rect bounds)
			=> HostedView?.Arrange(bounds) ?? Size.Zero;

		public override void LayoutSubviews(Rect frame)
		{
			this.Frame = frame;
			HostedView?.Arrange(frame);
		}

		public override Rect Frame
		{
			get => base.Frame;
			set
			{
				base.Frame = value;
				HostedView?.Handler?.PlatformArrange(value);
			}
		}

		public override Size GetDesiredSize(Size availableSize)
		{
			if (HostedView != null)
				return HostedView.Measure(availableSize.Width, availableSize.Height);
			return base.GetDesiredSize(availableSize);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_hostedView is IDisposable disposable)
					disposable.Dispose();
				_hostedView = null;
				_factory = null;
			}
			base.Dispose(disposing);
		}
	}
}
