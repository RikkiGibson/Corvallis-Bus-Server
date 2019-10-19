using CsvHelper.Configuration.Attributes;

namespace CorvallisBus.Core.Models.GoogleTransit
{
    /// <summary>
    /// Representation of a CTS Route from Google Transit data.
    /// </summary>
    public class RouteEntry
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// A name from the Google Transit CSV
        /// </summary>
        [Name("route_id")]
        public string Name { get; set; }

        /// <summary>
        /// The color of the route as a hex string, e.g. "35EFA0".
        /// </summary>
        [Name("route_color")]
        public string Color { get; set; }

        [Name("route_url")]
        public string Url { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}