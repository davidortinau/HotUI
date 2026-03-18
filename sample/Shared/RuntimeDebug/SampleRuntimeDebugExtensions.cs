#if DEBUG
using System;
using Comet;
using MauiDevFlow.Agent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;

namespace Microsoft.Maui.Hosting;

public static class SampleRuntimeDebugExtensions
{
	public static MauiAppBuilder EnableSampleRuntimeDebugging(this MauiAppBuilder builder)
	{
		builder.Logging.AddDebug();
		builder.AddMauiDevFlowAgent();
		return builder;
	}

	public static MauiAppBuilder UseCometSampleDebugHost<TView>(this MauiAppBuilder builder)
		where TView : Comet.View, new()
	{
		return builder.UseCometSampleDebugHost(static () => new TView(), typeof(TView));
	}

	public static MauiAppBuilder UseCometSampleDebugHost(this MauiAppBuilder builder, Func<Comet.View> rootViewFactory)
	{
		return builder.UseCometSampleDebugHost(rootViewFactory, null);
	}

	static MauiAppBuilder UseCometSampleDebugHost(this MauiAppBuilder builder, Func<Comet.View> rootViewFactory, Type? rootType)
	{
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(rootViewFactory);

		if (rootType != null && typeof(CometApp).IsAssignableFrom(rootType))
			throw new InvalidOperationException(
				$"DEBUG sample host requires a real root Comet.View factory, not '{rootType.FullName}'. " +
				"Use a root-view factory such as UseCometSampleDebugHost(() => new MainPage()).");

		builder.Services.AddSingleton<ICometSampleDebugRootViewFactory>(new CometSampleDebugRootViewFactory(rootViewFactory));
		builder.UseMauiApp<CometSampleDebugHostApplication>();
		builder.UseCometHandlers();
		return builder;
	}
}

interface ICometSampleDebugRootViewFactory
{
	Comet.View Create();
}

sealed class CometSampleDebugRootViewFactory : ICometSampleDebugRootViewFactory
{
	readonly Func<Comet.View> rootViewFactory;

	public CometSampleDebugRootViewFactory(Func<Comet.View> rootViewFactory)
	{
		ArgumentNullException.ThrowIfNull(rootViewFactory);
		this.rootViewFactory = rootViewFactory;
	}

	public Comet.View Create()
	{
		var rootView = rootViewFactory();
		if (rootView is null)
			throw new InvalidOperationException("DEBUG sample host root-view factory returned null.");
		if (rootView is CometApp)
			throw new InvalidOperationException(
				$"DEBUG sample host cannot wrap '{rootView.GetType().FullName}' because it inherits CometApp. " +
				"Return the real root Comet.View instead.");
		return rootView;
	}
}

sealed class CometSampleDebugHostApplication : Application
{
	readonly ICometSampleDebugRootViewFactory rootViewFactory;

	public CometSampleDebugHostApplication(ICometSampleDebugRootViewFactory rootViewFactory)
	{
		ArgumentNullException.ThrowIfNull(rootViewFactory);
		this.rootViewFactory = rootViewFactory;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var rootView = rootViewFactory.Create();
		var page = new Microsoft.Maui.Controls.ContentPage
		{
			Padding = 0,
			Content = new CometHost(rootView)
		};

		return new Window(page);
	}
}
#endif
