using Syncfusion.Maui.Core.Hosting;
using System;
using System.IO;
using loadshedding.Services;
using loadshedding.Model;
using Microsoft.Extensions.Configuration;
using System.Text;

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

        var jsonFilePath = @"C:\Users\Pee_Jay Simone\source\repos\Codility\LoadShedding\test.txt";

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(jsonFilePath)
            .AddEnvironmentVariables()
            .Build();


        var apiKeys = new ApiKeysConfiguration();
        configuration.GetSection("ApiKeys").Bind(apiKeys);

        builder.Services.AddSingleton(apiKeys);


        builder.Services.AddHttpClient<IWeatherServices, WeatherServices>(weatherclient =>
        {
            weatherclient.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");
        });

        builder.Services.AddHttpClient<ILoadSheddingServices, LoadSheddingServices>(loadsheddigclient =>
        {
            loadsheddigclient.BaseAddress = new Uri("https://developer.sepush.co.za/business/2.0/");
        });


        //var apiKeysConfiguration = new ApiKeysConfiguration
        //{
        //    WeatherApiKey = "f9269e5ecd3313a8bab2ed1d692a92b9",
        //    LoadSheddingApiKey = "8ED8EBBF-FEBE40C3-89D33271-27C7A791"
        //};
        //builder.Services.AddSingleton(apiKeysConfiguration);

        //builder.Services.AddSingleton<IWeatherServices, WeatherServices>();
        //builder.Services.AddSingleton<ILoadSheddingServices, LoadSheddingServices>();

        return builder.Build();
    }
}
