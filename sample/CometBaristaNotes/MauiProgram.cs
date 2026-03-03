using CometBaristaNotes.Services;
using Microsoft.Maui.Hosting;

namespace CometBaristaNotes;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<BaristaNotesApp>();
		builder.UseCometHandlers();
		builder.ConfigureFonts(fonts =>
		{
			fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
		});

		// Register singleton data store implementing all service interfaces
		var store = new InMemoryDataStore();
		builder.Services.AddSingleton<IShotService>(store);
		builder.Services.AddSingleton<IBeanService>(store);
		builder.Services.AddSingleton<IBagService>(store);
		builder.Services.AddSingleton<IEquipmentService>(store);
		builder.Services.AddSingleton<IUserProfileService>(store);
		builder.Services.AddSingleton<IRatingService>(store);

		return builder.Build();
	}
}
