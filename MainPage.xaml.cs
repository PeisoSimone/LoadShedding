using loadshedding.CustomControl;
using loadshedding.Services;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.ProgressBar;
using System.Text;


namespace loadshedding;

public partial class MainPage : ContentPage
{
    private double latitude;
    private double longitude;
    private readonly IWeatherServices _weatherServices;
    private readonly ILoadSheddingServices _loadsheddingServices;
    private readonly IAlertServices _alertServices;
    private CircularProgressBarControl circularProgressBarControl;
    private SfCircularProgressBar circularProgressBar;
    private List<string> dayResults;
    private bool isLoading;
    public DateTime EventStartTime { get; private set; }
    public DateTime EventEndTime { get; private set; }

    public MainPage(ILoadSheddingServices loadSheddingServices, IWeatherServices weatherServices, IAlertServices alertServices)
    {
        InitializeComponent();
        _weatherServices = weatherServices;
        _loadsheddingServices = loadSheddingServices;
        _alertServices = alertServices;
        circularProgressBarControl = new CircularProgressBarControl();
        circularProgressBarControl.UpdateProgressBar();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        ShowLoadingIndicator();

        try
        {
            string savedWeatherName = Preferences.Get("WeatherLocationName", string.Empty);
            string savedLoadSheddingName = Preferences.Get("LoadSheddingLocationName", string.Empty);

            if (savedLoadSheddingName.Length > 0 && savedWeatherName.Length > 0)
            {
                await GetAreaLoadShedding(savedLoadSheddingName);
                await GetWeatherBySearch(savedWeatherName);
            }
            else
            {
                Preferences.Remove("WeatherLocationName");
                Preferences.Remove("LoadSheddingLocationName");
                await GetLocation();
                await GetWeatherByGPS(latitude, longitude);
                await GetLoadSheddingByGPS(latitude, longitude);
            }

            LblDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
        }
        catch (Exception ex) 
        {
            await _alertServices.ShowAlert("onAppearing-An error occurred: " + ex.Message);
        }
        finally
        {
            HideLoadingIndicator();
        }
    }

    private void ShowLoadingIndicator()
    {
        loadingIndicatorContainer.IsVisible = true;
        loadingIndicator.IsRunning = true;
        loadingIndicator.IsVisible = true;
    }

    private void HideLoadingIndicator()
    {
        loadingIndicator.IsRunning = false;
        loadingIndicator.IsVisible = false;
        loadingIndicatorContainer.IsVisible = false;
    }

    public async Task GetLocation()
    {
        var location = await Geolocation.GetLocationAsync();
        latitude = location.Latitude;
        longitude = location.Longitude;
    }

    private async void Mylocation_Clicked(object sender, EventArgs e)
    {
        var response = await DisplayAlert(
            title: "",
            message: "Detect Current Location?",
            accept: "OK",
            cancel: "CANCEL");

        if (response)
        {
            await GetLocation();
            await GetLoadSheddingByGPS(latitude, longitude);
            await GetWeatherByGPS(latitude, longitude);
        }
        else
        {
            await DisplayAlert(
                title: "",
                message: "Action Cancelled.",
                cancel: "OK");
        }
    }

    public async Task GetWeatherByGPS(double latitude, double longitude)
    {
        var results = await _weatherServices.GetWeatherByGPS(latitude, longitude);
        WeatherUpdateUI(results);
    }

    private async void SearchLocation_Clicked(object sender, EventArgs e)
    {
        var response = await DisplayPromptAsync(
            title: "Search City",
            message: "",
            placeholder: "City Name...",
            accept: "SEARCH",
            cancel: "CANCEL"
            );

        if (response != null)
        {
            string results = response.Trim();

            await GetLoadSheddingBySearch(results);
            await GetWeatherBySearch(results);
        }
        else
        {
            await DisplayAlert(
                title: "",
                message: "Action Cancelled.",
                cancel: "OK");
        }
    }

    public async Task GetWeatherBySearch(string text)
    {
        var results = await _weatherServices.GetWeatherBySearch(text);
        WeatherUpdateUI(results);
    }

    public void WeatherUpdateUI(dynamic results)
    {
        LblCity.Text = results.name;
        LblWeatherDescription.Text = results.weather[0].description;
        LblTemperature.Text = results.main.temperature + "°C";
    }

