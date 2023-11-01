
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
        InitializeComponent();

        _loadsheddingServices = loadSheddingServices;
        _weatherServices = weatherServices;

        var loadshedding = new ServiceCollection();
        loadshedding.AddSingleton<ILoadSheddingServices, LoadSheddingServices>();

        var weather = new ServiceCollection();
        weather.AddSingleton<IWeatherServices, WeatherServices>();

    }
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        LblDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
        await GetLocation();
        await GetWeatherByLocation(latitude, longitude);
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

    //Update Area GPS
    public async Task GetLoadSheddingByGPS(double latitude, double longitude)
    {
        var loadSheddingAreaGPSResults = await _loadsheddingServices.GetAreasNearByGPS(latitude, longitude);

        string[] areaNames = loadSheddingAreaGPSResults.areas.Select(area => area.name).ToArray();
        var selectedAreaName = await DisplayActionSheet("Current Load Shedding Area", "Cancel", null, areaNames);

        if (selectedAreaName != null && selectedAreaName != "Cancel")
        {
            // User selected an area
            string areaId = loadSheddingAreaGPSResults.areas
                .First(area => area.name == selectedAreaName).id;
            await GetAreaLoadShedding(areaId);
        }
    }


    //Update Area Search
    public async Task GetLoadSheddingBySearch(string text)
    {
        var loadSheddingAreaSearchResults = await _loadsheddingServices.GetAreaBySearch(text);

        string[] areaNames = loadSheddingAreaSearchResults.areas.Select(area => area.name).ToArray();
        var selectedAreaName = await DisplayActionSheet("Select Load Shedding Area", "Cancel", null, areaNames);

        if (selectedAreaName != null && selectedAreaName != "Cancel")
        {
            // User selected an area
            string areaId = loadSheddingAreaSearchResults.areas
                .First(area => area.name == selectedAreaName).id;
            await GetAreaLoadShedding(areaId);
        }
    }

    public async Task GetAreaLoadShedding(string AreaId)
    {
        var loadSheddingAreaResults = await _loadsheddingServices.GetAreaInformation(AreaId);
        LoadSheddingAreaUpdateUI(loadSheddingAreaResults);
    }

    //Update Area Information

    public void LoadSheddingAreaUpdateUI(dynamic loadSheddingAreaResults)
    {
        if (loadSheddingAreaResults.info != null)
        {
            LblScheduleAreaName.Text = loadSheddingAreaResults.info.name;
        }

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
            LblSchedulesCurrentStage.Text = firstEvent.note;

            string currentStage = LblSchedulesCurrentStage.Text;



            if (currentStage != null)
            {
                var stageLoadshedding = loadSheddingAreaResults.schedule.days[0];

                if(currentStage == "Stage 0") 
                {
                    LblStage.Text = stageLoadshedding.stage[0];
                }

                if (currentStage == "Stage 1")
                {
                    LblStage.Text = stageLoadshedding.stage[1];
                }

                if (currentStage == "Stage 2")
                {
                    foreach(var i in stageLoadshedding.stages[4])
                    {
                        LblStage.Text = i;
                    }
                }

                if (currentStage == "Stage 3")
                {
                    LblStage.Text = stageLoadshedding.stage[3];
                }

                if (currentStage == "Stage 4")
                {
                    LblStage.Text = stageLoadshedding.stage[4];
                }

                if (currentStage == "Stage 5")
                {
                    LblStage.Text = stageLoadshedding.stage[5];
                }

                if (currentStage == "Stage 6")
                {
                    LblStage.Text = stageLoadshedding.stage[6];
                }

                if (currentStage == "Stage 7")
                {
                    LblStage.Text = stageLoadshedding.stage[7];
                }
            }
        }
    }
}