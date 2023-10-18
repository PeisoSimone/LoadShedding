using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding.Model
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class AreaSearch
    {
        public string id { get; set; }
        public string name { get; set; }
        public string region { get; set; }
    }

    public class AreaSearchRoot
    {
        public List<AreaSearch> areas { get; set; }
    }
}
