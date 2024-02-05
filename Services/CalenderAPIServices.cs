using loadshedding.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding.Services
{
    public interface ICalenderAPIServices
    {
        Task<OutagesRoot> GetAreaOutages(string area);
        Task<SchedulesRoot> GetAreaSchedules(string area);
    }

    public class CalenderAPIServices
    {
        private readonly HttpClient _httpClient;
        private readonly IAlertServices _alertServices;

        public CalenderAPIServices(HttpClient httpClient,IAlertServices alertServices)
        {
            _httpClient = httpClient;
            _alertServices = alertServices;
        }

        public async Task<OutagesRoot> GetAreaOutages(string area)
        {

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://eskom-calendar-api.shuttleapp.rs/outages/{area}");
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<OutagesRoot>();
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

        public async Task<SchedulesRoot> GetAreaSchedules(string area)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://eskom-calendar-api.shuttleapp.rs/schedules/{area}");
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<SchedulesRoot>();
                    SaveLoadSheddingAreaSettings(area);
                    return content;
                }
                else
                {
                    await _alertServices.ShowAlert("GetAreaSchedules-API request failed with status code: " + response.StatusCode);
                    ClearLoadSheddingAreaSettings();
                    return null;
                }
            }
            catch (Exception ex)
            {
                await _alertServices.ShowAlert("GetAreaSchedules-An error occurred: " + ex.Message);
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
