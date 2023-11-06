using Syncfusion.Maui.Core.Hosting;
using loadshedding.Services;
using loadshedding.Model;

namespace loadshedding;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
        .UseMauiApp<App>()
        .ConfigureSyncfusionCore()
        .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });

        var apiKeysConfiguration = new ApiKeysConfiguration
        {
            WeatherApiKey = "f9269e5ecd3313a8bab2ed1d692a92b9",
            LoadSheddingApiKey = "8ED8EBBF-FEBE40C3-89D33271-27C7A791"
        };
        builder.Services.AddSingleton(apiKeysConfiguration);

        builder.Services.AddSingleton<IWeatherServices, WeatherServices>();
        builder.Services.AddSingleton<ILoadSheddingServices, LoadSheddingServices>();

        return builder.Build();
    }
}
