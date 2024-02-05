using loadshedding.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding.Services
{
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
        private readonly IAlertServices _alertServices;

        public LoadSheddingServices(HttpClient httpClient, ApiKeysConfiguration apiKeys, IAlertServices alertServices)
        {
            _httpClient = httpClient;
            _loadSheddingApiKey = apiKeys.LoadSheddingApiKey;
            _alertServices = alertServices;
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
                    await _alertServices.ShowAlert("GetAreasNearByGPS-API request failed with status code: " + response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                await _alertServices.ShowAlert("GetAreasNearByGPS-An error occurred: " + ex.Message);
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
                    await _alertServices.ShowAlert("GetAreaBySearch-API request failed with status code: " + response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                await _alertServices.ShowAlert("GetAreaBySearch-An error occurred: " + ex.Message);
                return null;
            }
        }

        public async Task<AreaInformationRoot> GetAreaInformation(string AreaId)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("token", _loadSheddingApiKey);

                var request = new HttpRequestMessage(HttpMethod.Get, $"area?id={AreaId}");


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
                    await _alertServices.ShowAlert("GetAreaInformation-API request failed with status code: " + response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                await _alertServices.ShowAlert("GetAreaInformation-An error occurred: " + ex.Message);
                return null;
            }
        }

        private void SaveLoadSheddingSettings(string loadSheddingName)
        {
            Preferences.Set("LoadSheddingLocationName", loadSheddingName);
        }

        public void ClearLoadSheddingSettings()
        {
            Preferences.Remove("LoadSheddingLocationName");
        }
    }
}
