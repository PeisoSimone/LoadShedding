using loadshedding.Interfaces;
using loadshedding.Model;
using loadshedding.Services;
using Microsoft.Maui.Controls;

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

    public App(IWeatherServices weatherServices, ICalendarSearchServices calendarSearchServices, ICalenderServices calendarServices, IAlertServices alertServices, ILoadSheddingStatusServices loadsheddingStatusServices, INotificationServices notificationServices, LoadSheddingBackgroundService backgroundService)
    {
        _weatherServices = weatherServices;
        _loadsheddingStatusServices = loadsheddingStatusServices;
        _calendarSearchServices = calendarSearchServices;
        _calendarServices = calendarServices;
        _alertServices = alertServices;
        _notificationServices = notificationServices;
        _backgroundService = backgroundService;

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NHaF5cWWdCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWX5dcHVUR2JdV0d0VkI=");//Syncfusion Key Here

        InitializeComponent();

        _backgroundService.Start();

        MainPage = new MainPage(weatherServices, calendarSearchServices, calendarServices, alertServices, loadsheddingStatusServices, notificationServices);
       
    }
    protected override void OnStart()
    {
        base.OnStart();

        // Request notification permissions when app starts
        RequestNotificationPermission();
    }

    private async void RequestNotificationPermission()
    {
        // Request notification permission
        var result = await Plugin.LocalNotification.LocalNotificationCenter.Current.RequestNotificationPermission();
    }
}
