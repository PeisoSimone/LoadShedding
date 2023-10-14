
using loadshedding.Model;
using Newtonsoft.Json;
using System.Net.Http;


namespace loadshedding.Services
{
    public static class WeatherServices
    {
        public static async Task<WeatherRoot> GetWeatherByGPS(double latitude, double longitude)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(string.Format("https://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&units=metric&appid=f9269e5ecd3313a8bab2ed1d692a92b9", latitude, longitude));
            return JsonConvert.DeserializeObject<WeatherRoot>(response);
        }

        public static async Task<WeatherRoot> GetWeatherByCity(string city)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(string.Format("https://api.openweathermap.org/data/2.5/weather?q={0}&units=metric&appid=f9269e5ecd3313a8bab2ed1d692a92b9", city));
            return JsonConvert.DeserializeObject<WeatherRoot>(response);
        }
    }

    public static class LoadSheddingServices
    {
            public static async Task<StatusRoot> GetStatus()
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("token", "8ED8EBBF-FEBE40C3-89D33271-27C7A791");
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                    return null;
                }
            }
        }
    }
