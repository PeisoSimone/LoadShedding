using loadshedding.CustomControl;
using loadshedding.Interfaces;
using System.Text;

namespace loadshedding;

public partial class MainPage : ContentPage
{
    private double latitude;
    private double longitude;

    private readonly IWeatherServices _weatherServices;
    private readonly ICalendarSearchServices _calendarSearchServices;
    private readonly ICalenderServices _calendarServices;
    private readonly IAlertServices _alertServices;
    private readonly ILoadSheddingStatusServices _loadSheddingStatusServices;
    private readonly INotificationServices _notificationServices;

    private readonly CircularProgressBarControl circularProgressBarControl;

    private dynamic loadSheddingOutages;
    private int loadSheddingStage;
    private const int StageOffset = 1;

    public DateTime EventStartTime { get; private set; }
    public DateTime EventEndTime { get; private set; }

    public MainPage(
        IWeatherServices weatherServices,
        ICalendarSearchServices calendarSearchServices,
        ICalenderServices calendarServices,
        IAlertServices alertServices,
        ILoadSheddingStatusServices loadSheddingStatusServices,
        INotificationServices notificationServices)
    {
        InitializeComponent();

        this._weatherServices = weatherServices ?? throw new ArgumentNullException(nameof(weatherServices));
        this._loadSheddingStatusServices = loadSheddingStatusServices ?? throw new ArgumentNullException(nameof(loadSheddingStatusServices));
        this._calendarSearchServices = calendarSearchServices ?? throw new ArgumentNullException(nameof(calendarSearchServices));
        this._calendarServices = calendarServices ?? throw new ArgumentNullException(nameof(calendarServices));
        this._alertServices = alertServices ?? throw new ArgumentNullException(nameof(alertServices));
        this._notificationServices = notificationServices ?? throw new ArgumentNullException(nameof(alertServices));

        circularProgressBarControl = new CircularProgressBarControl();
        circularProgressBarControl.UpdateProgressBar();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        ShowLoadingIndicator();

        await GetLoadSheddingStatus();

        try
        {
            string savedWeatherName = Preferences.Get("WeatherLocationName", string.Empty);
            string savedLoadSheddingName = Preferences.Get("LoadSheddingAreaLocationName", string.Empty);

            if (!string.IsNullOrWhiteSpace(savedLoadSheddingName) && !string.IsNullOrWhiteSpace(savedWeatherName))
            {
                await Task.WhenAll(
                    GetAreaLoadShedding(savedLoadSheddingName, loadSheddingStage),
                    GetWeatherBySearch(savedWeatherName)
                );
            }
            else
            {
                Preferences.Remove("WeatherLocationName");
                Preferences.Remove("LoadSheddingAreaLocationName");

                await GetLocation();
                await GetLocationByGPS(latitude, longitude);
            }

            LblDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
        }
        catch (Exception ex)
        {
            await _alertServices.ShowAlert($"Error in OnAppearing: {ex.Message}");
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
        try
        {
            var location = await Geolocation.GetLocationAsync();
            if (location != null)
            {
                latitude = location.Latitude;
                longitude = location.Longitude;
            }
            else
            {
                await _alertServices.ShowAlert("Failed to retrieve GPS location.");
            }
        }
        catch (Exception ex)
        {
            await _alertServices.ShowAlert($"Location error: {ex.Message}");
        }
    }

    public async Task GetLoadSheddingStatus()
    {
        try
        {
            var stage = await _loadSheddingStatusServices.GetNationalStatus();
            loadSheddingStage = stage.Status - StageOffset;//Stages are calculated as stage - 1
        }
        catch (Exception ex)
        {
            await _alertServices.ShowAlert($"Failed to fetch load shedding status: {ex.Message}");
        }
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
        await SearchLocation();
    }

    private async Task SearchLocation()
    {
        var response = await DisplayPromptAsync(
            title: "Search City",
            message: "",
            placeholder: "City Name...",
            accept: "SEARCH",
            cancel: "CANCEL"
        );

        if (!string.IsNullOrWhiteSpace(response))
        {
            string inputLocation = response.Trim();

            await Task.WhenAll(
                GetLoadSheddingBySearch(inputLocation),
                GetWeatherBySearch(inputLocation)
            );
        }
        else
        {
            await DisplayAlert("Search Cancelled", "No input provided.", "OK");
        }
    }


    public async Task GetLocationByGPS(double latitude, double longitude)
    {
        var areaLocationResults = await _weatherServices.GetWeatherByGPS(latitude, longitude);
        WeatherUpdateUI(areaLocationResults);

        string locationName = areaLocationResults.name;
        await GetLoadSheddingBySearch(locationName);
    }

    public async Task GetWeatherBySearch(string inputLocation)
    {
        var weatherBySearchResults = await _weatherServices.GetWeatherBySearch(inputLocation);
        if (weatherBySearchResults != null)
        {
            WeatherUpdateUI(weatherBySearchResults);
        }
        else
        {
            await DisplayAlert(
                   title: "",
                   message: "City Not Found. Try Again",
                   cancel: "OK");

            await SearchLocation();
        }
    }

    public void WeatherUpdateUI(dynamic weatherBySearchResults)
    {
        LblCity.Text = weatherBySearchResults.name;
        LblWeatherDescription.Text = weatherBySearchResults.weather[0].description;
        LblTemperature.Text = weatherBySearchResults.main.temperature + "?C";
    }

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

                var stage = await _loadSheddingStatusServices.GetNationalStatus();
                int loadSheddingStage = stage.Status - StageOffset; //Stages are calculated as stage - 1

                await GetAreaLoadShedding(selectedAreaNameCalendar, loadSheddingStage);
            }
            else
            {
                await DisplayAlert(
                   title: "",
                   message: "Location Not Selected!.",
                   cancel: "OK");
            }
        }
        else
        {
            await DisplayAlert(
               title: "",
               message: "Location Not Found! Try again",
               cancel: "OK");
        }
    }

    public async Task GetAreaLoadShedding(string selectedAreaNameCalendar, int laodSheddingStage)
    {
        var loadSheddingOutages = await _calendarServices.GetAreaOutages(selectedAreaNameCalendar, laodSheddingStage);
        LoadSheddingAreaUpdateUI(loadSheddingOutages);
    }

    public void LoadSheddingAreaUpdateUI(dynamic loadSheddingOutages)
    {
        DateTime day = DateTime.Now.Date;
        if (loadSheddingOutages != null && loadSheddingOutages.Count > 0)
        {
            LblScheduleAreaName.Text = loadSheddingOutages[0].Area;
            LblSchedulesCurrentStage.Text = loadSheddingOutages[0].Stage.ToString();
            LoadSheddingActive(loadSheddingOutages, day);
        }
        else
        {
            LoadSheddingSuspended();
        }
    }

    private void LoadSheddingActive(dynamic loadSheddingOutages, DateTime day)
    {
        List<dynamic> allDayEventsOutages = GetAllDayEventsOutages(loadSheddingOutages, day);

        if (allDayEventsOutages == null || allDayEventsOutages.Count <= 0)
        {
            LoadSheddingData();
            return;
        }

        List<dynamic> todayNextEventOutages = GetTodayNextEventOutages(allDayEventsOutages);

        var lastItem = allDayEventsOutages[allDayEventsOutages.Count - 1];
        DateTime lastEvenStart = lastItem.StartTime;
        DateTime now = DateTime.Now;

        if (lastEvenStart < now && loadSheddingOutages.Count <= 0)
        {
            LoadSheddingData();
            return;
        }

        if (todayNextEventOutages == null || todayNextEventOutages.Count <= 0)
        {
            LoadSheddingActive(loadSheddingOutages, day.AddDays(1));
            return;
        }

        var firstEvent = todayNextEventOutages[0];
        LblDay.Text = day.DayOfWeek.ToString();
        LblSchedulesCurrentStage.Text = "Stage " + firstEvent.Stage.ToString();
        EventStartTime = firstEvent.StartTime;
        EventEndTime = firstEvent.FinishTime.AddMinutes(-30);

        circularProgressBarControl.EventStartTime = EventStartTime;
        circularProgressBarControl.EventEndTime = EventEndTime;

        // Schedule 15-minute reminders
        DateTime notifyBeforeStart = EventStartTime.AddMinutes(-30);
        DateTime notifyBeforeEnd = EventEndTime.AddMinutes(-30);

        if (DateTime.Now > notifyBeforeStart && DateTime.Now < EventStartTime)
        {
            _notificationServices.ShowNotification("Upcoming LoadShedding", $"Power will go off at {EventStartTime:t}.", notifyBeforeStart);
        }

        if (DateTime.Now > notifyBeforeEnd && DateTime.Now < EventEndTime)
        {
            _notificationServices.ShowNotification("Power Restoration", $"Power will be restored at {EventEndTime:t}.", notifyBeforeEnd);
        }

        circularProgressBarControl.UpdateProgressBar();
        ProgressBarUpdated();

        this.loadSheddingOutages = loadSheddingOutages;

        LoadSheddingActiveHours(allDayEventsOutages, todayNextEventOutages);
    }


    private void LoadSheddingData()
    {
        LblSchedulesCurrentStage.Text = "Waiting for Eskom updates";
        LblDay.Text = DateTime.Today.DayOfWeek.ToString();
        LblDaySheduleList.Text = "Schedules to be available Soon";

        DateTime EventStartTime = DateTime.Now;
        DateTime EventEndTime = DateTime.Now;

        circularProgressBarControl.EventStartTime = EventStartTime;
        circularProgressBarControl.EventEndTime = EventEndTime;

        circularProgressBarControl.UpdateProgressBar();
        ProgressBarUpdated();

        FillBottomCardsUI();
    }

    private void LoadSheddingActiveHours(List<dynamic> todayOutages, List<dynamic> todayEventOutages)
    {
        var firstEvent = todayEventOutages[0];

        EventStartTime = firstEvent.StartTime;
        EventEndTime = firstEvent.FinishTime;

        string SchedulesEventStart = EventStartTime.ToString("HH:mm");
        string SchedulesEventEnd = EventEndTime.ToString("HH:mm");
        string ScheduleJoin = "-";

        StringBuilder ScheduleShow = new StringBuilder();
        ScheduleShow.Append(SchedulesEventStart);
        ScheduleShow.Append(ScheduleJoin);
        ScheduleShow.Append(SchedulesEventEnd);

        string scheduleHighlight = ScheduleShow.ToString();

        List<string> todayOutageDates = GetTodayOutagesDates(todayOutages);

        var formattedString = new FormattedString();

        NextScheduleHighlight(todayOutageDates, scheduleHighlight, formattedString);

        LblDaySheduleList.FormattedText = formattedString;
        LblDaySheduleList.SizeChanged += LblDaySheduleList_LayoutChanged;

        FillBottomCardsUI();
    }

    public void FillBottomCardsUI()
    {
        int dayOffset;
        if (EventStartTime.Date >= DateTime.Today && EventStartTime.Date <= DateTime.Today.AddDays(3))
        {
            dayOffset = (EventStartTime.Date - DateTime.Today).Days;
        }
        else
        {
            dayOffset = 0;
        }

        Label[] dayLabels = { LblNextDay, LblNextNextDay, LblNextNextNextDay };
        Label[] scheduleLabels = { LblNextSchedule, LblNextNextSchedule, LblNextNextNextSchedule };

        for (int i = 0; i < dayLabels.Length; i++)
        {
            DateTime targetDay = DateTime.Today.AddDays(i + 1 + dayOffset);

            dayLabels[i].Text = targetDay.DayOfWeek.ToString();

            if (loadSheddingOutages != null)
            {
                List<dynamic> allDayEventsOutages = GetAllDayEventsOutages(loadSheddingOutages, targetDay);
                List<string> outagesDates = GetTodayOutagesDates(allDayEventsOutages);

                if (outagesDates.Count > 0)
                {
                    scheduleLabels[i].Text = string.Join("\n", outagesDates);
                }
                else
                {
                    scheduleLabels[i].Text = "Events Unavailable";
                }
            }
            else
            {
                scheduleLabels[i].Text = "Schedules Unavailable";
            }
        }
    }

    private static List<dynamic> GetAllDayEventsOutages(dynamic loadSheddingOutages, DateTime day)
    {
        List<dynamic> todayOutages = new List<dynamic>();

        foreach (var outage in loadSheddingOutages)
        {
            string outageStart = outage.StartTime.Date.ToString();

            if (outageStart == day.ToString())
            {
                todayOutages.Add(outage);
            }
        }
        return todayOutages;
    }

    public static List<dynamic> GetTodayNextEventOutages(List<dynamic> todayOutages)
    {
        List<dynamic> todayEventOutages = new List<dynamic>();

        foreach (var outage in todayOutages)
        {
            DateTime currentTime = DateTime.Now;
            DateTime evenStart = outage.StartTime;
            DateTime evenFinish = outage.FinishTime;

            if (evenStart > currentTime || (evenStart <= currentTime && currentTime < evenFinish))
            {
                todayEventOutages.Add(outage);
            }
        }
        return todayEventOutages;
    }

    private static List<string> GetTodayOutagesDates(List<dynamic> todayOutages)
    {
        List<string> outageDates = new List<string>();

        foreach (var outage in todayOutages)
        {
            string outageStart = outage.StartTime.ToString("HH:mm");
            string outageFinish = outage.FinishTime.ToString("HH:mm");

            string concatenatedDates = $"{outageStart}-{outageFinish}";
            outageDates.Add(concatenatedDates);
        }
        return outageDates;
    }

    private void NextScheduleHighlight(List<string> todayOutageDates, string scheduleHighlight, FormattedString formattedString)
    {
        foreach (var sbText in todayOutageDates)
        {
            if (scheduleHighlight.Equals(sbText, StringComparison.OrdinalIgnoreCase))
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
                    FontSize = 20,
                };

                formattedString.Spans.Add(span);
            }
            else
            {
                formattedString.Spans.Add(new Span { Text = sbText + " " });
            }
        }
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

    private void LblDaySheduleList_LayoutChanged(object sender, EventArgs e)
    {
        Label lblDaySheduleList = (Label)sender;

        double availableWidth = lblDaySheduleList.Width;
        double fontSize = CalculateFontSize(availableWidth);
        lblDaySheduleList.FontSize = fontSize;
    }

    private static double CalculateFontSize(double availableWidth)
    {
        double baseFontSize = 15;
        double calculatedFontSize = baseFontSize * availableWidth / 300;
        return Math.Min(Math.Max(calculatedFontSize, 50), 15);
    }
}