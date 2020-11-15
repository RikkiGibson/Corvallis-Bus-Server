using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Container for the set of static data which is expected to
    /// remain the same during a client application lifecycle.
    /// </summary>
    public record BusStaticData(
        [property: JsonProperty("routes")]
        Dictionary<string, BusRoute> Routes,

        [property: JsonProperty("stops")]
        Dictionary<int, BusStop> Stops);
}
