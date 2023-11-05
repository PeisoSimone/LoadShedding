
using loadshedding.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace loadshedding;

public partial class App : Application
{
	public App()
	{
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NHaF5cXmVCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdgWH9fdnVWR2BZVExzW0o=");

        InitializeComponent();

        
        IWeatherServices weatherServices = new WeatherServices();
        ILoadSheddingServices loadSheddingServices = new LoadSheddingServices();

        MainPage = new MainPage(loadSheddingServices, weatherServices);
    }
}
