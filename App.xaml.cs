using loadshedding.Services;

namespace loadshedding;

public partial class App : Application
{
	public App()
	{
		Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjYxMDUwMkAzMjMyMmUzMDJlMzBpUGExL3NtVVVKTHZsMGdrRmRzNDVnL2wyOWZSRDBOSEw3ZzZQMnkzNWVFPQ==");
		InitializeComponent();

		MainPage = new MainPage();

        //var loadSheddingServices = new LoadSheddingServices(/* constructor arguments */);

        //MainPage = new MainPage(loadSheddingServices);
    }
}
