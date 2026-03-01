using System;
using System.Net;
using Microsoft.Maui;

namespace Comet
{
	class CometHtmlWebViewSource : IWebViewSource
	{
		public string Html { get; set; }
		public void Load(IWebViewDelegate webViewDelegate) => webViewDelegate.LoadHtml(Html, null);
	}

	class CometUrlWebViewSource : IWebViewSource
	{
		public string Url { get; set; }
		public void Load(IWebViewDelegate webViewDelegate) => webViewDelegate.LoadUrl(Url);
	}

	public class WebView : View, IWebView
	{
		Binding<string> html;
		public Binding<string> Html
		{
			get => html;
			set => this.SetBindingValue(ref html, value);
		}

		Binding<string> source;
		public Binding<string> Source
		{
			get => source;
			set => this.SetBindingValue(ref source, value);
		}

		IWebViewSource IWebView.Source
		{
			get
			{
				var src = Source?.CurrentValue;
				var htm = Html?.CurrentValue;
				if (!string.IsNullOrEmpty(htm))
					return new CometHtmlWebViewSource { Html = htm };
				if (!string.IsNullOrEmpty(src))
					return new CometUrlWebViewSource { Url = src };
				return null;
			}
		}

		bool IWebView.CanGoBack { get; set; }
		bool IWebView.CanGoForward { get; set; }
		string IWebView.UserAgent { get; set; }
		CookieContainer IWebView.Cookies => new CookieContainer();

		void IWebView.GoBack() { }
		void IWebView.GoForward() { }
		void IWebView.Reload() { }
		void IWebView.Eval(string script) { }
		Task<string> IWebView.EvaluateJavaScriptAsync(string script) => Task.FromResult<string>(null);
		bool IWebView.Navigating(WebNavigationEvent evnt, string url) => true;
		void IWebView.Navigated(WebNavigationEvent evnt, string url, WebNavigationResult result) { }
	}
}
