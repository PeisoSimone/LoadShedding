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

             Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("License Here");

        InitializeComponent();

            MainPage = new MainPage(loadSheddingServices, weatherServices);
        }
    
}
