using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding.Model
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    public class OutagesRoot
    {
        public string AreaName { get; set; }
        public int Stage { get; set; }
        public DateTime Start { get; set; }
        public DateTime Finsh { get; set; }
        public string Source { get; set; }
    }
}
