using loadshedding.Model;
using loadshedding.Services;
using System.Collections.ObjectModel;
using System.Text;

namespace loadshedding;

public partial class MainPage : ContentPage
{
    private double latitude;
    private double longitude;
    private readonly IWeatherServices _weatherServices;
    private readonly ILoadSheddingServices _loadsheddingServices;
   // private string AreaId;

    public MainPage(ILoadSheddingServices loadSheddingServices, IWeatherServices weatherServices)
    {
        InitializeComponent();

        _weatherServices = weatherServices;
        _loadsheddingServices = loadSheddingServices;

    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await GetLocation();
        
        await GetLoadSheddingByGPS(latitude, longitude);
        await GetWeatherByLocation(latitude, longitude);
        LblDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
        //await GetAreaLoadShedding(AreaId);
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
        await GetLoadSheddingByGPS(latitude, longitude);
        await GetWeatherByLocation(latitude, longitude);
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
            
            await GetLoadSheddingBySearch(response);
            await GetWeatherByCity(response);
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

            LblSchedulesEvetStart.Text = firstEvent.start.ToString("HH:mm");
            LblSchedulesEvetStop.Text = firstEvent.end.ToString("HH:mm");
            LblSchedulesCurrentStage.Text = firstEvent.note;
            LblDay.Text = loadSheddingAreaResults.schedule.days[0].name;


            string currentStage = LblSchedulesCurrentStage.Text;

            if (currentStage != null)
            {
                StringBuilder sb = new StringBuilder();

                var stageLoadshedding = loadSheddingAreaResults.schedule.days[0];

                switch (currentStage)
                {

                    case "Stage 0":
                        foreach (var schedule in stageLoadshedding.stages[0])
                        {
                            sb.Append(schedule).Append(" ");
                        }
                        break;

                    case "Stage 1":
                        foreach (var schedule in stageLoadshedding.stages[1])
                        {
                            sb.Append(schedule).Append("  ");
                        }
                        break;

                    case "Stage 2":
                        foreach (var schedule in stageLoadshedding.stages[2])
                        {
                            sb.Append(schedule).Append("  ");
                        }
                        break;

                    case "Stage 3":
                        foreach (var schedule in stageLoadshedding.stages[3])
                        {
                            sb.Append(schedule).Append("  ");
                        }
                        break;

                    case "Stage 4":
                        foreach (var schedule in stageLoadshedding.stages[4])
                        {
                            sb.Append(schedule).Append("  ");
                        }
                        break;

                    case "Stage 5":
                        foreach (var schedule in stageLoadshedding.stages[5])
                        {
                            sb.Append(schedule).Append("  ");
                        }
                        break;

                    case "Stage 6":
                        foreach (var schedule in stageLoadshedding.stages[6])
                        {
                            sb.Append(schedule).Append("  ");
                        }
                        break;

                    case "Stage 7":
                        foreach (var schedule in stageLoadshedding.stages[7])
                        {
                            sb.Append(schedule).Append("  ");
                        }
                        break;

                    case "Stage 8 (TESTING: current)":
                        foreach (var schedule in stageLoadshedding.stages[7])
                        {
                            sb.Append(schedule).Append("  ");
                        }
                        break;

                    default:
                        break;
                }

                if (sb != null && sb.Length > 0)
                {
                    LblStage.Text = sb.ToString();
                }
                else
                {
                    LblDate.Text = "There is no LoadShedding";
                }
            }
        }
    }
}