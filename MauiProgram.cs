using Syncfusion.Maui.Core.Hosting;
using System;
using System.IO;
using loadshedding.Services;
using loadshedding.Model;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;
using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;

namespace loadshedding;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().UseMauiCommunityToolkit();

        builder
        .UseMauiApp<App>()
        .ConfigureSyncfusionCore()
        .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            fonts.AddFont("Nexa-ExtraLight.ttf", "Nexa-Light");
            fonts.AddFont("Nexa-Heavy.ttf", "Nexa-Heavy");
        });

        builder.AddAppSettings();

        var apiKeysConfig = builder.Configuration.GetSection("ApiKeys").Get<ApiKeysConfiguration>();
        var apiKeys = new ApiKeysConfiguration
        {
            WeatherApiKey = apiKeysConfig?.WeatherApiKey,
            //LoadSheddingApiKey = apiKeysConfig?.LoadSheddingApiKey,
        };
        builder.Services.AddSingleton(apiKeys);
        builder.Services.AddHttpClient<IWeatherServices, WeatherServices>(weatherclient =>
        {
            weatherclient.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");
            
        });

        builder.Services.AddHttpClient<ICalenderAPIServices, CalenderAPIServices>(calenderapiclient =>
        {
            calenderapiclient.BaseAddress = new Uri("https://eskom-calendar-api.shuttleapp.rs/");
        });

        builder.Services.AddSingleton<ICalendarSearchServices, CalendarSearchServices>();
        builder.Services.AddSingleton<IAlertServices, AlertServices>();

        return builder.Build();
    }

    private static void AddAppSettings(this MauiAppBuilder builder)
    {
        using Stream stream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("loadshedding.appsettings.json");

        if(stream != null)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            builder.Configuration.AddConfiguration(config);
        }
    }
}
