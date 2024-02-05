using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace loadshedding.Services
{
    public class CalendarSearchServices
    {
        private readonly IAlertServices _alertServices;

        public CalendarSearchServices( IAlertServices alertServices)
        {
            _alertServices = alertServices;
        }
        public async Task<List<(string CalendarName, string AreaName)>> GetAreaBySearch(string text)
        {
            try
            {
                string fileName = "areametadata.json";
                string jsonFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);

                if (File.Exists(jsonFilePath))
                {
                    string jsonContent = File.ReadAllText(jsonFilePath);
                    JsonDocument jsonDocument = JsonDocument.Parse(jsonContent);
                    JsonElement root = jsonDocument.RootElement;

                    List<(string CalendarName, string AreaName)> matchingDetails = SearchAreaDetails(root, text);

                    jsonDocument.Dispose();

                    return matchingDetails;
                }
                else
                {

                    await _alertServices.ShowAlert("Cannot find filepath");
                    return null;
                }
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