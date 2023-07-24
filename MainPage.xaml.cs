using loadshedding.Services;
using Microsoft.Maui.Controls;

namespace loadshedding;

public partial class MainPage : ContentPage
{
    private double latitude;
    private double longitude;
	public MainPage()
	{
		InitializeComponent();
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await GetLocation();
        await GetWeatherByLocation(latitude, longitude);

    }

    public async Task GetLocation()
    {
        var location = await Geolocation.GetLocationAsync();
        latitude = location.Latitude;
        longitude = location.Longitude;
    }

    private async void TapLocation_Tapped(object sender, EventArgs e)
    {
        await GetLocation();
        await GetWeatherByLocation(latitude, longitude);
    }

    public async Task GetWeatherByLocation(double latitude, double longitude)
    {
        var results = await ApiServices.GetWeather(latitude, longitude);
        UpdateUI(results);
        
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        var response = await DisplayPromptAsync(title: "", message: "", placeholder: "Search City", accept: "Search", cancel: "Cancel");
        if(response != null)
        {
            await GetWeatherByCity(response);
            
        }
    }

    public async Task GetWeatherByCity(string city)
    {
        var results = await ApiServices.GetWeatherByCity(city);
        UpdateUI(results);
        
    }

    public void UpdateUI(dynamic results)
    {
        LblCity.Text = results.name;
        LblWeatherDescription.Text = results.weather[0].description;
        LblTemperature.Text = results.main.temperature + "°C";
    }
}