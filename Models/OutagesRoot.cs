using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding.Model
{
    public class OutagesRoot
    {
        public string area_name { get; set; }
        public int stage { get; set; }
        public DateTime start { get; set; }
        public DateTime finsh { get; set; }
        public string source { get; set; }
    }
}