    public async Task GetLoadSheddingByGPS(double latitude, double longitude)
    {
        var loadSheddingAreaGPSResults = await _loadsheddingServices.GetAreasNearByGPS(latitude, longitude);

        string[] areaNames = loadSheddingAreaGPSResults.areas.Select(area => area.name).ToArray();
        var selectedAreaName = await DisplayActionSheet(
            "Current LoadShedding Area",
            "CANCEL",
            null,
            areaNames);

        if (selectedAreaName != null && selectedAreaName != "CANCEL")
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
        var selectedAreaName = await DisplayActionSheet(
            "Select LoadShedding Area",
            "CANCEL",
            null,
            areaNames);

        if (selectedAreaName != null && selectedAreaName != "CANCEL")
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

        EventStartTime = firstEvent.start;
        EventEndTime = firstEvent.end;

        EventEndTime = EventEndTime.AddMinutes(-30);

        circularProgressBarControl.EventStartTime = EventStartTime;
        circularProgressBarControl.EventEndTime = EventEndTime;

        circularProgressBarControl.UpdateProgressBar();
        ProgressBarUpdated();

        LblSchedulesCurrentStage.Text = firstEvent.note;

        //Check if the next LoadShedding event is today or the following day
        if (EventStartTime.Date == DateTime.Today.Date)
        {
            LblDay.Text = loadSheddingAreaResults.schedule.days[0].name;
        }
        else
        {
            LblDay.Text = loadSheddingAreaResults.schedule.days[1].name;
        }

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

        FillBottomCardsUI();
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

        dayResults = new List<string>();

        StringBuilder sb = new StringBuilder();

        if (loadSheddingAreaResults.schedule != null)
        {
            var scheduleDays = loadSheddingAreaResults.schedule.days;

            for (int i = 0; i < scheduleDays.Count; i++)
            {
                var stageLoadshedding = scheduleDays[i];

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
                            sb.Append(schedule).Append(" ");
                        }
                        break;

                    case "Stage 2":
                        foreach (var schedule in stageLoadshedding.stages[2])
                        {
                            sb.Append(schedule).Append(" ");
                        }
                        break;

                    case "Stage 3":
                        foreach (var schedule in stageLoadshedding.stages[3])
                        {
                            sb.Append(schedule).Append(" ");
                        }
                        break;

                    case "Stage 4":
                        foreach (var schedule in stageLoadshedding.stages[4])
                        {
                            sb.Append(schedule).Append(" ");
                        }
                        break;

                    case "Stage 5":
                        foreach (var schedule in stageLoadshedding.stages[5])
                        {
                            sb.Append(schedule).Append(" ");
                        }
                        break;

                    case "Stage 6":
                        foreach (var schedule in stageLoadshedding.stages[6])
                        {
                            sb.Append(schedule).Append(" ");
                        }
                        break;

                    case "Stage 7":
                        foreach (var schedule in stageLoadshedding.stages[7])
                        {
                            sb.Append(schedule).Append(" ");
                        }
                        break;
                    //Test Mode case
                    case "Stage 8 (TESTING: current)":
                        foreach (var schedule in stageLoadshedding.stages[7])
                        {
                            sb.Append(schedule).Append(" ");
                        }
                        break;

                    default:
                        break;
                }

                dayResults.Add(sb.ToString());
                sb.Clear();
            }
            if (dayResults != null)
            {
                LoadSheddingActiveHours(loadSheddingAreaResults);
            }
        }
    }

    private void LoadSheddingActiveHours(dynamic loadSheddingAreaResults)
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

        string scheduleHighlight = ScheduleShow.ToString();

        string result = dayResults[0].ToString();

        string[] sbTexts = result.ToString().Split(" ");

        var formattedString = new FormattedString();

        NextScheduleHighlight(sbTexts, scheduleHighlight, formattedString);

        LblStage.FormattedText = formattedString;
        LblStage.SizeChanged += LblStage_LayoutChanged;

        FillBottomCardsUI();
    }

    private void NextScheduleHighlight(string[] sbTexts, string scheduleHighlight, FormattedString formattedString)
    {
        foreach (var sbText in sbTexts)
        {
            if (scheduleHighlight.Equals(sbText))
            {
                if (EventStartTime > DateTime.Now)
                {
                    LblOccure.Text = "Next Schedule";
                }
                else if (EventStartTime < DateTime.Now)
                {
                    LblOccure.Text = "Active Now";
                }

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
    }

    public void FillBottomCardsUI()
    {
        for (int i = 1; i < 4; i++)
        {
            if (i == 1)
            {
                DateTime nextDay = DateTime.Today.AddDays(i);
                LblNextDay.Text = nextDay.DayOfWeek.ToString();

                if (dayResults != null)
                {
                    string scheduleContent = dayResults[i].ToString();
                    string[] scheduleArray = scheduleContent.Split(' ');
                    string joinedSchedule = string.Join("\n", scheduleArray);
                    LblNextSchedule.Text = joinedSchedule;
                }
                else
                {
                    LblNextSchedule.Text = "Suspended till further notice";
                }
            }
            else if (i == 2)
            {
                DateTime nextDay = DateTime.Today.AddDays(i);
                LblNextNextDay.Text = nextDay.DayOfWeek.ToString();

                if (dayResults != null)
                {
                    string scheduleContent = dayResults[i].ToString();
                    string[] scheduleArray = scheduleContent.Split(' ');
                    string joinedSchedule = string.Join("\n", scheduleArray);
                    LblNextNextSchedule.Text = joinedSchedule;
                }
                else
                {
                    LblNextNextSchedule.Text = "Suspended till further notice";
                }
            }
            else if (i == 3)
            {
                DateTime nextDay = DateTime.Today.AddDays(i);
                LblNextNextNextDay.Text = nextDay.DayOfWeek.ToString();

                if (dayResults != null)
                {
                    string scheduleContent = dayResults[i].ToString();
                    string[] scheduleArray = scheduleContent.Split(' ');
                    string joinedSchedule = string.Join("\n", scheduleArray);
                    LblNextNextNextSchedule.Text = joinedSchedule;
                }
                else
                {
                    LblNextNextNextSchedule.Text = "Suspended till further notice";
                }
            }
        }
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