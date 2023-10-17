using loadshedding.Services;

namespace loadshedding;

public partial class App : Application
{
	public App()
	{
		Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NHaF5cXmVCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdgWH9fdnVWR2BZVExzW0o=");
		InitializeComponent();

		MainPage = new MainPage();

        //var loadSheddingServices = new LoadSheddingServices(/* constructor arguments */);

        //MainPage = new MainPage(loadSheddingServices);
    }
}
