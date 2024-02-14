using loadshedding.Model;
using System.Net.Http.Json;



namespace loadshedding.Services
{
    public interface ICalenderAPIServices
    {
        Task<List<OutagesRoot>> GetAreaOutages(string area);
    }

    public class CalenderAPIServices : ICalenderAPIServices
    {
        private readonly HttpClient _httpClient;
        private readonly IAlertServices _alertServices;

        public CalenderAPIServices(HttpClient httpClient, IAlertServices alertServices)
        {
            _httpClient = httpClient;
            _alertServices = alertServices;
        }


        public async Task<List<OutagesRoot>> GetAreaOutages(string area)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                var request = new HttpRequestMessage(HttpMethod.Get, $"outages/{area}");
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    List<OutagesRoot> content = await response.Content.ReadFromJsonAsync<List<OutagesRoot>>();
                    SaveLoadSheddingAreaSettings(area);
                    return content;
                }
                else
                {
                    await _alertServices.ShowAlert("GetAreaOutages-API request failed with status code: " + response.StatusCode);
                    ClearLoadSheddingAreaSettings();
                    return null;
                }
            }
            catch (Exception ex)
            {
                await _alertServices.ShowAlert("GetAreaOutages-An error occurred: " + ex.Message);
                return null;
            }
        }

        private void SaveLoadSheddingAreaSettings(string area)
        {
            Preferences.Set("LoadSheddingAreaLocationName", area);
        }

        public void ClearLoadSheddingAreaSettings()
        {
            Preferences.Remove("LoadSheddingAreaLocationName");
        }
    }
}
