using API.Models.Connexionz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace API.Models
{
    /// <summary>
    /// A bus stop in the Corvallis Transit System. This is analogous to the Platform entity in Connexionz.
    /// </summary>
    public class BusStop
    {
        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public BusStop() { }

        /// <summary>
        /// This makes it look like there is little purpose to this type,
        /// but it's good to get the semantics clear. ConnexionzPlatform is
        /// something you've just pulled from Connexionz,
        /// while BusStop is fully processed and ready to serve to clients.
        /// </summary>
        public BusStop(ConnexionzPlatform platform, List<string> routeNames)
        {
            ID = platform.PlatformNo;
            Name = platform.Name;
            Lat = platform.Lat;
            Long = platform.Long;
            RouteNames = routeNames;
        }

        /// <summary>
        /// This stop tag is used to get ETAs for the stop from Connexionz.
        /// </summary>
        [JsonProperty("id")]
        public int ID { get; set; }

        /// <summary>
        /// The name of the stop, for example: "NW Monroe Ave & NW 7th St".
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The latitude value for the stop (between -90 and 90 degrees).
        /// </summary>
        [JsonProperty("lat")]
        public double Lat { get; set; }

        /// <summary>
        /// The longitude value for the stop (between -180 and 180 degrees).
        /// </summary>
        [JsonProperty("lng")]
        public double Long { get; set; }

        /// <summary>
        /// List of route names which arrive at this stop.
        /// </summary>
        [JsonProperty("routeNames")]
        public List<string> RouteNames { get; set; }
    }
}
