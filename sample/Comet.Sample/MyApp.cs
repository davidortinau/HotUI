using System;
using System.Linq;
using Comet.Graphics;
using Comet.Samples.Models;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using static Comet.CometControls;

namespace Comet.Samples
{
	public class MyApp : CometApp
	{
		[Body]
		View view() => new MainPage();

		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();

#if DEBUG
			builder.UseCometSampleDebugHost<MyApp>();
#else
			builder.UseCometApp<MyApp>();
#endif

			builder.ConfigureFonts(fonts => {
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});
#if DEBUG
			builder.EnableHotReload();
			builder.EnableSampleRuntimeDebugging();
#endif

			return builder.Build();
		}
	}
}
