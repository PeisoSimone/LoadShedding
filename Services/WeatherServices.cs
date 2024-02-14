using loadshedding.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding.Services
{
    public interface IWeatherServices
    {
        Task<WeatherRoot> GetWeatherByGPS(double latitude, double longitude);
        Task<WeatherRoot> GetWeatherBySearch(string text);
    }

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
                    Console.WriteLine("GetWeatherByGPS-API request failed with status code: " + response.StatusCode);
                    ClearWeatherSettings();
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetWeatherByGPS-An error occurred: " + ex.Message);
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
                    Console.WriteLine("GetWeatherBySearch-API request failed with status code: " + response.StatusCode);
                    ClearWeatherSettings();
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetWeatherBySearch-An error occurred: " + ex.Message);
                return null;
            }
        }

        public void ClearWeatherSettings()
        {
            Preferences.Remove("WeatherLocationName");
        }

        private void SaveLocationSettings(string weatherName)
        {
            Preferences.Set("WeatherLocationName", weatherName);
        }
    }
}
