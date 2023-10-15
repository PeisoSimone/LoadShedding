
using loadshedding.Services;
using Microsoft.VisualBasic;
using Microsoft.Extensions.Configuration;
using loadshedding.Model;


namespace loadshedding;

public partial class MainPage : ContentPage
{
    private double latitude;
    private double longitude;
    private string id;
    public MainPage()
    {
        InitializeComponent();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await GetLocation();
        await GetWeatherByLocation(latitude, longitude);

        await GetLoadSheddingStatus();
        await GetLoadSheddingByGPS(latitude, longitude);
        await GetAreaLoadShedding(id);
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
        WeatherUpdateUI(results);
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
        WeatherUpdateUI(results);
    }

    public void WeatherUpdateUI(dynamic results)
    {
        LblCity.Text = results.name;
        LblWeatherDescription.Text = results.weather[0].description;
        LblTemperature.Text = results.main.temperature + "°C";
    }




    //Update Stages
    public async Task GetLoadSheddingStatus()
    {
        var loadSheddingStatus = await LoadSheddingServices.GetStatus();
        LoadSheddingStatusUpdateUI(loadSheddingStatus);
    }
    public void LoadSheddingStatusUpdateUI(dynamic loadSheddingStatus)
    {
        LblSchedulesCurrentStage.Text = loadSheddingStatus.status.eskom.next_stages[0].stage;
    }

    //Update Area Info
    public async Task GetLoadSheddingByGPS(double latitude, double longitude)
    //{
 
    //    string id = await LoadSheddingServices.GetAreasNearByGPS(latitude, longitude);
    //    if (id != null)
    //    {
    //        AreaInformationRoot areaInformation = await LoadSheddingServices.GetAreaInformation(id);
    //    }
    //}

    public async Task GetAreaLoadShedding(string id)
    {
        var loadSheddingAreaResults = await LoadSheddingServices.GetAreaInformation(id);
        LoadSheddingAreaUpdateUI(loadSheddingAreaResults);
    }
    public void LoadSheddingAreaUpdateUI(dynamic loadSheddingAreaResults)
    {
        if (loadSheddingAreaResults.events != null && loadSheddingAreaResults.events.Count > 0)
        {
            var firstEvent = loadSheddingAreaResults.events[0];
            DateTime startTime = firstEvent.start;
            DateTime endTime = firstEvent.end;

            // Format the DateTime objects as strings
            string startTimeString = startTime.ToString("HH:mm");
            string endTimeString = endTime.ToString("HH:mm");

            LblSchedulesEvetStart.Text = startTimeString;
            LblSchedulesEvetStop.Text = endTimeString;
        }
    }
}