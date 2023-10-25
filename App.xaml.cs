using loadshedding.Services;

namespace loadshedding;

public partial class App : Application
{
	public App()
	{
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NHaF5cXmVCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdgWH9fdnVWR2BZVExzW0o=");

        InitializeComponent();

        ILoadSheddingServices loadSheddingServices = new LoadSheddingServices(new HttpClient());
        IWeatherServices weatherServices = new WeatherServices(new HttpClient());

        MainPage = new MainPage(loadSheddingServices, weatherServices);
    }
}
