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

        public WeatherServices(HttpClient httpClient, ApiKeysConfiguration apiKeys, IAlertServices alertServices)
        {
            _httpClient = httpClient;
            _weatherApiKey = apiKeys.WeatherApiKey;
            _alertServices = alertServices;
        }

        public async Task<WeatherRoot> GetWeatherByGPS(double latitude, double longitude)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"weather?lat={latitude}&lon={longitude}&units=metric&appid={_weatherApiKey}");
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<WeatherRoot>();
                    SaveLocationSettings(content?.name);
                    return content;
                }
                else
                {
                    await _alertServices.ShowAlert($"GetWeatherByGPS-API request failed with status code: {response.StatusCode}");
                    ClearWeatherSettings();
                    return null;
                }
            }
            catch (Exception ex)
            {
                await _alertServices.ShowAlert($"GetWeatherByGPS-An error occurred: {ex.Message}");
                return null;
            }
        }

        public async Task<WeatherRoot> GetWeatherBySearch(string text)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"weather?q={text}&units=metric&appid={_weatherApiKey}");
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<WeatherRoot>();
                    SaveLocationSettings(content?.name);
                    return content;
                }
                else
                {
                    await _alertServices.ShowAlert($"GetWeatherBySearch-API request failed with status code: {response.StatusCode}");
                    ClearWeatherSettings();
                    return null;
                }
            }
            catch (Exception ex)
            {
                await _alertServices.ShowAlert($"GetWeatherBySearch-An error occurred: {ex.Message}");
                return null;
            }
        }

        public void ClearWeatherSettings()
        {
            Preferences.Remove("WeatherLocationName");
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
