using loadshedding.Interfaces;
using loadshedding.Model;
using System.Net.Http.Json;


namespace loadshedding.Services
{
    public class WeatherServices : IWeatherServices
    {
        private readonly string _weatherApiKey;
        private readonly HttpClient _httpClient;
        private readonly IAlertServices _alertServices;
        private readonly INotificationServices _notificationServices;
        private string _lastLocation = null;

        public WeatherServices(
            HttpClient httpClient,
            ApiKeysConfiguration apiKeys,
            IAlertServices alertServices,
            INotificationServices notificationServices)
        {
            _httpClient = httpClient;
            _weatherApiKey = apiKeys.WeatherApiKey;
            _alertServices = alertServices;
            _notificationServices = notificationServices;
        }

        public async Task<WeatherRoot> GetWeatherByGPS(double latitude, double longitude)
        {
            string endpoint = $"weather?lat={latitude}&lon={longitude}&units=metric&appid={_weatherApiKey}";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<WeatherRoot>();

                    //if (_lastLocation != null && _lastLocation != content?.name)
                    //{
                    //    _notificationServices.ShowNotification("Weather Location Update", $"Weather location changed to {content?.name}");
                    //    AnalyticsHelper.TrackWeatherLocationChange(_lastLocation, content?.name);
                    //}
                    _lastLocation = content?.name;

                    SaveLocationSettings(content?.name);
                    return content;
                }
                else
                {
                    AnalyticsHelper.TrackWeatherApiFailure(
                        "GetWeatherByGPS",
                        "API request failed",
                        response.StatusCode.ToString());

                    await _alertServices.ShowAlert($"GetWeatherByGPS-API request failed with status code: {response.StatusCode}");
                    ClearWeatherSettings();
                    return null;
                }
            }
            catch (Exception ex)
            {
                AnalyticsHelper.TrackWeatherApiFailure("GetWeatherByGPS", ex.Message);
                AnalyticsHelper.ReportHandledException(ex, new Dictionary<string, string>
                {
                    { "Method", "GetWeatherByGPS" },
                    { "Latitude", latitude.ToString() },
                    { "Longitude", longitude.ToString() }
                });

                await _alertServices.ShowAlert($"GetWeatherByGPS-An error occurred: {ex.Message}");
                return null;
            }
        }

        public async Task<WeatherRoot> GetWeatherBySearch(string text)
        {
            string endpoint = $"weather?q={text}&units=metric&appid={_weatherApiKey}";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<WeatherRoot>();

                    //if (_lastLocation != null && _lastLocation != content?.name)
                    //{
                    //    _notificationServices.ShowNotification("Weather Location Update", $"Weather location changed to {content?.name}");
                    //    AnalyticsHelper.TrackWeatherLocationChange(_lastLocation, content?.name);
                    //}
                    _lastLocation = content?.name;

                    SaveLocationSettings(content?.name);
                    return content;
                }
                else
                {
                    AnalyticsHelper.TrackWeatherApiFailure(
                        "GetWeatherBySearch",
                        "API request failed",
                        response.StatusCode.ToString());

                    await _alertServices.ShowAlert($"GetWeatherBySearch-API request failed with status code: {response.StatusCode}");
                    ClearWeatherSettings();
                    return null;
                }
            }
            catch (Exception ex)
            {
                AnalyticsHelper.TrackWeatherApiFailure("GetWeatherBySearch", ex.Message);
                AnalyticsHelper.ReportHandledException(ex, new Dictionary<string, string>
                {
                    { "Method", "GetWeatherBySearch" },
                    { "SearchText", text }
                });

                await _alertServices.ShowAlert($"GetWeatherBySearch-An error occurred: {ex.Message}");
                return null;
            }
        }

        public void ClearWeatherSettings()
        {
            Preferences.Remove("WeatherLocationName");
            _lastLocation = null;
        }

        public void SaveLocationSettings(string weatherName)
        {
            if (weatherName != null)
            {
                Preferences.Set("WeatherLocationName", weatherName);
            }
        }
    }
}
