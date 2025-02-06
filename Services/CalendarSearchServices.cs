using Microsoft.Maui.Storage;
using System.Reflection;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using loadshedding.Interfaces;
using System.Text.RegularExpressions;

namespace loadshedding.Services
{

    public class CalendarSearchServices : ICalendarSearchServices
    {
        private readonly IAlertServices _alertServices;

        public CalendarSearchServices(IAlertServices alertServices)
        {
            _alertServices = alertServices;
        }

        public async Task<List<(string CalendarName, string AreaName)>> GetAreaBySearch(string text)
        {
            string fileName = "loadshedding.areametadata.json";
            return await ReadAreaMetadataAsync(fileName, text);
        }

        public async Task<List<(string CalendarName, string AreaName)>> ReadAreaMetadataAsync(string fileName, string text)
        {
            try
            {
                using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName);

                if (stream != null)
                {
                    using StreamReader reader = new StreamReader(stream);
                    string jsonContent = await reader.ReadToEndAsync();

                    using (JsonDocument jsonDocument = JsonDocument.Parse(jsonContent))
                    {
                        JsonElement root = jsonDocument.RootElement;
                        return SearchAreaDetails(root, text);
                    }
                }
                else
                {
                    await _alertServices.ShowAlert("File not found: " + fileName);
                    return null;
                }
            }
            catch (Exception ex)
            {
                await _alertServices.ShowAlert("Error: " + ex.Message);
                return null;
            }
        }

        private List<(string CalendarName, string AreaName)> SearchAreaDetails(JsonElement element, string text)
        {
            List<(string CalendarName, string AreaName)> matchingDetails = new();

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
                                        if (Regex.IsMatch(name.GetString() ?? "", $".*{Regex.Escape(text)}.*", RegexOptions.IgnoreCase))
                                        {
                                            matchingDetails.Add((calendarNameElement.GetString(), name.GetString()));
                                        }
                                    }
                                }
                                else if (names.ValueKind == JsonValueKind.String &&
                                         names.GetString()?.Contains(text, StringComparison.OrdinalIgnoreCase) == true)
                                {
                                    matchingDetails.Add((calendarNameElement.GetString(), names.GetString()));
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
