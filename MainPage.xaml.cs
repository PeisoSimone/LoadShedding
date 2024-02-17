using loadshedding.CustomControl;
using loadshedding.Services;
using Syncfusion.Maui.ProgressBar;
using System.Text;


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

    private bool isLoading;
    private object todayOutages;
    private dynamic loadSheddingOutages;

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
                await GetAreaLoadShedding(savedLoadSheddingName);
                await GetWeatherBySearch(savedWeatherName);
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
        LblTemperature.Text = weatherBySearchResults.main.temperature + "°C";
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

                await GetAreaLoadShedding(selectedAreaNameCalendar);

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

    public async Task GetAreaLoadShedding(string selectedAreaNameCalendar)
    {
        var loadSheddingOutages = await _calendarAPIServices.GetAreaOutages(selectedAreaNameCalendar);
        LoadSheddingAreaUpdateUI(loadSheddingOutages);
    }

    public void LoadSheddingAreaUpdateUI(dynamic loadSheddingOutages)
    {
        DateTime day = DateTime.Now.Date;
        if (loadSheddingOutages != null)
        { 
           
            LblScheduleAreaName.Text = loadSheddingOutages[0].area_name;
            LblSchedulesCurrentStage.Text = loadSheddingOutages[0].stage.ToString();
            LoadSheddingActive(loadSheddingOutages,day);
        }
        else
        {
            LoadSheddingSuspended();
        }
    }

    private void LoadSheddingActive(dynamic loadSheddingOutages, DateTime day )
    {
        List<dynamic> allDayEventsOutages = GetAllDayEventsOutages(loadSheddingOutages, day);
        List<dynamic> todayNextEventOutages = GetTodayNextEventOutages(allDayEventsOutages);

        for (int i = 0; i < loadSheddingOutages.Count; i++)
        {
            if (todayNextEventOutages.Count <=0)
            {
                LoadSheddingActive(loadSheddingOutages, day.AddDays(1));
                return;
            }
            else
            {
                LblDay.Text = day.DayOfWeek.ToString();

                var firstEvent = todayNextEventOutages[0];

                LblScheduleAreaName.Text = firstEvent.area_name;
                LblSchedulesCurrentStage.Text = "Stage " + firstEvent.stage.ToString();

                EventStartTime = firstEvent.start;
                EventEndTime = firstEvent.finsh;

                EventEndTime = EventEndTime.AddMinutes(-30);

                circularProgressBarControl.EventStartTime = EventStartTime;
                circularProgressBarControl.EventEndTime = EventEndTime;

                circularProgressBarControl.UpdateProgressBar();
                ProgressBarUpdated();
            }
        }
        
        this.loadSheddingOutages = loadSheddingOutages;

        LoadSheddingActiveHours(allDayEventsOutages, todayNextEventOutages);
    }

    private void LoadSheddingActiveHours(List<dynamic> todayOutages, List<dynamic> todayEventOutages)
    {
        var firstEvent = todayEventOutages[0];

        EventStartTime = firstEvent.start;
        EventEndTime = firstEvent.finsh;

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
        for (int i = 1; i < 4; i++)
        {
            if (i == 1)
            {
                DateTime nextDay = DateTime.Today.AddDays(i);
                LblNextDay.Text = nextDay.DayOfWeek.ToString();

                if (loadSheddingOutages != null)
                {
                    DateTime day = DateTime.Now.Date.AddDays(i);
                    List<dynamic> allDayEventsOutages = GetAllDayEventsOutages(loadSheddingOutages, day);
                    List<string> todayOutageDates = GetTodayOutagesDates(allDayEventsOutages);

                    if(todayOutageDates.Count>0)
                    {
                        string joinedSchedule = string.Join("\n", todayOutageDates);
                        LblNextSchedule.Text = joinedSchedule;
                    }
                    else
                    {
                        LblNextSchedule.Text = "Data Not Available";
                    }
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

                if (loadSheddingOutages != null)
                {
                    DateTime day = DateTime.Now.Date.AddDays(i);
                    List<dynamic> allDayEventsOutages = GetAllDayEventsOutages(loadSheddingOutages, day);
                    List<string> todayOutageDates = GetTodayOutagesDates(allDayEventsOutages);

                    if (todayOutageDates.Count > 0)
                    {
                        string joinedSchedule = string.Join("\n", todayOutageDates);
                        LblNextNextSchedule.Text = joinedSchedule;
                    }
                    else
                    {
                        LblNextNextSchedule.Text = "Data Not Available";
                    }
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

                if (loadSheddingOutages != null)
                {
                    DateTime day = DateTime.Now.Date.AddDays(i);
                    List<dynamic> allDayEventsOutages = GetAllDayEventsOutages(loadSheddingOutages, day);
                    List<string> todayOutageDates = GetTodayOutagesDates(allDayEventsOutages);

                    if (todayOutageDates.Count > 0)
                    {
                        string joinedSchedule = string.Join("\n", todayOutageDates);
                        LblNextNextNextSchedule.Text = joinedSchedule;
                    }
                    else
                    {
                        LblNextNextNextSchedule.Text = "Data Not Available" ;
                    }

                }
                else
                {
                    LblNextNextNextSchedule.Text = "Suspended till further notice";
                }
            }
        }
    }

    private List<dynamic> GetAllDayEventsOutages(dynamic loadSheddingOutages, DateTime day)
    {
        List<dynamic> todayOutages = new List<dynamic>();

        foreach (var outage in loadSheddingOutages)
        {
            string outageStart = outage.start.Date.ToString();

            if (outageStart == day.ToString())
            {
                todayOutages.Add(outage);
            }
        }
        return todayOutages;
    }

    public List<dynamic> GetTodayNextEventOutages(List<dynamic> todayOutages)
    {
        List<dynamic> todayEventOutages = new List<dynamic>();

        foreach (var outage in todayOutages)
        {
            DateTime currentTime = DateTime.Now;
            DateTime evenStart = outage.start;
            DateTime evenFinish = outage.finsh;

            if (evenStart > currentTime || (evenStart <= currentTime && currentTime < evenFinish))
            {
                todayEventOutages.Add(outage);
            }
        }
        return todayEventOutages;
    }

    private List<string> GetTodayOutagesDates(List<dynamic> todayOutages)
    {
        List<string> outageDates = new List<string>();

        foreach (var outage in todayOutages)
        {
            string outageStart = outage.start.ToString("HH:mm");
            string outageFinish = outage.finsh.ToString("HH:mm");

            string concatenatedDates = $"{outageStart}-{outageFinish}";
            outageDates.Add(concatenatedDates);
        }
        return outageDates;
    }

    private void NextScheduleHighlight(List<string> todayOutageDates, string scheduleHighlight, FormattedString formattedString)
    {
        foreach (var sbText in todayOutageDates)
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

    private double CalculateFontSize(double availableWidth)
    {
        double baseFontSize = 15;
        double calculatedFontSize = baseFontSize * availableWidth / 300;
        return Math.Min(Math.Max(calculatedFontSize, 50), 15);
    }
}