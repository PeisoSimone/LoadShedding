namespace loadshedding;

public partial class App : Application
{
	public App()
	{
		Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjYxMDUwMkAzMjMyMmUzMDJlMzBpUGExL3NtVVVKTHZsMGdrRmRzNDVnL2wyOWZSRDBOSEw3ZzZQMnkzNWVFPQ==");
		InitializeComponent();

		MainPage = new MainPage();
	}
}
