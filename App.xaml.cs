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

    public App(IWeatherServices weatherServices, ICalendarSearchServices calendarSearchServices, ICalenderServices calendarServices, IAlertServices alertServices, ILoadSheddingStatusServices loadsheddingStatusServices)
    {
        _weatherServices = weatherServices;
        _loadsheddingStatusServices = loadsheddingStatusServices;
        _calendarSearchServices = calendarSearchServices;
        _calendarServices = calendarServices;
        _alertServices = alertServices;

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NHaF5cWWdCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWX5dcHVUR2JdV0d0VkI=");//Syncfusion Key Here

        InitializeComponent();

        MainPage = new MainPage(weatherServices, calendarSearchServices, calendarServices,  alertServices, loadsheddingStatusServices);
    }
}
