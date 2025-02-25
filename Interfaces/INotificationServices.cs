using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding.Interfaces
{
    public interface INotificationServices
    {
        void ShowNotification(string title, string message, DateTime? scheduleTime = null);
    }
}
