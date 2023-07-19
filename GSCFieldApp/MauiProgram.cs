using GSCFieldApp.ViewModel;
using GSCFieldApp.Views;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

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
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialDesignIconsDesktop.ttf", "MatDesign");
            });

		// Need to add these to actually create them on start
		//Singleton will be created once
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<MainViewModel>();

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
        builder.Services.AddTransient<DetailPage>();
        builder.Services.AddTransient<DetailVieModel>();

		builder.Services.AddTransient<StationPage>();
		builder.Services.AddTransient<StationViewModel>();

        builder.Services.AddTransient<FieldBookPage>();
        builder.Services.AddTransient<FieldBookViewModel>();

        //Add localization service, making it available for all views
        builder.Services.AddLocalization();



#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
