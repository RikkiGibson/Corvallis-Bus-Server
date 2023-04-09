using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using System.Xml.Linq;
using CorvallisBus.Core.Models.Connexionz;
using CorvallisBus.Core.Models;
using System.Threading.Tasks;
using System.Net.Http;

namespace CorvallisBus.Core.WebClients
{
    /// <summary>
    /// Exposes methods for getting transit data from Connexionz.
    /// </summary>
    public static class ConnexionzClient
    {
        private const string BASE_URL = "http://www.corvallistransit.com/rtt/public/utility/file.aspx?contenttype=SQLXML";

        /// <summary>
        /// Gets and deserializes XML from the specified Connexionz/CTS endpoints.
        /// </summary>
        private static T GetEntity<T>(string url) where T : class
        {
            return GetEntityAsync<T>(url).Result;
        }

        /// <summary>
        /// Gets and deserializes XML from the specified Connexionz/CTS endpoints.
        /// </summary>
        private static async Task<T> GetEntityAsync<T>(string url) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));
            var client = new HttpClient();
            var content = await client.GetStringAsync(url);
            return (T?)serializer.Deserialize(new StringReader(content)) ?? throw new InvalidOperationException();
        }

        /// <summary>
        /// Downloads static Connexionz Platforms (Stops) info.
        /// </summary>
        public static List<ConnexionzPlatform> LoadPlatforms()
        {
            var client = new HttpClient();
            var content = client.GetStringAsync(BASE_URL + "&Name=Platform.rxml").Result;
            var document = XDocument.Parse(content);
            return document.Element("Platforms")!
                .Elements("Platform")
                .Where(e => e.Attribute("PlatformNo") is object)
                .Select(ConnexionzPlatform.Create)
                .ToList();
        }

        /// <summary>
        /// Downloads static Connexionz Route (e.g. Route 1, Route 8, etc) info.
        /// </summary>
        public static List<ConnexionzRoute> LoadRoutes()
        {
            var routePattern = GetEntity<RoutePattern>(BASE_URL + "&Name=RoutePattern.rxml");

            var routePatternProject = (RoutePatternProject)routePattern.Items.Skip(1).First();

            return routePatternProject.Route.Select(r => new ConnexionzRoute(r)).ToList();
        }

        /// <summary>
        /// Gets the Connexionz-estimated time of arrival for a given stop.
        /// </summary>
        public static async Task<ConnexionzPlatformET?> GetPlatformEta(int platformTag)
        {
            RoutePositionET position = await GetEntityAsync<RoutePositionET>(BASE_URL + "&Name=RoutePositionET.xml&PlatformTag=" + platformTag.ToString());

            var positionPlatform = position.Items.OfType<RoutePositionPlatform>().FirstOrDefault();

            return positionPlatform != null ?
                new ConnexionzPlatformET(positionPlatform) :
                null;
        }
    }
}