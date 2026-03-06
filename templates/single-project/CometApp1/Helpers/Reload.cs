#nullable enable
//-:cnd
#if DEBUG
using Microsoft.Maui.Hosting;

namespace Comet
{
	/// <summary>
	/// Hot Reload is natively supported via MetadataUpdateHandler in the Comet library.
	/// No additional setup is required — .NET Hot Reload (dotnet watch, VS Code, Visual Studio)
	/// automatically triggers Comet's view diff and state transfer pipeline.
	///
	/// This extension method is kept for backward compatibility but is a no-op.
	/// </summary>
	public static partial class Reload
	{
		public static MauiAppBuilder EnableHotReload(this MauiAppBuilder builder,
			string? ideIp = null, int idePort = 9988)
		{
			// MetadataUpdateHandler in Comet handles hot reload automatically.
			// No explicit wiring needed.
			return builder;
		}
	}
}
#endif
//+:cnd