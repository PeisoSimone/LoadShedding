using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding.Model
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class AreasGPS
    {
        public int count { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string region { get; set; }
    }

    public class AreasNearbyGPSRoot
    {
        public List<AreasGPS> areas { get; set; }
    }
}