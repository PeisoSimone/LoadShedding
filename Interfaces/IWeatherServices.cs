using loadshedding.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding.Interfaces
{
    public interface IWeatherServices
    {
        Task<WeatherRoot> GetWeatherByGPS(double latitude, double longitude);
        Task<WeatherRoot> GetWeatherBySearch(string text);
        void ClearWeatherSettings();
        void SaveLocationSettings(string weatherName);
    }
}
