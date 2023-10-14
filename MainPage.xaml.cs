
using loadshedding.Services;
using Microsoft.VisualBasic;
using Microsoft.Extensions.Configuration;

namespace loadshedding;

public partial class MainPage : ContentPage
{
    private double latitude;
    private double longitude;
    private string area;
    public MainPage()
    {
        InitializeComponent();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await GetLocation();
        await GetLoadSheddingStatus();
        await GetWeatherByLocation(latitude, longitude);
        LblDate.Text = DateAndTime.DateString;
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
        var results = await WeatherServices.GetWeatherByGPS(latitude, longitude);
        UpdateUI(results);
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        var response = await DisplayPromptAsync(title: "", message: "", placeholder: "Search City", accept: "Search", cancel: "Cancel");
        if (response != null)
        {
            await GetWeatherByCity(response);
        }
    }

    public async Task GetWeatherByCity(string city)
    {
        var results = await WeatherServices.GetWeatherByCity(city);
        UpdateUI(results);
    }

    public void UpdateUI(dynamic results)
    {
        LblCity.Text = results.name;
        LblWeatherDescription.Text = results.weather[0].description;
        LblTemperature.Text = results.main.temperature + "°C";
    }

    public void LoadUpdateUI(dynamic loadSheddingResults)
    {
        LblSchedulesCurrentStage.Text = loadSheddingResults.status.eskom.next_stages[0].stage;
    }

    public async Task GetLoadSheddingStatus()
    {
       var loadSheddingResults = await LoadSheddingServices.GetStatus();
        LoadUpdateUI(loadSheddingResults);
    }
}