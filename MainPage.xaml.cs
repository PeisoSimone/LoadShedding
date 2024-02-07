using loadshedding.CustomControl;
using loadshedding.Services;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.ProgressBar;
using System.Text;
using System.Threading.Channels;
using static System.Net.Mime.MediaTypeNames;


namespace loadshedding;

public partial class MainPage : ContentPage
{
    private double latitude;
    private double longitude;

    private readonly IWeatherServices _weatherServices;
    private readonly ICalendarSearchServices _calendarSearchServices;
    private readonly ICalenderAPIServices _calendarAPIServices;
    private readonly IAlertServices _alertServices;

    private CircularProgressBarControl circularProgressBarControl;
    private SfCircularProgressBar circularProgressBar;

    private List<string> dayResults;
    private bool isLoading;

    public DateTime EventStartTime { get; private set; }
    public DateTime EventEndTime { get; private set; }

    public MainPage(IWeatherServices weatherServices, ICalendarSearchServices calendarSearchServices, ICalenderAPIServices calendarAPIServices, IAlertServices alertServices)
    {
        InitializeComponent();
        _weatherServices = weatherServices;
        _calendarSearchServices = calendarSearchServices;
        _calendarAPIServices = calendarAPIServices;
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
            string savedLoadSheddingName = Preferences.Get("LoadSheddingAreaLocationName", string.Empty);

            if (savedLoadSheddingName.Length > 0 && savedWeatherName.Length > 0)
            {
                await GetLoadSheddingBySearch(savedLoadSheddingName);
                await GetWeatherBySearch(savedWeatherName);
            }
            else
            {
                Preferences.Remove("WeatherLocationName");
                Preferences.Remove("LoadSheddingAreaLocationName");
                await GetLocation();
                await GetLocationByGPS(latitude, longitude);
                //await GetLoadSheddingByGPS(latitude, longitude);
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
            await GetLocationByGPS(latitude, longitude);
        }
        else
        {
            await DisplayAlert(
                title: "",
                message: "Action Cancelled.",
                cancel: "OK");
        }
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
            string inputLocation = response.Trim();

            await GetLoadSheddingBySearch(inputLocation);
            await GetWeatherBySearch(inputLocation);
        }
        else
        {
            await DisplayAlert(
                title: "",
                message: "Action Cancelled.",
                cancel: "OK");
        }
    }

    public async Task GetLocationByGPS(double latitude, double longitude)
    {
        var areaLocationResults = await _weatherServices.GetWeatherByGPS(latitude, longitude);
        WeatherUpdateUI(areaLocationResults);

        string locationGPS = areaLocationResults.name;
        await GetLoadSheddingBySearch(locationGPS);

        //results name should upadate GetAreaLoadShedding(string AreaId) areaId == name
    }

    public async Task GetWeatherBySearch(string inputLocation)
    {
        var weatherBySearchResults = await _weatherServices.GetWeatherBySearch(inputLocation);
        WeatherUpdateUI(weatherBySearchResults);
    }

    public void WeatherUpdateUI(dynamic weatherBySearchResults)
    {
        LblCity.Text = weatherBySearchResults.name;
        LblWeatherDescription.Text = weatherBySearchResults.weather[0].description;
        LblTemperature.Text = weatherBySearchResults.main.temperature + "°C";
    }

    //public async Task GetLoadSheddingByGPS(double latitude, double longitude)
    //{
    //    var loadSheddingAreaGPSResults = await _loadsheddingServices.GetAreasNearByGPS(latitude, longitude);

    //    string[] areaNames = loadSheddingAreaGPSResults.areas.Select(area => area.name).ToArray();

    //    if (loadSheddingAreaGPSResults.areas.Count > 0)
    //    {
    //        var selectedAreaName = await DisplayActionSheet(
    //        "Current LoadShedding Area",
    //        "CANCEL",
    //        null,
    //        areaNames);

    //        if (selectedAreaName != null)
    //        {
    //            // User selected an area
    //            string areaId = loadSheddingAreaGPSResults.areas
    //                .First(area => area.name == selectedAreaName).id;
    //            await GetAreaLoadShedding(areaId);
    //        }
    //        else
    //        {
    //            await DisplayAlert(
    //               title: "",
    //               message: "Location Not Found!.",
    //               cancel: "OK");
    //        }
    //    }
    //    else
    //    {
    //        await DisplayAlert(
    //          title: "",
    //          message: "Location Not Found!.",
    //          cancel: "OK");

    //    }
    //}

    //Update Area Search
    public async Task GetLoadSheddingBySearch(string inputLocation)
    {
        var areaSearchResults = await _calendarSearchServices.GetAreaBySearch(inputLocation);

        List<(string DisplayOption, string CalendarName)> displayOptions = areaSearchResults
            .Select(detail => (DisplayOption: $" {detail.AreaName}", CalendarName: detail.CalendarName))
            .ToList();

        if (displayOptions.Count > 0)
        {
            List<string> displayStrings = displayOptions.Select(option => option.DisplayOption).ToList();

            string selectedOption = await DisplayActionSheet("Select Area", "Cancel", null, displayStrings.ToArray());

            if (!string.IsNullOrEmpty(selectedOption) && selectedOption != "Cancel")
            {
                string selectedCalendarName = displayOptions.First(option => option.DisplayOption == selectedOption).CalendarName;

                string selectedAreaNameCalendar = Path.GetFileNameWithoutExtension(selectedCalendarName);

                await GetAreaLoadShedding(selectedAreaNameCalendar);

            }
            else
            {
                await DisplayAlert(
                   title: "",
                   message: "Location Not Found!.",
                   cancel: "OK");
            }
        }
        else
        {
            await DisplayAlert(
               title: "",
               message: "Location Not Found!.",
               cancel: "OK");
        }
    }

    public async Task GetAreaLoadShedding(string selectedAreaNameCalendar)
    {
        //var loadSheddingSchedules = await _calendarAPIServices.GetAreaSchedules(selectedAreaNameCalendar);
        var loadSheddingOutages = await _calendarAPIServices.GetAreaOutages(selectedAreaNameCalendar);

        LoadSheddingAreaUpdateUI(loadSheddingOutages);
    }


    //Attention
    public void LoadSheddingAreaUpdateUI(dynamic loadSheddingOutages)
    {

        if (loadSheddingOutages != null)
        {
            LblScheduleAreaName.Text = loadSheddingOutages[0].area_name;
            LoadSheddingActive(loadSheddingOutages);
        }
        else
        {
            LoadSheddingSuspended();
        }
    }

    //Attention
    private void LoadSheddingActive(dynamic loadSheddingOutages)
    {
        var firstEvent = loadSheddingOutages[0];

        EventStartTime = firstEvent.start;
        EventEndTime = firstEvent.finsh;

        EventEndTime = EventEndTime.AddMinutes(-30);

        circularProgressBarControl.EventStartTime = EventStartTime;
        circularProgressBarControl.EventEndTime = EventEndTime;

        circularProgressBarControl.UpdateProgressBar();
        ProgressBarUpdated();


        LblSchedulesCurrentStage.Text = firstEvent.stage;

        //Check if the next LoadShedding event is today or the following day
        if (EventStartTime.Date == DateTime.Today.Date)
        {
            //Day name Today
            LblDay.Text = DateTime.Today.DayOfWeek.ToString();
        }
        else if (EventStartTime.Date == DateTime.Today.AddDays(1).Date)
        {
            //Day name of tomorrow
            LblDay.Text = DateTime.Today.AddDays(1).DayOfWeek.ToString();

        }
        else if (EventStartTime.Date == DateTime.Today.AddDays(2).Date)
        {
            //Day name after tomorrow
            LblDay.Text = DateTime.Today.AddDays(2).DayOfWeek.ToString();

        }

        //StageSwitch(loadSheddingOutages);
        LoadSheddingActiveHours(loadSheddingOutages);
    }

    private void LoadSheddingSuspended()
    {
        LblSchedulesCurrentStage.Text = "LoadShedding Suspended";
        LblDay.Text = DateTime.Today.DayOfWeek.ToString();
        LblDaySheduleList.Text = "No LoadShedding Today";

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

    //private void StageSwitch(dynamic loadSheddingOutages)
    //{
    //    string currentStage = LblSchedulesCurrentStage.Text;

    //    dayResults = new List<string>();

    //    StringBuilder sb = new StringBuilder();

    //    if (loadSheddingAreaResults.schedule != null)
    //    {
    //        var scheduleDays = loadSheddingAreaResults.schedule.days;

    //        for (int i = 0; i < scheduleDays.Count; i++)
    //        {
    //            var stageLoadshedding = scheduleDays[i];

    //            switch (currentStage)
    //            {
    //                case "Stage 0":
    //                    foreach (var schedule in stageLoadshedding.stages[0])
    //                    {
    //                        sb.Append(schedule).Append(" ");
    //                    }
    //                    break;

    //                case "Stage 1":
    //                    foreach (var schedule in stageLoadshedding.stages[1])
    //                    {
    //                        sb.Append(schedule).Append(" ");
    //                    }
    //                    break;

    //                case "Stage 2":
    //                    foreach (var schedule in stageLoadshedding.stages[2])
    //                    {
    //                        sb.Append(schedule).Append(" ");
    //                    }
    //                    break;

    //                case "Stage 3":
    //                    foreach (var schedule in stageLoadshedding.stages[3])
    //                    {
    //                        sb.Append(schedule).Append(" ");
    //                    }
    //                    break;

    //                case "Stage 4":
    //                    foreach (var schedule in stageLoadshedding.stages[4])
    //                    {
    //                        sb.Append(schedule).Append(" ");
    //                    }
    //                    break;

    //                case "Stage 5":
    //                    foreach (var schedule in stageLoadshedding.stages[5])
    //                    {
    //                        sb.Append(schedule).Append(" ");
    //                    }
    //                    break;

    //                case "Stage 6":
    //                    foreach (var schedule in stageLoadshedding.stages[6])
    //                    {
    //                        sb.Append(schedule).Append(" ");
    //                    }
    //                    break;

    //                case "Stage 7":
    //                    foreach (var schedule in stageLoadshedding.stages[7])
    //                    {
    //                        sb.Append(schedule).Append(" ");
    //                    }
    //                    break;
    //                //Test Mode case
    //                case "Stage 8 (TESTING: current)":
    //                    foreach (var schedule in stageLoadshedding.stages[7])
    //                    {
    //                        sb.Append(schedule).Append(" ");
    //                    }
    //                    break;

    //                default:
    //                    break;
    //            }

    //            dayResults.Add(sb.ToString());
    //            sb.Clear();
    //        }
    //        if (dayResults != null)
    //        {
    //            LoadSheddingActiveHours(loadSheddingOutages);
    //        }
    //    }
    //}

    private void LoadSheddingActiveHours(dynamic loadSheddingOutages)
    {
        var firstEvent = loadSheddingOutages[0];

        Dictionary<DateTime, List<dynamic>> groupedByDate = new Dictionary<DateTime, List<dynamic>>();

        foreach (var outage in loadSheddingOutages)
        {
            DateTime startDate = DateTime.Parse(outage.start).Date;

            if (!groupedByDate.ContainsKey(startDate))
            {
                groupedByDate[startDate] = new List<dynamic>();
            }
            groupedByDate[startDate].Add(outage);
        }

        var sortedGroups = groupedByDate.OrderBy(pair => pair.Key);
        List<List<dynamic>> groupedItems = sortedGroups.Select(pair => pair.Value).ToList();


        EventStartTime = firstEvent.start;
        EventEndTime = firstEvent.finsh;

        string SchedulesEventStart = EventStartTime.ToString("HH:mm");
        string SchedulesEventEnd = EventEndTime.ToString("HH:mm");
        string ScheduleJoin = "-";

        StringBuilder ScheduleShow = new StringBuilder();
        ScheduleShow.Append(SchedulesEventStart);
        ScheduleShow.Append(ScheduleJoin);
        ScheduleShow.Append(SchedulesEventEnd);

        string scheduleHighlight = ScheduleShow.ToString();//Create a string of start to stop events on [0]

        string result = dayResults[0].ToString();

        string[] sbTexts = result.ToString().Split(" ");

        var formattedString = new FormattedString();

        NextScheduleHighlight(sbTexts, scheduleHighlight, formattedString);

        //LblDaySheduleList.FormattedText = formattedString;
        //LblDaySheduleList.SizeChanged += LblDaySheduleList_LayoutChanged;

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
                    //LblOccure.Text = "Next Schedule";
                }
                else if (EventStartTime < DateTime.Now)
                {
                    //LblOccure.Text = "Active Now";
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
                //LblNextDay.Text = nextDay.DayOfWeek.ToString();

                if (dayResults != null)
                {
                    string scheduleContent = dayResults[i].ToString();
                    string[] scheduleArray = scheduleContent.Split(' ');
                    string joinedSchedule = string.Join("\n", scheduleArray);
                    //LblNextSchedule.Text = joinedSchedule;
                }
                else
                {
                    //LblNextSchedule.Text = "Suspended till further notice";
                }
            }
            else if (i == 2)
            {
                DateTime nextDay = DateTime.Today.AddDays(i);
                //LblNextNextDay.Text = nextDay.DayOfWeek.ToString();

                if (dayResults != null)
                {
                    string scheduleContent = dayResults[i].ToString();
                    string[] scheduleArray = scheduleContent.Split(' ');
                    string joinedSchedule = string.Join("\n", scheduleArray);
                   // LblNextNextSchedule.Text = joinedSchedule;
                }
                else
                {
                   // LblNextNextSchedule.Text = "Suspended till further notice";
                }
            }
            else if (i == 3)
            {
                DateTime nextDay = DateTime.Today.AddDays(i);
               // LblNextNextNextDay.Text = nextDay.DayOfWeek.ToString();

                if (dayResults != null)
                {
                    string scheduleContent = dayResults[i].ToString();
                    string[] scheduleArray = scheduleContent.Split(' ');
                    string joinedSchedule = string.Join("\n", scheduleArray);
                   // LblNextNextNextSchedule.Text = joinedSchedule;
                }
                else
                {
                   // LblNextNextNextSchedule.Text = "Suspended till further notice";
                }
            }
        }
    }

    private void LblDaySheduleList_LayoutChanged(object sender, EventArgs e)
    {
        Label lblDaySheduleList = (Label)sender;
        double availableWidth = lblDaySheduleList.Width;
        double fontSize = CalculateFontSize(availableWidth);
        lblDaySheduleList.FontSize = fontSize;
    }
    private double CalculateFontSize(double availableWidth)
    {
        double baseFontSize = 20;
        double calculatedFontSize = baseFontSize * availableWidth / 300;
        return Math.Min(Math.Max(calculatedFontSize, 50), 15);
    }
}