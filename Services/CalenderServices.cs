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
        private readonly HttpClient _httpClient;

        public CalenderServices(IHttpClientFactory httpClientFactory, IAlertServices alertServices)
        {
            _httpClient = httpClientFactory.CreateClient("Supabase");
            _alertServices = alertServices;
        }

        public async Task<List<ScheduleRoot>> GetAreaOutages(string area, int stage)
        {
            int todayDay = DateTime.Today.Day;
            int maxDay = todayDay + 3;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"schedule?area=eq.{area}&stage=eq.{stage}&date_of_month=gte.{todayDay}&date_of_month=lte.{maxDay}");
                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    List<ScheduleRoot> content = await response.Content.ReadFromJsonAsync<List<ScheduleRoot>>();
                    SaveLoadSheddingAreaSettings(area);
                    return content;
                }
                else
                {
                    await _alertServices.ShowAlert($"GetAreaOutages-API request failed with status code: {response.StatusCode}");
                    ClearLoadSheddingAreaSettings();
                    return null;
                }
            }
            catch (Exception ex)
            {
                await _alertServices.ShowAlert($"GetAreaOutages-An error occurred: {ex.Message}");
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
        }
    }
}