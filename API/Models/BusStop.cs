using API.Models.Connexionz;

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

        public BusStop(ConnexionzPlatform platform)
        {
            ID = int.Parse(platform.PlatformNo);
            Name = platform.Name;
            Lat = platform.Lat.HasValue ? platform.Lat.Value : 0.0;
            Long = platform.Long.HasValue ? platform.Long.Value : 0.0;
        }

        /// <summary>
        /// This stop tag is used to get ETAs for the stop from Connexionz.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The name of the stop, for example: "NW Monroe Ave & NW 7th St".
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The latitude value for the stop (between -90 and 90 degrees).
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// The longitude value for the stop (between -180 and 180 degrees).
        /// </summary>
        public double Long { get; set; }
    }
}
