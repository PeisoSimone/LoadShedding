using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding.Model
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Outage
    {
        public string StartTime { get; set; }
        public string FinshTime { get; set; }
        public int Stage { get; set; }
        public string Recurrence { get; set; }
        public int Day1OfRecurrence { get; set; }
    }

    public class SchedulesRoot
    {
        public int Id { get; set; }
        public List<Outage> Outages { get; set; }
        public List<object> Source { get; set; }
        public List<object> Info { get; set; }
        public object LastUpdated { get; set; }
        public object ValidFrom { get; set; }
        public object ValidUntil { get; set; }
    }


}
