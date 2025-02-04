using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding.Interfaces
{
    public interface ICalendarSearchServices
    {
        Task<List<(string CalendarName, string AreaName)>> GetAreaBySearch(string text);
    }
}
