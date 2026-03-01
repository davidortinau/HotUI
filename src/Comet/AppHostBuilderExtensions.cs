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
				{ typeof(Border), typeof(BorderHandler) },
				{ typeof(Button), typeof(ButtonHandler) },
				{ typeof(CheckBox), typeof(CheckBoxHandler) },
				{ typeof(CometWindow), typeof(WindowHandler) },
				{ typeof(DatePicker), typeof(DatePickerHandler) },
				{ typeof(FlyoutView), typeof(FlyoutViewHandler) },
				{ typeof(Frame), typeof(BorderHandler) },
				{ typeof(GraphicsView), typeof(GraphicsViewHandler) },
				{ typeof(Image) , typeof(ImageHandler) },
				{ typeof(ImageButton) , typeof(ImageButtonHandler) },
				{ typeof(IndicatorView), typeof(IndicatorViewHandler) },
				{ typeof(Picker), typeof(PickerHandler) },
				{ typeof(ProgressBar), typeof(ProgressBarHandler) },
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


			ThreadHelper.SetFireOnMainThread(MainThread.BeginInvokeOnMainThread);

			return builder;
		}


	}
}