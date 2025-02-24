using loadshedding.Interfaces;
using loadshedding.Models;

namespace loadshedding.Services
{
    public class LoadSheddingStatusServices : ILoadSheddingStatusServices
    {
        private readonly HttpClient _httpClient;
        private readonly IAlertServices _alertServices;

        public LoadSheddingStatusServices(HttpClient httpClient, IAlertServices alertServices)
        {
            _httpClient = httpClient;
            _alertServices = alertServices;
        }

        public async Task<StatusRoot> GetNationalStatus()
        {
            try
            {
                const string endpoint = "LoadShedding/GetStatus";
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var statusValue = await response.Content.ReadAsStringAsync();
                    if (int.TryParse(statusValue, out int stage))
                    {
                        return new StatusRoot { Status = stage };
                    }

                    await _alertServices.ShowAlert("Invalid response format from LoadShedding API");
                    return null;
                }

                await _alertServices.ShowAlert($"Could not get National LoadShedding Status. Status Code: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                await _alertServices.ShowAlert($"GetNationalStatus failed: {ex.Message}");
                return null;
            }
        }
    }
}
