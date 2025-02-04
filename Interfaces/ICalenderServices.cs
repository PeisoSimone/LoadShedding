using loadshedding.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding.Interfaces
{
    public interface ICalenderServices
    {
        Task<List<OutagesRoot>> GetAreaOutages(string area, int stage);
        void SaveLoadSheddingAreaSettings(string area);
        void ClearLoadSheddingAreaSettings();
    }
}
