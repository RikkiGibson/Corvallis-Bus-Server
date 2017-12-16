using System.Text.RegularExpressions;

namespace CorvallisBus.Core.Models.GoogleTransit
{
    /// <summary>
    /// Representation of a CTS Route from Google Transit data.
    /// </summary>
    public class GoogleRoute
    {
        public GoogleRoute(string[] csv)
        {
            Name = csv[0].Replace("\"", string.Empty); ;
            Color = csv[csv.Length - 2].Replace("\"", string.Empty);
            Url = csv[csv.Length - 3].Replace("\"", string.Empty);
        }

        public GoogleRoute(string Name, string Color, string Url)
        {
            this.Name = Name;
            this.Color = Color;
            this.Url = Url;
        }

        /// <summary>
        /// A name from the Google Transit CSV e.g. "BB_N".
        /// Must be converted to Connexionz format before merging.
        /// </summary>
        public string Name { get; private set; }

        public string ConnexionzName => ToConnexionzName(Name);

        public static string ToConnexionzName(string googleRouteNo) =>
            Regex.Replace(googleRouteNo.Replace("BB_", "NO"), "^R", "");

        /// <summary>
        /// The color of the route as a hex string, e.g. "35EFA0".
        /// </summary>
        public string Color { get; private set; }

        /// <summary>
        /// The URL at the Corvallis website where more information about this route can be found.
        /// </summary>
        public string Url { get; private set; }
    }
}