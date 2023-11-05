﻿using loadshedding.Model;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace loadshedding.Services
{
    public interface IWeatherServices
    {
        Task<WeatherRoot> GetWeatherByGPS(double latitude, double longitude);
        Task<WeatherRoot> GetWeatherByCity(string text);
    }

    public class WeatherServices : IWeatherServices
    {
        private readonly string _weatherApiKey = "f9269e5ecd3313a8bab2ed1d692a92b9";

        private readonly HttpClient _httpClient = new ()

            {
                BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/")
            };

    public async Task<WeatherRoot> GetWeatherByGPS(double latitude, double longitude)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"weather?lat={latitude}&lon={longitude}&units=metric&appid={_weatherApiKey}");
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<WeatherRoot>();
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

        public async Task<WeatherRoot> GetWeatherByCity(string text)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"weather?q={text}&units=metric&appid={_weatherApiKey}");
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<WeatherRoot>();
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


    public interface ILoadSheddingServices
    {
        Task<AreaSearchRoot> GetAreaBySearch(string text);
        Task<AreasNearbyGPSRoot> GetAreasNearByGPS(double latitude, double longitude);
        Task<AreaInformationRoot> GetAreaInformation(string AreaId);
    }

    public class LoadSheddingServices : ILoadSheddingServices
    {
        private readonly string _loadSheddingKey = "8ED8EBBF-FEBE40C3-89D33271-27C7A791";

        private readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("https://developer.sepush.co.za/business/2.0/")
        };


        public async Task<AreasNearbyGPSRoot> GetAreasNearByGPS(double latitude, double longitude)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("token", _loadSheddingKey);
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
                _httpClient.DefaultRequestHeaders.Add("token", _loadSheddingKey);
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
                _httpClient.DefaultRequestHeaders.Clear();
                //AreaId = "tshwane-16-onderstepoortext9";
                _httpClient.DefaultRequestHeaders.Add("token", _loadSheddingKey);
                var request = new HttpRequestMessage(HttpMethod.Get, $"area?id={AreaId}");

                var response = await _httpClient.SendAsync(request);

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
