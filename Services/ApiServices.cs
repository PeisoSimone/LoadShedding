using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http.Json;
using loadshedding.Model;

namespace loadshedding.Services
{

    public static class WeatherServices
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string WeatherApiKey = "f9269e5ecd3313a8bab2ed1d692a92b9";

        public static async Task<WeatherRoot> GetWeatherByGPS(double latitude, double longitude)
        {
            try
            {
                string url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&units=metric&appid={WeatherApiKey}";
                string response = await httpClient.GetStringAsync(url);
                return JsonConvert.DeserializeObject<WeatherRoot>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return null;
            }
        }

        public static async Task<WeatherRoot> GetWeatherByCity(string text)
        {
            try
            {
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={text}&units=metric&appid={WeatherApiKey}";
                string response = await httpClient.GetStringAsync(url);
                return JsonConvert.DeserializeObject<WeatherRoot>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return null;
            }
        }
    }

    public static class LoadSheddingServices
    {
        public static string AreaId;

        private const string SepushToken = "18F86816-BCBC4CFA-B0DF9CB8-CA2E2DA7";

        public static async Task<StatusRoot> GetStatus()
        {
            var client = new HttpClient();
            try
            {
                client.DefaultRequestHeaders.Add("token", SepushToken);
                HttpResponseMessage response = await client.GetAsync("https://developer.sepush.co.za/business/2.0/status");

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<StatusRoot>(content);
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

        public static async Task<AreaSearchRoot> GetAreaBySearch(string text)
        {
            var client = new HttpClient();
            try
            {
                client.DefaultRequestHeaders.Add("token", SepushToken);
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://developer.sepush.co.za/business/2.0/areas_search?text={text}");
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<AreaSearchRoot>();

                    if (content != null && content.areas.Count > 0)
                    {

                        string id = content.areas[0].id;
                        AreaId = id;
                    }

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

        public static async Task<AreasNearbyGPSRoot> GetAreasNearByGPS(double latitude, double longitude)
        {
            var client = new HttpClient();
            try
            {
                client.DefaultRequestHeaders.Add("token", SepushToken);
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://developer.sepush.co.za/business/2.0/areas_nearby?lat={latitude}&lon={longitude}");
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<AreasNearbyGPSRoot>();

                    if (content != null && content.areas.Count > 0)
                    {

                        string id = content.areas[0].id;
                        AreaId = id;
                    }

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

        public static async Task<AreaInformationRoot> GetAreaInformation(string AreaId)
        {

            var client = new HttpClient();
            try
            {
                client.DefaultRequestHeaders.Add("token", SepushToken);
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://developer.sepush.co.za/business/2.0/area?id={AreaId}&test=current");

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<AreaInformationRoot>();
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
    }
}