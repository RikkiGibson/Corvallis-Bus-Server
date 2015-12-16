using CorvallisBusDNX.Models.Connexionz;
using CorvallisBusDNX.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CorvallisBusDNX.WebClients
{
    /// <summary>
    /// Exposes methods for getting transit data from Connexionz.
    /// </summary>
    public static class ConnexionzClient
    {
        private const string BASE_URL = "http://www.corvallistransit.com/rtt/public/utility/file.aspx?contenttype=SQLXML";

        private static readonly HttpClient _client = new HttpClient();

        public static readonly AsyncLazy<IEnumerable<ConnexionzPlatform>> Platforms = new AsyncLazy<IEnumerable<ConnexionzPlatform>>(() => DownloadPlatforms());
        public static readonly AsyncLazy<IEnumerable<ConnexionzRoute>> Routes = new AsyncLazy<IEnumerable<ConnexionzRoute>>(() => DownloadRoutes());

        // Yes this is IDisposable, but it makes sense to have this object "live"
        // for the entire duration of the service, hence make it just a static object.
        //private static Lazy<HttpClient> _httpClient = new Lazy<HttpClient>();

        /// <summary>
        /// Gets and deserializes XML from the specified Connexionz/CTS endpoints.
        /// </summary>
        private static async Task<T> GetEntityAsync<T>(string url) where T : class
        {
            string xml = await _client.GetStringAsync(url);

            var reader = new StringReader(xml);

            return new XmlSerializer(typeof(T)).Deserialize(reader) as T;
        }

        /// <summary>
        /// Downloads static Connexionz Platforms (Stops) info.
        /// </summary>
        private static async Task<IEnumerable<ConnexionzPlatform>> DownloadPlatforms()
        {
            string xml = await _client.GetStringAsync(BASE_URL + "&Name=Platform.rxml");

            var document = XDocument.Parse(xml);

            return document.Element("Platforms")
                    .Elements("Platform")
                    .Select(e => new ConnexionzPlatform(e));
        }

        /// <summary>
        /// Downloads static Connexionz Route (e.g. Route 1, Route 8, etc) info.
        /// </summary>
        private static async Task<IEnumerable<ConnexionzRoute>> DownloadRoutes()
        {
            RoutePattern routePattern = await GetEntityAsync<RoutePattern>(BASE_URL + "&Name=RoutePattern.rxml");

            var routePatternProject = routePattern.Items.Skip(1).FirstOrDefault() as RoutePatternProject;

            return routePatternProject.Route.Select(r => new ConnexionzRoute(r));
        }

        /// <summary>
        /// Gets the Connexionz-estimated time of arrival for a given stop.
        /// </summary>
        public static async Task<ConnexionzPlatformET> GetPlatformEta(int platformTag)
        {
            RoutePosition position = await GetEntityAsync<RoutePosition>(BASE_URL + "&Name=RoutePositionET.xml&PlatformTag=" + platformTag.ToString());

            var positionPlatform = position.Items.FirstOrDefault(p => p is RoutePositionPlatform) as RoutePositionPlatform;

            return positionPlatform != null ?
                new ConnexionzPlatformET(positionPlatform) :
                null;
        }
    }
}