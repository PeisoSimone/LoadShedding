
using loadshedding.Services;
using Microsoft.VisualBasic;


namespace loadshedding;

public partial class MainPage : ContentPage
{
    private double latitude;
    private double longitude;
    private readonly ILoadSheddingServices _loadsheddingServices;
    private readonly IWeatherServices _weatherServices;

    public MainPage(ILoadSheddingServices loadSheddingServices, IWeatherServices weatherServices)
    {
        _loadsheddingServices = loadSheddingServices;
        _weatherServices = weatherServices;

        var loadshedding = new ServiceCollection();
        loadshedding.AddSingleton<ILoadSheddingServices, LoadSheddingServices>();

        var weather = new ServiceCollection();  
        weather.AddSingleton<IWeatherServices, WeatherServices>();

        InitializeComponent();
    }
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        LblDate.Text = DateAndTime.DateString;
        await GetLocation();
        await GetWeatherByLocation(latitude, longitude);
        await GetNationalLoadSheddingStatus();
        await GetLoadSheddingByGPS(latitude, longitude);
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
        await GetNationalLoadSheddingStatus();
        await GetLoadSheddingByGPS(latitude, longitude);
    }

    public async Task GetWeatherByLocation(double latitude, double longitude)
    {
        var results = await _weatherServices.GetWeatherByGPS(latitude, longitude);
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
        var results = await _weatherServices.GetWeatherByCity(text);
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
        var loadSheddingStatus = await _loadsheddingServices.GetStatus();
        LoadSheddingStatusUpdateUI(loadSheddingStatus);
    }
    public void LoadSheddingStatusUpdateUI(dynamic loadSheddingStatus)
    {
        LblSchedulesCurrentStage.Text = loadSheddingStatus.status.eskom.next_stages[0].stage;
    }

    //Update Area GPS
    public async Task GetLoadSheddingByGPS(double latitude, double longitude)
    {
        var loadSheddingAreaGPSResults = await _loadsheddingServices.GetAreasNearByGPS(latitude, longitude);
        LoadSheddingAreaUpdateAsync(loadSheddingAreaGPSResults);

        string areaId = loadSheddingAreaGPSResults.areas[0].id.ToString();
        if (areaId != null)
        {
            await GetAreaLoadShedding(areaId);
        }
    }

    //Update Area Search
    public async Task GetLoadSheddingBySearch(string text)
    {
        var loadSheddingAreaSearchResults = await _loadsheddingServices.GetAreaBySearch(text);
        LoadSheddingAreaUpdateAsync(loadSheddingAreaSearchResults);

        string areaId = loadSheddingAreaSearchResults.areas[0].id.ToString();
        if (areaId != null)
        {
            await GetAreaLoadShedding(areaId);
        }
    }
    public void  LoadSheddingAreaUpdateAsync(dynamic loadSheddingAreaGPSResults)
    {
        LblScheduleAreaName.Text = loadSheddingAreaGPSResults.areas[0].name;
        LblScheduleAreaRegion.Text = loadSheddingAreaGPSResults.areas[0].region;
    }

    //Update Area Information
    public async Task GetAreaLoadShedding(string AreaId)
    {
        var loadSheddingAreaResults = await _loadsheddingServices.GetAreaInformation(AreaId);
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