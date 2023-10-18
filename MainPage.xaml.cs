
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
    private string name;
    public MainPage()
    {
        InitializeComponent();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        LblDate.Text = DateAndTime.DateString;
        await GetLocation();
        await GetWeatherByLocation(latitude, longitude);
        //await GetWeatherByCity(city);

        await GetNationalLoadSheddingStatus();
        await GetLoadSheddingByGPS(latitude, longitude);
        await GetAreaLoadShedding(id);

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

            await GetLoadSheddingBySearch(response);
        }
    }

    public async Task GetWeatherByCity(string text)
    {
        var results = await WeatherServices.GetWeatherByCity(text);
        WeatherUpdateUI(results);
    }

    public void WeatherUpdateUI(dynamic results)
    {
        LblCity.Text = results.name;
        LblWeatherDescription.Text = results.weather[0].description;
        LblTemperature.Text = results.main.temperature + "°C";
    }

    //Update Stages
    public async Task GetNationalLoadSheddingStatus()
    {
        var loadSheddingStatus = await LoadSheddingServices.GetStatus();
        LoadSheddingStatusUpdateUI(loadSheddingStatus);
    }
    public void LoadSheddingStatusUpdateUI(dynamic loadSheddingStatus)
    {
        LblSchedulesCurrentStage.Text = loadSheddingStatus.status.eskom.next_stages[0].stage;
    }

    //Update Area GPS
    public async Task GetLoadSheddingByGPS(double latitude, double longitude)
    {
        var loadSheddingAreaGPSResults = await LoadSheddingServices.GetAreasNearByGPS(latitude, longitude);
        LoadSheddingAreaUpdateAsync(loadSheddingAreaGPSResults);

        string areaId = loadSheddingAreaGPSResults.areas[0].id.ToString();
        if (areaId != null)
        {
            var areaInformation = await LoadSheddingServices.GetAreaInformation(areaId);
            // Use the area information as needed
        }
    }

    //Update Area Search
    public async Task GetLoadSheddingBySearch(string text)
    {
        var loadSheddingAreaSearchResults = await LoadSheddingServices.GetAreaBySearch(text);
        LoadSheddingAreaUpdateAsync(loadSheddingAreaSearchResults);

        string areaId = loadSheddingAreaSearchResults.areas[0].id.ToString();
        if (areaId != null)
        {
            var areaInformation = await LoadSheddingServices.GetAreaInformation(areaId);
            // Use the area information as needed
        }

    }
    public void  LoadSheddingAreaUpdateAsync(dynamic loadSheddingAreaGPSResults)
    {
        LblScheduleAreaName.Text = loadSheddingAreaGPSResults.areas[0].name;
        LblScheduleAreaRegion.Text = loadSheddingAreaGPSResults.areas[0].region;
  
    }


    //Update Area Information
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