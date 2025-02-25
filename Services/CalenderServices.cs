using CsvHelper;
using loadshedding.Interfaces;
using loadshedding.Model;
using loadshedding.Models;
using System.Formats.Asn1;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;


namespace loadshedding.Services
{
    public class CalenderServices : ICalenderServices
    {
        private readonly IAlertServices _alertServices;
        private readonly INotificationServices _notificationServices;
        private readonly HttpClient _httpClient;
        private string _lastArea = null;

        public CalenderServices(
            IHttpClientFactory httpClientFactory,
            IAlertServices alertServices,
            INotificationServices notificationServices)
        {
            _httpClient = httpClientFactory.CreateClient("Supabase");
            _alertServices = alertServices;
            _notificationServices = notificationServices;
        }

        public async Task<List<ScheduleRoot>> GetAreaOutages(string area, int stage)
        {
            int todayDay = DateTime.Today.Day;
            int maxDay = todayDay + 3;
            string endpoint = $"schedule?area=eq.{area}&stage=eq.{stage}&date_of_month=gte.{todayDay}&date_of_month=lte.{maxDay}";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    List<ScheduleRoot> content = await response.Content.ReadFromJsonAsync<List<ScheduleRoot>>();

                    if (_lastArea != null && _lastArea != area)
                    {
                        _notificationServices.ShowNotification("Load-Shedding Area Update", $"Area changed to {area}");
                        AnalyticsHelper.TrackLoadSheddingAreaChange(_lastArea, area);
                    }
                    _lastArea = area;

                    SaveLoadSheddingAreaSettings(area);
                    return content;
                }
                else
                {
                    AnalyticsHelper.TrackCalendarApiFailure(
                        "GetAreaOutages",
                        "API request failed",
                        response.StatusCode.ToString());

                    await _alertServices.ShowAlert($"Area Not Found: {response.StatusCode}");
                    ClearLoadSheddingAreaSettings();
                    return null;
                }
            }
            catch (Exception ex)
            {
                AnalyticsHelper.TrackCalendarApiFailure("GetAreaOutages", ex.Message);
                AnalyticsHelper.ReportHandledException(ex, new Dictionary<string, string>
                {
                    { "Method", "GetAreaOutages" },
                    { "Area", area },
                    { "Stage", stage.ToString() }
                });

                await _alertServices.ShowAlert($"Area, error occurred: {ex.Message}");
                return null;
            }
        }

        public void SaveLoadSheddingAreaSettings(string area)
        {
            Preferences.Set("LoadSheddingAreaLocationName", area);
        }

        public void ClearLoadSheddingAreaSettings()
        {
            Preferences.Remove("LoadSheddingAreaLocationName");
            _lastArea = null;
        }
    }
}