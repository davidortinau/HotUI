using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet
{
	public class RootComponent
	{
		public string Selector { get; set; }
		public Type ComponentType { get; set; }
		public IDictionary<string, object> Parameters { get; set; }
	}

	public class BlazorWebView : WebView
	{
		public string HostPage { get; set; }
		public IList<RootComponent> RootComponents { get; } = new List<RootComponent>();
	}

	public class HybridWebView : WebView
	{
		public string DefaultFile { get; set; } = "index.html";

		public async Task<string> InvokeJavaScriptAsync(string methodName, params object[] args)
		{
			var script = $"{methodName}({string.Join(",", args.Select(a => System.Text.Json.JsonSerializer.Serialize(a)))})";
			return await ((Microsoft.Maui.IWebView)this).EvaluateJavaScriptAsync(script);
		}

		public event EventHandler<string> RawMessageReceived;

		public void SendRawMessage(string message)
		{
			RawMessageReceived?.Invoke(this, message);
		}
	}
}
