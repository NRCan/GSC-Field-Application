﻿using CommunityToolkit.Maui;
using GSCFieldApp.Services;
using GSCFieldApp.ViewModel;
using GSCFieldApp.Views;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;


//#if WINDOWS10_0_19041_0_OR_GREATER
//using Microsoft.Maui.LifecycleEvents;
//using WinUIEx;
//#endif

namespace GSCFieldApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        //NOTE: fix for mapsui failing on navigating to
		//https://github.com/mono/SkiaSharp/discussions/1882

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp(true)
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialDesignIconsDesktop.ttf", "MatDesign");
                fonts.AddFont("STENCIL.TTF", "Stencil");
            });

//#if WINDOWS
//            builder.ConfigureLifecycleEvents(events =>
//            {
//                events.AddWindows(wndLifeCycleBuilder =>
//                {
//                    wndLifeCycleBuilder.OnWindowCreated(window =>
//                    {
//                        window.CenterOnScreen(1024,768); //Set size and center on screen using WinUIEx extension method

//                        var manager = WinUIEx.WindowManager.Get(window);
//                        manager.PersistenceId = "MainWindowPersistanceId"; // Remember window position and size across runs
//                        manager.MinWidth = 640;
//                        manager.MinHeight = 480;
//                    });
//                });
//            });
//#endif

        // Need to add these to actually create them on start
        //Singleton will be created once
        builder.Services.AddSingleton<SettingsPage>();
		builder.Services.AddSingleton<SettingsViewModel>();

		builder.Services.AddSingleton<FieldBooksPage>();
        builder.Services.AddSingleton<FieldBooksViewModel>();

        builder.Services.AddSingleton<FieldNotesPage>();
        builder.Services.AddSingleton<FieldNotesViewModel>();

        builder.Services.AddSingleton<MapPage>();
        builder.Services.AddSingleton<MapViewModel>();

		builder.Services.AddSingleton<AboutPage>();
        builder.Services.AddSingleton<AboutPageViewModel>();

        //Transient will be created/deleted each time
		builder.Services.AddTransient<StationPage>();
		builder.Services.AddTransient<StationViewModel>();

        builder.Services.AddTransient<FieldBookPage>();
        builder.Services.AddTransient<FieldBookViewModel>();

        builder.Services.AddTransient<EarthmatPage>();
        builder.Services.AddTransient<EarthmatViewModel>();

        builder.Services.AddTransient<SamplePage>();
        builder.Services.AddTransient<SampleViewModel>();

        builder.Services.AddTransient<DocumentPage>();
        builder.Services.AddTransient<DocumentViewModel>();

        builder.Services.AddTransient<StructurePage>();
        builder.Services.AddTransient<StructureViewModel>();

        builder.Services.AddTransient<PaleoflowPage>();
        builder.Services.AddTransient<PaleoflowViewModel>();

        builder.Services.AddTransient<FossilPage>();
        builder.Services.AddTransient<FossilViewModel>();

        builder.Services.AddTransient<EnvironmentPage>();
        builder.Services.AddTransient<EnvironmentViewModel>();

        builder.Services.AddTransient<MineralPage>();
        builder.Services.AddTransient<MineralViewModel>();

        builder.Services.AddTransient<MineralizationAlterationPage>();
        builder.Services.AddTransient<MineralizationAlterationViewModel>();

        builder.Services.AddTransient<LocationPage>();
        builder.Services.AddTransient<LocationViewModel>();

        builder.Services.AddTransient<DrillHolePage>();
        builder.Services.AddTransient<DrillHoleViewModel>();

        builder.Services.AddTransient<PicklistPage>();
        builder.Services.AddTransient<PicklistViewModel>();

        builder.Services.AddTransient<LineworkPage>();
        builder.Services.AddTransient<LineworkViewModel>();

        //Add localization service, making it available for all views
        builder.Services.AddLocalization();
        builder.Services.AddTransient<MessageService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
