using Itenso.TimePeriod;
using loadshedding.CustomControl;
using loadshedding.Services;
using Syncfusion.Maui.ProgressBar;
using System.Drawing;
using System.Text;

namespace loadshedding;

public partial class MainPage : ContentPage
{
    private double latitude;
    private double longitude;
    private readonly IWeatherServices _weatherServices;
    private readonly ILoadSheddingServices _loadsheddingServices;
    private CircularProgressBarControl circularProgressBarControl;
    private SfCircularProgressBar circularProgressBar;


    public DateTime EventStartTime { get; private set; }
    public DateTime EventEndTime { get; private set; }
    public DateTime secEventStartTime { get; private set; }
    public DateTime secEventEndTime { get; private set; }


    //Unomment below line for test mode
    //private string AreaId;

    public MainPage(ILoadSheddingServices loadSheddingServices, IWeatherServices weatherServices)
    {
        InitializeComponent();
        _weatherServices = weatherServices;
        _loadsheddingServices = loadSheddingServices;
        circularProgressBarControl = new CircularProgressBarControl();
        circularProgressBarControl.UpdateProgressBar();

    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await GetLocation();

        //Comment below line for test mode
        await GetLoadSheddingByGPS(latitude, longitude);

        await GetWeatherByLocation(latitude, longitude);
        LblDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

        //Uncomment out below line for test mode
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
            LoadSheddingActive(loadSheddingAreaResults);
        }
        else
        {
            LoadSheddingSuspended();
        }
    }

    private void LoadSheddingActive(dynamic loadSheddingAreaResults)
    {
        var firstEvent = loadSheddingAreaResults.events[0];

        //Comment below line for test mode
        var secondEvent = loadSheddingAreaResults.events[1];

        EventStartTime = firstEvent.start;
        EventEndTime = firstEvent.end;

        //Comment out below 2 lines for test mode
        secEventStartTime = secondEvent.start;
        secEventEndTime = secondEvent.end;

        circularProgressBarControl.EventStartTime = firstEvent.start;
        circularProgressBarControl.EventEndTime = firstEvent.end;

        //Comment out below line for test mode
        circularProgressBarControl.secEventStartTime = secondEvent.start;

        circularProgressBarControl.UpdateProgressBar();

        ProgressBarUpdated();

        LblSchedulesEventStart.Text = firstEvent.start.ToString("HH:mm");
        LblSchedulesEventStop.Text = firstEvent.end.ToString("HH:mm");
        LblSchedulesCurrentStage.Text = firstEvent.note;
        LblDay.Text = loadSheddingAreaResults.schedule.days[0].name;

        StageSwitch(loadSheddingAreaResults);

    }
    private void LoadSheddingSuspended()
    {
        LblSchedulesCurrentStage.Text = "LoadShedding Suspended";
        LblDay.Text = DateTime.Today.DayOfWeek.ToString();
        LblStage.Text = "No LoadShedding Today";

        DateTime EventStartTime = DateTime.Now;
        DateTime EventEndTime = DateTime.Now;

        circularProgressBarControl.EventStartTime = EventStartTime;
        circularProgressBarControl.EventEndTime = EventEndTime;

        circularProgressBarControl.UpdateProgressBar();

        ProgressBarUpdated();
    }
    private void ProgressBarUpdated()
    {
        StackLayout stackLayout = Content.FindByName<StackLayout>("Circular");
        if (stackLayout != null)
        {
            stackLayout.Children.Clear();
            stackLayout.Children.Add(circularProgressBarControl);
        }
    }

    private void StageSwitch(dynamic loadSheddingAreaResults)
    {
        string currentStage = LblSchedulesCurrentStage.Text;

        StringBuilder sb = new StringBuilder();

        var stageLoadshedding = loadSheddingAreaResults.schedule.days[0];

        switch (currentStage)
        {
            case "Stage 0":
                foreach (var schedule in stageLoadshedding.stages[0])
                {
                    sb.Append(schedule).Append("  ");
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
            //Test Mode case
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
            LoadSheddingActiveHours(loadSheddingAreaResults, sb);
        }
        else
        {
            LblStage.Text = "No LoadShedding Today";
        }
    }


    private void LoadSheddingActiveHours(dynamic loadSheddingAreaResults, StringBuilder sb)
    {
        var firstEvent = loadSheddingAreaResults.events[0];

        EventStartTime = firstEvent.start;
        EventEndTime = firstEvent.end;

        string SchedulesEventStart = EventStartTime.ToString("HH:mm");
        string SchedulesEventEnd = EventEndTime.ToString("HH:mm");
        string ScheduleJoin = "-";

        StringBuilder ScheduleShow = new StringBuilder();
        ScheduleShow.Append(SchedulesEventStart);
        ScheduleShow.Append(ScheduleJoin);
        ScheduleShow.Append(SchedulesEventEnd);

        string ScheduleHighlight = ScheduleShow.ToString();


        string[] sbTexts = sb.ToString().Split(" ");

        var formattedString = new FormattedString();

        foreach (var sbText in sbTexts)
        {
            if (ScheduleHighlight.Equals(sbText))
            {
                var span = new Span
                {
                    Text = sbText + " ",
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 25,
                };

                formattedString.Spans.Add(span);
            }
            else
            {
                formattedString.Spans.Add(new Span { Text = sbText + " " });
            }
        }

        LblStage.FormattedText = formattedString;
        LblStage.SizeChanged += LblStage_LayoutChanged;
    }
    private void LblStage_LayoutChanged(object sender, EventArgs e)
    {
        Label lblStage = (Label)sender;
        double availableWidth = lblStage.Width;
        double fontSize = CalculateFontSize(availableWidth);
        lblStage.FontSize = fontSize;
    }
    private double CalculateFontSize(double availableWidth)
    {
        double baseFontSize = 20;
        double calculatedFontSize = baseFontSize * availableWidth / 300;
        return Math.Min(Math.Max(calculatedFontSize, 50), 15);
    }
    
}