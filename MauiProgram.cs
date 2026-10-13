using CommunityToolkit.Maui;
using LocationHeatMapApp.Controls;
using LocationHeatMapApp.Services;
using LocationHeatMapApp.ViewModels;
using LocationHeatMapApp.Views;

namespace LocationHeatMapApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .UseMauiCommunityToolkit()
            .ConfigureMauiHandlers(handlers =>
            {
#if ANDROID || IOS || MACCATALYST || WINDOWS
                // Register custom HeatMap handler extending standard MAUI MapHandler
                handlers.AddHandler<HeatMap, HeatMapHandler>();
#endif
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 1. Storage & Database Services
        builder.Services.AddSingleton<ILocationDatabase, LocationDatabase>();

        // 2. Geolocation Hardware & Tracking Service
        builder.Services.AddSingleton<ILocationTracker, LocationTrackerService>();

        // 3. ViewModels
        builder.Services.AddSingleton<MainMapViewModel>();
        builder.Services.AddSingleton<HistoryViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();

        // 4. Views / Pages
        builder.Services.AddTransient<MainMapView>();
        builder.Services.AddTransient<HistoryView>();
        builder.Services.AddTransient<SettingsView>();

        return builder.Build();
    }
}
