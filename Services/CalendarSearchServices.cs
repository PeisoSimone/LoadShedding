using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace loadshedding.Services
{
    public interface ICalendarSearchServices
    {
        Task<List<(string CalendarName, string AreaName)>> GetAreaBySearch(string text);
    }
    public class CalendarSearchServices : ICalendarSearchServices
    {
        private readonly IConfiguration _configuration;
        private readonly IAlertServices _alertServices;

        public CalendarSearchServices(IConfiguration configuration, IAlertServices alertServices)
        {
            _configuration = configuration;
            _alertServices = alertServices;
        }

        [Obsolete]
        public async Task<List<(string CalendarName, string AreaName)>> GetAreaBySearch(string text)
        {
            try
            {
                string localAppDataPath;
                if (Device.RuntimePlatform == Device.Android)
                {
                    localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                }
                else if (Device.RuntimePlatform == Device.iOS)
                {
                    localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                else // For other platforms, fallback to the original code
                {
                    localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                }
                string fileName = "metadata.json";
                string filePath = Path.Combine(localAppDataPath, fileName);


                string jsonContent = File.ReadAllText(filePath);
                JsonDocument jsonDocument = JsonDocument.Parse(jsonContent);
                JsonElement root = jsonDocument.RootElement;

                List<(string CalendarName, string AreaName)> matchingDetails = SearchAreaDetails(root, text);

                jsonDocument.Dispose();

                return matchingDetails;

            }
            catch (Exception ex)
            {
                await _alertServices.ShowAlert("Read File Error: " + ex.Message);
                return null;
            }
        }



        private List<(string CalendarName, string AreaName)> SearchAreaDetails(JsonElement element, string text)
        {
            List<(string CalendarName, string AreaName)> matchingDetails = new List<(string, string)>();
            if (element.TryGetProperty("area_details", out JsonElement areaDetails))
            {
                foreach (var areaDetail in areaDetails.EnumerateArray())
                {
                    if (areaDetail.TryGetProperty("calendar_name", out JsonElement calendarNameElement) &&
                        areaDetail.TryGetProperty("areas", out JsonElement areas))
                    {
                        foreach (var area in areas.EnumerateArray())
                        {
                            if (area.TryGetProperty("name", out JsonElement names))
                            {
                                if (names.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var name in names.EnumerateArray())
                                    {
                                        if (name.GetString()?.Contains(text, StringComparison.OrdinalIgnoreCase) == true)
                                        {
                                            matchingDetails.Add((calendarNameElement.GetString(), name.GetString()));
                                            break;
                                        }
                                    }
                                }
                                else if (names.ValueKind == JsonValueKind.String &&
                                         names.GetString()?.Contains(text, StringComparison.OrdinalIgnoreCase) == true)
                                {
                                    matchingDetails.Add((calendarNameElement.GetString(), names.GetString()));
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return matchingDetails;
        }
    }
}