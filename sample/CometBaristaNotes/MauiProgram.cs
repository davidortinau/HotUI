using CometBaristaNotes.Services;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Syncfusion.Maui.Core.Hosting;
using UXDivers.Popups.Maui;

namespace CometBaristaNotes;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<BaristaNotesApp>();
		builder.UseCometHandlers();
		builder.ConfigureSyncfusionCore();
		builder.UseUXDiversPopups();

		// Add UXDivers popup theme resources
		builder.ConfigureLifecycleEvents(events =>
		{
#if IOS || MACCATALYST
			events.AddiOS(ios => ios.FinishedLaunching((app, options) =>
			{
				var mauiApp = Microsoft.Maui.Controls.Application.Current;
				if (mauiApp != null)
				{
					mauiApp.Resources.MergedDictionaries.Add(new UXDivers.Popups.Maui.Controls.DarkTheme());
					mauiApp.Resources.MergedDictionaries.Add(new UXDivers.Popups.Maui.Controls.PopupStyles());
				}

				return true;
			}));
#endif
		});

		// Configure iOS navigation bar: large titles + matching background color
		builder.ConfigureMauiHandlers(handlers =>
		{
#if IOS || MACCATALYST
			// Use compatibility renderer for Shell to enable PrefersLargeTitles
			handlers.AddHandler(typeof(Microsoft.Maui.Controls.Shell),
				typeof(CometBaristaNotes.Platforms.iOS.CustomShellRenderer));
#endif
		});

		builder.ConfigureFonts(fonts =>
		{
			fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			fonts.AddFont("Manrope-Regular.ttf", "Manrope");
			fonts.AddFont("Manrope-SemiBold.ttf", "ManropeSemibold");
			fonts.AddFont("MaterialSymbols.ttf", "MaterialIcons");
			fonts.AddFont("coffee-icons.ttf", "coffee-icons");
		});

		// Register singleton data store with SQLite persistence
		var store = new SqliteDataStore();
		builder.Services.AddSingleton<IShotService>(store);
		builder.Services.AddSingleton<IBeanService>(store);
		builder.Services.AddSingleton<IBagService>(store);
		builder.Services.AddSingleton<IEquipmentService>(store);
		builder.Services.AddSingleton<IUserProfileService>(store);
		builder.Services.AddSingleton<IRatingService>(store);

		// Alias so pages using InMemoryDataStore.Instance keep working
		InMemoryDataStore.Instance = store;

		// Feedback, theme, and data change notification services
		builder.Services.AddSingleton<IFeedbackService, FeedbackService>();
		builder.Services.AddSingleton<IThemeService, ThemeService>();
		var notifier = new DataChangeNotifier();
		builder.Services.AddSingleton<IDataChangeNotifier>(notifier);
		store.DataChangeNotifier = notifier;

		// Register AI services
		builder.Services.AddSingleton<IAIAdviceService, MockAIAdviceService>();
		builder.Services.AddSingleton<IVisionService, MockVisionService>();

		// Register navigation and voice command services
		builder.Services.AddSingleton<INavigationRegistry, NavigationRegistry>();
		builder.Services.AddSingleton<IVoiceCommandService, MockVoiceCommandService>();
		builder.Services.AddSingleton<ISpeechRecognitionService, MockSpeechRecognitionService>();

		var app = builder.Build();
		ServiceHelper.Services = app.Services;
		return app;
	}
}
