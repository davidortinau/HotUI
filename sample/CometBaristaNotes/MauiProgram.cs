using CometBaristaNotes.Services;
using Microsoft.Maui.Hosting;
using Syncfusion.Maui.Core.Hosting;

namespace CometBaristaNotes;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<BaristaNotesApp>();
		builder.UseCometHandlers();
		builder.ConfigureSyncfusionCore();
		builder.ConfigureFonts(fonts =>
		{
			fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			fonts.AddFont("Manrope-Regular.ttf", "Manrope");
			fonts.AddFont("Manrope-SemiBold.ttf", "ManropeSemibold");
			fonts.AddFont("MaterialSymbols.ttf", "MaterialIcons");
			fonts.AddFont("coffee-icons.ttf", "coffee-icons");
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
