using loadshedding.Interfaces;
using loadshedding.Services;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter;
using loadshedding.Models;

namespace loadshedding;
public partial class App : Application
{
    private readonly IWeatherServices _weatherServices;
    private readonly ICalendarSearchServices _calendarSearchServices;
    private readonly ICalenderServices _calendarServices;
    private readonly IAlertServices _alertServices;
    private readonly ILoadSheddingStatusServices _loadsheddingStatusServices;
    private readonly INotificationServices _notificationServices;
    private readonly LoadSheddingBackgroundService _backgroundService;
    private readonly AppCenterConfiguration _appCenterConfig;
    private readonly SyncfusionConfiguration _syncfusionConfiguration;

    public App(IWeatherServices weatherServices, 
        ICalendarSearchServices calendarSearchServices, 
        ICalenderServices calendarServices, IAlertServices alertServices, 
        ILoadSheddingStatusServices loadsheddingStatusServices, 
        INotificationServices notificationServices, 
        LoadSheddingBackgroundService backgroundService, 
        AppCenterConfiguration appCenterConfig,
        SyncfusionConfiguration syncfusionConfiguration)
    {
        _weatherServices = weatherServices;
        _loadsheddingStatusServices = loadsheddingStatusServices;
        _calendarSearchServices = calendarSearchServices;
        _calendarServices = calendarServices;
        _alertServices = alertServices;
        _notificationServices = notificationServices;
        _backgroundService = backgroundService;
        _appCenterConfig = appCenterConfig;
        _syncfusionConfiguration = syncfusionConfiguration;

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(_syncfusionConfiguration.Key);

        InitializeComponent();

        _backgroundService.Start();

        MainPage = new MainPage(weatherServices, calendarSearchServices, calendarServices, alertServices, loadsheddingStatusServices, notificationServices);
       
    }
    protected override void OnStart()
    {
        base.OnStart();

        //Initialise App Center
        AppCenter.Start(
            $"android={_appCenterConfig.AndroidKey};",
            typeof(Analytics), typeof(Crashes));

        // Request notification permissions when app starts
        RequestNotificationPermission();
    }

    private async void RequestNotificationPermission()
    {
        // Request notification permission
        var result = await Plugin.LocalNotification.LocalNotificationCenter.Current.RequestNotificationPermission();
    }

    protected override void OnSleep()
    {
        // Handle when your app sleeps
        base.OnSleep();
    }

    protected override void OnResume()
    {
        // Handle when your app resumes
        base.OnResume();
    }
}
