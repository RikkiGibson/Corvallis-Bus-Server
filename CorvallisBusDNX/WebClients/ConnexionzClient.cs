using CorvallisBusDNX.Models.Connexionz;
using CorvallisBusDNX.Util;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CorvallisBusDNX.WebClients
{
    /// <summary>
    /// Exposes methods for getting transit data from Connexionz.
    /// </summary>
    public static class ConnexionzClient
    {
        private const string BASE_URL = "http://www.corvallistransit.com/rtt/public/utility/file.aspx?contenttype=SQLXML";

        private static readonly HttpClient _client = new HttpClient();

        /// <summary>
        /// Lazy-loaded platform data.
        /// </summary>
        public static readonly AsyncLazy<IEnumerable<ConnexionzPlatform>> Platforms = new AsyncLazy<IEnumerable<ConnexionzPlatform>>(() => GetPlatformsAsync());

        /// <summary>
        /// lazy-loaded Route data.
        /// </summary>
        public static readonly AsyncLazy<IEnumerable<ConnexionzRoute>> Routes = new AsyncLazy<IEnumerable<ConnexionzRoute>>(() => GetRoutesAsync());

        /// <summary>
        /// Downloads static Connexionz Platforms (Stops) info.
        /// </summary>
        private static async Task<IEnumerable<ConnexionzPlatform>> GetPlatformsAsync()
        {
            var xmlStream = await _client.GetStreamAsync(BASE_URL + "&Name=Platform.rxml");

            var document = XDocument.Load(xmlStream);

            return document.Element("Platforms")
                           .Elements("Platform")
                           .Select(e => new ConnexionzPlatform(e));
        }

        /// <summary>
        /// Downloads static Connexionz Route (e.g. Route 1, Route 8, etc) info.
        /// </summary>
        private static async Task<IEnumerable<ConnexionzRoute>> GetRoutesAsync()
        {
            var xmlStream = await _client.GetStreamAsync(BASE_URL + "&Name=RoutePattern.rxml");
            return ConnexionzXmlParser.ParseConnexionzRoutes(xmlStream);
        }

        /// <summary>
        /// Gets the Connexionz-estimated time of arrival for a given stop.
        /// </summary>
        public static async Task<ConnexionzPlatformET> GetPlatformEtaAsync(int platformTag)
        {
            var xmlStream = await _client.GetStreamAsync($"{BASE_URL}&Name=RoutePositionET.xml&PlatformTag={platformTag}");
            return ConnexionzXmlParser.ParseConnextionzPlatforms(xmlStream);
        }
    }
}