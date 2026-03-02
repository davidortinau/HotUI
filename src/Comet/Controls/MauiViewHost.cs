using System;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Comet
{
	/// <summary>
	/// Embeds any Microsoft.Maui.Controls view (XAML or code-behind) inside a Comet MVU view tree.
	/// 
	/// Usage:
	///   // Embed a XAML-defined ContentView
	///   new MauiViewHost(new MyXamlControl())
	///   
	///   // Embed a Syncfusion chart or any third-party MAUI control
	///   new MauiViewHost(new SfCircularChart { ... })
	///   
	///   // Embed with factory for lazy creation
	///   new MauiViewHost(() => new MyExpensiveControl())
	///   
	///   // Use fluent API
	///   new MauiViewHost(new MyChart()).Frame(width: 300, height: 200)
	/// </summary>
	public class MauiViewHost : View, IReplaceableView
	{
		private IView _hostedView;
		private Func<IView> _factory;

		/// <summary>
		/// Create a host for an existing MAUI Controls view.
		/// </summary>
		public MauiViewHost(IView view)
		{
			_hostedView = view ?? throw new ArgumentNullException(nameof(view));
		}

		/// <summary>
		/// Create a host with a factory that lazily creates the MAUI view.
		/// Useful for expensive controls that should only be created when needed.
		/// </summary>
		public MauiViewHost(Func<IView> factory)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}

		/// <summary>
		/// The hosted MAUI view. Created lazily if a factory was provided.
		/// </summary>
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

		/// <summary>
		/// The replaceable view delegates to the hosted MAUI view,
		/// allowing the handler system to render it directly.
		/// </summary>
		IView IReplaceableView.ReplacedView => HostedView ?? this;

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
