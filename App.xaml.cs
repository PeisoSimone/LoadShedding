using loadshedding.Model;
using loadshedding.Services;
using Microsoft.Maui.Controls;

namespace loadshedding;

public partial class App : Application
{

        private readonly IWeatherServices weatherServices;
        private readonly ILoadSheddingServices loadSheddingServices;

        public App(IWeatherServices weatherServices, ILoadSheddingServices loadSheddingServices)
        {
            this.weatherServices = weatherServices;
            this.loadSheddingServices = loadSheddingServices;

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NHaF5cXmVCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdgWH9fdnVWR2BZVExzW0o=");

            InitializeComponent();

            MainPage = new MainPage(loadSheddingServices, weatherServices);
        }
    
}
