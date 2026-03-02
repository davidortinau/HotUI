using System;
using System.Collections.Generic;
using Comet.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Animations;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

#if WINDOWS
using Microsoft.Maui.Graphics.Win2D;
#endif

namespace Comet
{
	public static class AppHostBuilderExtensions
	{
		static void AddHandlers(this IMauiHandlersCollection collection, Dictionary<Type, Type> handlers) => handlers.ForEach(x => collection.AddHandler(x.Key, x.Value));
		public static MauiAppBuilder UseCometApp<TApp>(this MauiAppBuilder builder)
			where TApp : class, IApplication
		{
			builder.Services.TryAddSingleton<IApplication, TApp>();
			builder.UseCometHandlers();
			return builder;
		}
		public static MauiAppBuilder UseCometHandlers(this MauiAppBuilder builder)
		{

			//AnimationManger.SetTicker(new iOSTicker());

			//Set Default Style
			var style = new Styles.Style();
			style.Apply();

			ViewHandler.ViewMapper.AppendToMapping(nameof(IGestureView.Gestures), CometViewHandler.AddGestures);
			ViewHandler.ViewCommandMapper.AppendToMapping(Gesture.AddGestureProperty, CometViewHandler.AddGesture);
			ViewHandler.ViewCommandMapper.AppendToMapping(Gesture.RemoveGestureProperty, CometViewHandler.RemoveGesture);
			Lerp.Lerps[typeof(FrameConstraints)] = new Lerp
			{
				Calculate = (s, e, progress) => {
					var start = (FrameConstraints)s;
					var end = (FrameConstraints)(e);
					return start.Lerp(end, progress);
				}
			};
			builder.ConfigureMauiHandlers((handlersCollection) => handlersCollection.AddHandlers(new Dictionary<Type, Type>
			{
				{ typeof(AbstractLayout), typeof(LayoutHandler) },
				{ typeof(AbsoluteLayout), typeof(LayoutHandler) },
				{ typeof(FlexLayout), typeof(LayoutHandler) },
				{ typeof(ActivityIndicator), typeof(ActivityIndicatorHandler) },
				{ typeof(Border), typeof(ContentViewHandler) },
			{ typeof(MauiViewHost), typeof(Handlers.MauiViewHostHandler) },
			{ typeof(CometHost), typeof(Handlers.CometHostHandler) },
				{ typeof(Button), typeof(ButtonHandler) },
				{ typeof(CheckBox), typeof(CheckBoxHandler) },
				{ typeof(CometWindow), typeof(WindowHandler) },
				{ typeof(DatePicker), typeof(DatePickerHandler) },
				{ typeof(FlyoutView), typeof(FlyoutViewHandler) },
				{ typeof(Frame), typeof(ContentViewHandler) },
				{ typeof(GraphicsView), typeof(GraphicsViewHandler) },
				{ typeof(Image) , typeof(ImageHandler) },
				{ typeof(ImageButton) , typeof(ImageButtonHandler) },
				{ typeof(IndicatorView), typeof(IndicatorViewHandler) },
				{ typeof(Picker), typeof(PickerHandler) },
				{ typeof(ProgressBar), typeof(ProgressBarHandler) },
				{ typeof(RadioButton), typeof(RadioButtonHandler) },
				{ typeof(RefreshView), typeof(RefreshViewHandler) },
				{ typeof(SearchBar), typeof(SearchBarHandler) },
				{ typeof(SecureField), typeof(EntryHandler) },
				{ typeof(Slider), typeof(SliderHandler) },
				{ typeof(Stepper), typeof(StepperHandler) },
				{ typeof(Spacer), typeof(SpacerHandler) },
				{ typeof(SwipeView), typeof(SwipeViewHandler) },
				{ typeof(TabView), typeof(TabViewHandler) },
				{ typeof(TextEditor), typeof(EditorHandler) },
				{ typeof(TextField), typeof(EntryHandler) },
				{ typeof(Text), typeof(LabelHandler) },
				{ typeof(TimePicker), typeof(TimePickerHandler) },
				{ typeof(Toggle), typeof(SwitchHandler) },
				{ typeof(Toolbar), typeof(ToolbarHandler) },
				{ typeof(CometApp), typeof(ApplicationHandler) },
				{ typeof(ListView),typeof(ListViewHandler) },
				{ typeof(CollectionView),typeof(ListViewHandler) },
				{ typeof(CarouselView),typeof(ListViewHandler) },
				{ typeof(BoxView), typeof(Handlers.ShapeViewHandler) },
#if __MOBILE__
				{typeof(ScrollView), typeof(Handlers.ScrollViewHandler) },
				{typeof(ShapeView), typeof(Handlers.ShapeViewHandler)},
#else
				
				{typeof(ScrollView), typeof(Microsoft.Maui.Handlers.ScrollViewHandler) },
#endif


#if __IOS__
				{typeof(NavigationView), typeof (Handlers.NavigationViewHandler)},
				{typeof(View), typeof(CometViewHandler)},
#else
				
				{typeof(NavigationView), typeof (Microsoft.Maui.Handlers.NavigationViewHandler)},
#endif
				{typeof(WebView), typeof(Microsoft.Maui.Handlers.WebViewHandler)},
			}));

			// Register standard MAUI Controls handlers for MauiViewHost embedding.
			// These enable Microsoft.Maui.Controls types (Label, Entry, Border, etc.)
			// to be rendered when hosted inside a Comet view tree via MauiViewHost.
			// Interface-based registrations are critical for third-party controls (e.g. Syncfusion)
			// that implement IContentView but don't extend ContentView directly.
			builder.ConfigureMauiHandlers((handlersCollection) =>
			{
				// Interface-based handler registrations (matches MAUI's own registrations)
				handlersCollection.TryAddHandler<IContentView, Microsoft.Maui.Handlers.ContentViewHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.Label, Microsoft.Maui.Handlers.LabelHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.Entry, Microsoft.Maui.Handlers.EntryHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.Editor, Microsoft.Maui.Handlers.EditorHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.Button, Microsoft.Maui.Handlers.ButtonHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.CheckBox, Microsoft.Maui.Handlers.CheckBoxHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.Switch, Microsoft.Maui.Handlers.SwitchHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.Slider, Microsoft.Maui.Handlers.SliderHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.Stepper, Microsoft.Maui.Handlers.StepperHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.Picker, Microsoft.Maui.Handlers.PickerHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.DatePicker, Microsoft.Maui.Handlers.DatePickerHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.TimePicker, Microsoft.Maui.Handlers.TimePickerHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.Image, Microsoft.Maui.Handlers.ImageHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.ImageButton, Microsoft.Maui.Handlers.ImageButtonHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.SearchBar, Microsoft.Maui.Handlers.SearchBarHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.ProgressBar, Microsoft.Maui.Handlers.ProgressBarHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.ActivityIndicator, Microsoft.Maui.Handlers.ActivityIndicatorHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.RadioButton, Microsoft.Maui.Handlers.RadioButtonHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.Border, Microsoft.Maui.Handlers.BorderHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.BoxView, Microsoft.Maui.Handlers.ShapeViewHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.ContentView, Microsoft.Maui.Handlers.ContentViewHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.Layout, Microsoft.Maui.Handlers.LayoutHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.Frame, Microsoft.Maui.Handlers.BorderHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.ScrollView, Microsoft.Maui.Handlers.ScrollViewHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.Grid, Microsoft.Maui.Handlers.LayoutHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.StackLayout, Microsoft.Maui.Handlers.LayoutHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.HorizontalStackLayout, Microsoft.Maui.Handlers.LayoutHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.VerticalStackLayout, Microsoft.Maui.Handlers.LayoutHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.FlexLayout, Microsoft.Maui.Handlers.LayoutHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.AbsoluteLayout, Microsoft.Maui.Handlers.LayoutHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.RefreshView, Microsoft.Maui.Handlers.RefreshViewHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.SwipeView, Microsoft.Maui.Handlers.SwipeViewHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.IndicatorView, Microsoft.Maui.Handlers.IndicatorViewHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.WebView, Microsoft.Maui.Handlers.WebViewHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.Page, Microsoft.Maui.Handlers.PageHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.ContentPage, Microsoft.Maui.Handlers.PageHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.NavigationPage, Microsoft.Maui.Handlers.NavigationViewHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.TabbedPage, Microsoft.Maui.Handlers.TabbedViewHandler>();
				handlersCollection.TryAddHandler<Microsoft.Maui.Controls.FlyoutPage, Microsoft.Maui.Handlers.FlyoutViewHandler>();
			});


			ThreadHelper.SetFireOnMainThread(MainThread.BeginInvokeOnMainThread);

			return builder;
		}


	}
}