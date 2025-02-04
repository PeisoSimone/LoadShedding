using CsvHelper;
using loadshedding.Interfaces;
using loadshedding.Model;
using System.Formats.Asn1;
using System.Globalization;
using System.Net.Http.Json;



namespace loadshedding.Services
{
    public class CalenderServices : ICalenderServices
    {
        private readonly IAlertServices _alertServices;
        private readonly HttpClient _httpClient;

        public CalenderServices(HttpClient httpClient, IAlertServices alertServices)
        {
            _httpClient = httpClient;
            _alertServices = alertServices;
        }
        public async Task<List<OutagesRoot>> GetAreaOutages(string area, int stage)
        {
            int staged = 2;
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                var request = new HttpRequestMessage(HttpMethod.Get, $"/{area}?stage={staged}");
                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    List<OutagesRoot> content = await response.Content.ReadFromJsonAsync<List<OutagesRoot>>();
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