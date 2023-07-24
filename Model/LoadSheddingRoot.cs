using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding.Model
{
    public class Schedule
    {
        public string area_name { get; set; }
        public int stage { get; set; }
        public DateTime start { get; set; }
        public DateTime finsh { get; set; }
        public string source { get; set; }
    }
    public class Outage
    {
        public string start_time { get; set; }
        public string finsh_time { get; set; }
        public int stage { get; set; }
        public string recurrence { get; set; }
        public int day1_of_recurrence { get; set; }
    }

    public class LoadSheddingRoot
    {
        public int id { get; set; }
        public List<Outage> outages { get; set; }
        public List<object> source { get; set; }
        public List<object> info { get; set; }
        public object last_updated { get; set; }
        public object valid_from { get; set; }
        public object valid_until { get; set; }
    }
}
