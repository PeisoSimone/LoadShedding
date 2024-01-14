using loadshedding.Model;
using System.Net.Http.Json;

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

        public WeatherServices(HttpClient httpClient, ApiKeysConfiguration apiKeys)
        {
            _httpClient = httpClient;
            _weatherApiKey = apiKeys.WeatherApiKey;
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
                    Console.WriteLine("API request failed with status code: " + response.StatusCode);
                    ClearWeatherSettings();
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
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
                    Console.WriteLine("API request failed with status code: " + response.StatusCode);
                    ClearWeatherSettings();
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return null;
            }
        }
        public void ClearWeatherSettings()
        {
            // Clear the saved location settings
            Preferences.Remove("WeatherLocationName");
        }

        private void SaveLocationSettings(string weatherName)
        {
            // Save location name using Preferences (persistent storage)
            Preferences.Set("WeatherLocationName", weatherName);
        }
    }


    public interface ILoadSheddingServices
    {
        Task<AreaSearchRoot> GetAreaBySearch(string text);
        Task<AreasNearbyGPSRoot> GetAreasNearByGPS(double latitude, double longitude);
        Task<AreaInformationRoot> GetAreaInformation(string AreaId);
    }

    public class LoadSheddingServices : ILoadSheddingServices
    {
        private readonly string _loadSheddingApiKey;
        private readonly HttpClient _httpClient;

        public LoadSheddingServices(HttpClient httpClient, ApiKeysConfiguration apiKeys)
        {
            _httpClient = httpClient;
            _loadSheddingApiKey = apiKeys.LoadSheddingApiKey;
        }


        public async Task<AreasNearbyGPSRoot> GetAreasNearByGPS(double latitude, double longitude)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("token", _loadSheddingApiKey);
                var request = new HttpRequestMessage(HttpMethod.Get, $"areas_nearby?lat={latitude}&lon={longitude}");
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<AreasNearbyGPSRoot>();

                    return content;
                }
                else
                {
                    Console.WriteLine("API request failed with status code: " + response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return null;
            }
        }

        public async Task<AreaSearchRoot> GetAreaBySearch(string text)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("token", _loadSheddingApiKey);
                var request = new HttpRequestMessage(HttpMethod.Get, $"areas_search?text={text}");
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<AreaSearchRoot>();
                    return content;
                }
                else
                {
                    Console.WriteLine("API request failed with status code: " + response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return null;
            }
        }

        public async Task<AreaInformationRoot> GetAreaInformation(string AreaId)
        {
            try
            {
                //Uncomment below line for test mode
                //AreaId = "tshwane-16-onderstepoortext9";

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("token", _loadSheddingApiKey);

                //Comment below line for test mode
                //var request = new HttpRequestMessage(HttpMethod.Get, $"area?id={AreaId}");

                //Uncomment below line for test mode
                var request = new HttpRequestMessage(HttpMethod.Get, $"area?id={AreaId}&test=current");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<AreaInformationRoot>();
                    SaveLoadSheddingSettings(AreaId);
                    return content;
                }
                else
                {
                    ClearLoadSheddingSettings();
                    Console.WriteLine("API request failed with status code: " + response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return null;
            }
        }

        private void SaveLoadSheddingSettings(string loadSheddingName)
        {
            // Save location name using Preferences (persistent storage)
            Preferences.Set("LoadSheddingLocationName", loadSheddingName);
        }

        public void ClearLoadSheddingSettings()
        {
            // Clear the saved location settings
            Preferences.Remove("LoadSheddingLocationName");
        }
    }
}
