using System;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

namespace Comet
{
    /// <summary>
    /// Embeds any Microsoft.Maui.Controls view inside a Comet MVU view tree.
    /// Uses a ContentViewHandler to host the MAUI control, bypassing Comet's
    /// CometViewHandler which only works with Comet View types.
    /// </summary>
    public class MauiViewHost : View, IContentView
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

        // IContentView implementation — delegates to the hosted view
        object IContentView.Content => HostedView;
        IView IContentView.PresentedContent => HostedView;
        Thickness IPadding.Padding => this.GetPadding();

        Size IContentView.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
        {
            if (HostedView != null)
                return HostedView.Measure(widthConstraint, heightConstraint);
            return Size.Zero;
        }

        Size IContentView.CrossPlatformArrange(Rect bounds)
        {
            if (HostedView != null)
            {
                HostedView.Arrange(bounds);
                return bounds.Size;
            }
            return bounds.Size;
        }

        public override void LayoutSubviews(Rect frame)
        {
            this.Frame = frame;
            HostedView?.Arrange(frame);
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
