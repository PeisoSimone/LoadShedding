using loadshedding.Model;
using loadshedding.Services;
using Microsoft.Maui.Controls;

namespace loadshedding;
public partial class App : Application
{
    private readonly IWeatherServices weatherServices;
    private readonly ICalendarSearchServices calendarSearchServices;
    private readonly ICalenderAPIServices calendarAPIServices;
    private readonly IAlertServices alertServices;

    public App(IWeatherServices weatherServices, ICalendarSearchServices calendarSearchServices, ICalenderAPIServices calendarAPIServices, IAlertServices alertServices)
    {
        this.weatherServices = weatherServices;
        this.calendarSearchServices = calendarSearchServices;
        this.calendarAPIServices = calendarAPIServices;
        this.alertServices = alertServices;

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NHaF5cWWdCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWX5dcHVUR2JdV0d0VkI=");//Syncfusion Key Here

        InitializeComponent();

        MainPage = new MainPage(weatherServices, calendarSearchServices, calendarAPIServices,  alertServices);
    }
}
