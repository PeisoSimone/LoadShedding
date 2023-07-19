namespace loadshedding;

public partial class App : Application
{
	public App()
	{
		Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NGaF5cXmdCeUx0Rnxbf1xzZFRHal5QTnJfUiweQnxTdEZjXn5YcXVXR2RcVkN+Ww==");
		InitializeComponent();

		MainPage = new MainPage();
	}
}
