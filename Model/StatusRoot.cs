using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding.Model
{
    public class Capetown
    {
        public string name { get; set; }
        public List<NextStage> next_stages { get; set; }
        public string stage { get; set; }
        public DateTime stage_updated { get; set; }
    }

    public class Eskom
    {
        public string name { get; set; }
        public List<NextStage> next_stages { get; set; }
        public string stage { get; set; }
        public DateTime stage_updated { get; set; }
    }

    public class NextStage
    {
        public string stage { get; set; }
        public DateTime stage_start_timestamp { get; set; }
    }

    public class StatusRoot
    {
        public Status status { get; set; }
    }

    public class Status
    {
        public Capetown capetown { get; set; }
        public Eskom eskom { get; set; }
    }
}
