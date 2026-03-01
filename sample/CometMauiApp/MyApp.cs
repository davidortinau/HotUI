using Comet;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;

namespace CometMauiApp
{
	public class MyApp : CometApp
	{
		[Body]
		View view() => new MainPage();

		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder.UseCometApp<MyApp>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});

#if DEBUG
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}
