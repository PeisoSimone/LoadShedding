using loadshedding.Model;
using Newtonsoft.Json;
using System.Net.Http.Json;


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
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("token", "18F86816-BCBC4CFA-B0DF9CB8-CA2E2DA7");
                HttpResponseMessage response = await client.GetAsync("https://developer.sepush.co.za/business/2.0/status");

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content?.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<StatusRoot>(content);
                }
                else
                {
                    Console.WriteLine("API request failed with status code: " + response.StatusCode);
                    return null;
                }
            }
        }

        public static async Task<AreasNearbyGPSRoot> GetAreasNearByGPS(double latitude,double longitude)
        {
            var id = "";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("token", "18F86816-BCBC4CFA-B0DF9CB8-CA2E2DA7");
                    HttpResponseMessage response = await client.GetAsync($"https://developer.sepush.co.za/business/2.0/areas_nearby?lat={latitude}&lon={longitude}");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content?.ReadFromJsonAsync<AreasNearbyGPSRoot>();

                        if (content != null)
                        {
                            id = content.areas[0].id;
                            var areaInfo = await GetAreaInformation(id);
                        }
                        return content;
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


        public static async Task<AreaInformationRoot> GetAreaInformation(string id)
        {
            id = "tshwane-16-onderstepoortext9";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("token", "18F86816-BCBC4CFA-B0DF9CB8-CA2E2DA7");
                    HttpResponseMessage response = await client.GetAsync($"https://developer.sepush.co.za/business/2.0/area?id={id}&test=current");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content?.ReadFromJsonAsync<AreaInformationRoot>();
                        return content;
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