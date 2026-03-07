using BaristaNotes.Styles;
using CometBaristaNotes.Services;
using Fonts;
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
		builder.ConfigureLifecycleEvents(events => {
#if IOS || MACCATALYST
			events.AddiOS(ios => ios.FinishedLaunching((app, options) => {
				var mauiApp = Microsoft.Maui.Controls.Application.Current;
				if (mauiApp != null)
				{
					mauiApp.Resources.MergedDictionaries.Add(new UXDivers.Popups.Maui.Controls.DarkTheme());
					mauiApp.Resources.MergedDictionaries.Add(new UXDivers.Popups.Maui.Controls.PopupStyles());
					// Add custom resources
					var customResources = new Microsoft.Maui.Controls.ResourceDictionary
				{
					// Font Families
					{ "IconsFontFamily", MaterialSymbolsFont.FontFamily },
					{ "AppFontFamily", "Manrope" },
					{ "AppSemiBoldFamily", "ManropeSemibold" },
					
					// UXDivers Popups Icon Overrides
					{ "UXDPopupsCloseIconButton", MaterialSymbolsFont.Close },
					{ "UXDPopupsCheckCircleIconButton", MaterialSymbolsFont.Check_circle },

					{ "BackgroundColor", AppColors.Dark.Surface },
					{ "BackgroundSecondaryColor", AppColors.Dark.Surface },
					{ "BackgroundTertiaryColor", Colors.Red },
					{ "PrimaryColor", AppColors.Dark.Primary },
					{ "PrimaryVariantColor", AppColors.Dark.SurfaceElevated },
					{ "TextColor", AppColors.Dark.TextPrimary },
					{ "TextTertiaryColor", AppColors.Dark.TextSecondary },
					{ "PopupBackgroundColor", AppColors.Dark.SurfaceElevated },
					{ "PopupBorderColor", AppColors.Dark.Outline }
				};
					mauiApp.Resources.MergedDictionaries.Add(customResources);
				}

				return true;
			}));
#endif
		});

		// Configure iOS navigation bar: large titles + matching background color
		builder.ConfigureMauiHandlers(handlers => {

			ModifyEntrys();

			// #if IOS || MACCATALYST
			// 			// Use compatibility renderer for Shell to enable PrefersLargeTitles
			// 			handlers.AddHandler(typeof(Microsoft.Maui.Controls.Shell),
			// 				typeof(CometBaristaNotes.Platforms.iOS.CustomShellRenderer));
			// #endif
		});

		builder.ConfigureFonts(fonts => {
			fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			fonts.AddFont("Manrope-Regular.ttf", "Manrope");
			fonts.AddFont("Manrope-SemiBold.ttf", "ManropeSemibold");
			fonts.AddFont("MaterialSymbols.ttf", MaterialSymbolsFont.FontFamily);
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

	private static void ModifyEntrys()
	{
#if IOS || MACCATALYST
		EntryHandler.Mapper.AppendToMapping("NoBorder", (handler, view) => {
			// Remove border
			handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;

			// Optional: transparent background
			handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;

			// Optional: add a tiny left padding so text isn't flush
			handler.PlatformView.LeftView = new UIKit.UIView(new CoreGraphics.CGRect(0, 0, 4, 0));
			handler.PlatformView.LeftViewMode = UIKit.UITextFieldViewMode.Always;
		});

		PickerHandler.Mapper.AppendToMapping("NoBorder", (handler, view) => {
			// Remove border + make background transparent
			handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
			handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;

			// Optional: add a tiny left padding so text isn't flush to the edge
			handler.PlatformView.LeftView = new UIKit.UIView(new CoreGraphics.CGRect(0, 0, 4, 0));
			handler.PlatformView.LeftViewMode = UIKit.UITextFieldViewMode.Always;
		});
#endif

#if ANDROID
		EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
		{
			// Remove background/underline + any focus tint
			handler.PlatformView.Background = null;
			handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
			handler.PlatformView.BackgroundTintList =
				Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);

			// Optional: tweak padding so text isn't cramped
			handler.PlatformView.SetPadding(0, handler.PlatformView.PaddingTop, 0, handler.PlatformView.PaddingBottom);
		});

		PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
		{
			var pv = handler.PlatformView;

			// Remove default underline / background & tints
			pv.Background = null;
			pv.SetBackgroundColor(Android.Graphics.Color.Transparent);
			pv.BackgroundTintList =
				Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);

			// Optional: tighten side padding so text aligns with other controls
			pv.SetPadding(0, 0, 0, 0);
		});
#endif
	}
}
