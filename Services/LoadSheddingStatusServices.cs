using loadshedding.Interfaces;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using loadshedding.Models;

namespace loadshedding.Services
{
    public class LoadSheddingStatusServices : ILoadSheddingStatusServices
    {
        private readonly HttpClient _httpClient;
        private readonly IAlertServices _alertServices;
        private readonly INotificationServices _notificationServices;
        private readonly ICalenderServices _calenderServices;
        private int _lastStage = -1;

        public LoadSheddingStatusServices(
            HttpClient httpClient,
            IAlertServices alertServices,
            INotificationServices notificationServices,
            ICalenderServices calenderServices
            )
        {
            _httpClient = httpClient;
            _alertServices = alertServices;
            _notificationServices = notificationServices;
            _calenderServices = calenderServices;
        }

        public async Task<StatusRoot> GetNationalStatus()
        {
            try
            {
                const string endpoint = "LoadShedding/GetStatus";
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var statusValue = await response.Content.ReadAsStringAsync();
                    if (int.TryParse(statusValue, out int stage))
                    {
                        if (_lastStage != -1 && _lastStage != stage)
                        {
                            if (_lastStage < 0)
                            {
                                _notificationServices.ShowNotification("Load-Shedding Update", $"LoadShedding is Active: Stage {stage}");

                                string savedLoadSheddingName = Preferences.Get("LoadSheddingAreaLocationName", string.Empty);

                                if (!string.IsNullOrWhiteSpace(savedLoadSheddingName) )
                                {
                                    await _calenderServices.GetAreaOutages(savedLoadSheddingName, stage);
                                }

                            }
                            else if (stage < 1)
                            {
                                _notificationServices.ShowNotification("Load-Shedding Update", $"LoadShedding is Suspended");
                            }
                            else 
                            {
                                _notificationServices.ShowNotification("Load-Shedding Update", $"Stage changed to {stage}");
                            }
                        }
                        _lastStage = stage;
                        return new StatusRoot { Status = stage };
                    }

                    AnalyticsHelper.TrackEvent("LoadSheddingStatusFailure",
                        new Dictionary<string, string> { { "Invalid response format from LoadShedding API", $"{response.StatusCode}" } });

                    await _alertServices.ShowAlert("Invalid response format from LoadShedding API");
                    return null;
                }

                AnalyticsHelper.TrackEvent("LoadSheddingStatusFailure",
                    new Dictionary<string, string> { { "Could not get National LoadShedding Status. Status Code", $"{response.StatusCode}" } });

                await _alertServices.ShowAlert($"Could not get National LoadShedding Status. Status Code: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                AnalyticsHelper.TrackEvent("LoadSheddingStatusFailure",
                    new Dictionary<string, string> { { "GetNationalStatus failed", $"{ex.Message}" } });

                await _alertServices.ShowAlert($"GetNationalStatus failed: {ex.Message}");
                return null;
            }
        }
    }
}