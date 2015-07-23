using CorvallisTransit.Models;
using CorvallisTransit.Models.Connexionz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Serialization;

namespace CorvallisTransit.Components
{
    /// <summary>
    /// Exposes methods for getting transit data from Connexionz.
    /// </summary>
    public static class ConnexionzClient
    {
        private const string BASE_URL = "http://www.corvallistransit.com/rtt/public/utility/file.aspx?contenttype=SQLXML";

        // TODO: handle expiration
        public static Lazy<IEnumerable<ConnexionzPlatform>> Platforms = new Lazy<IEnumerable<ConnexionzPlatform>>(DownloadPlatforms);
        public static Lazy<IEnumerable<ConnexionzRoute>> Routes = new Lazy<IEnumerable<ConnexionzRoute>>(DownloadRoutes);

        /// <summary>
        /// Gets and deserializes XML from the specified Connexionz/CTS endpoints.
        /// </summary>
        private static T GetEntity<T>(string url) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var client = new WebClient())
            {
                string s = client.DownloadString(url);

                var reader = new StringReader(s);

                return serializer.Deserialize(reader) as T;
            }
        }

        /// <summary>
        /// Downloads static Connexionz Platforms (Stops) info.
        /// </summary>
        private static IEnumerable<ConnexionzPlatform> DownloadPlatforms()
        {
            Platforms platforms = GetEntity<Platforms>(BASE_URL + "&Name=Platform.rxml");

            return platforms.Items
                   .Where(i => i is PlatformsPlatform)
                   .Cast<PlatformsPlatform>()
                   .Select(pp => new ConnexionzPlatform(pp));
        }

        /// <summary>
        /// Downloads static Connexionz Route (e.g. Route 1, Route 8, etc) info.
        /// </summary>
        private static IEnumerable<ConnexionzRoute> DownloadRoutes()
        {
            RoutePattern routePattern = GetEntity<RoutePattern>(BASE_URL + "&Name=RoutePattern.rxml");

            var routePatternProject = routePattern.Items.Skip(1).FirstOrDefault() as RoutePatternProject;

            return routePatternProject.Route.Select(r => new ConnexionzRoute(r));
        }

        /// <summary>
        /// Gets the Connexionz-estimated time of arrival for a given stop.
        /// </summary>
        public static ConnexionzPlatformET GetPlatformEta(string platformTag)
        {
            RoutePosition position = GetEntity<RoutePosition>(BASE_URL + "&Name=RoutePositionET.xml&PlatformTag=" + platformTag);

            var positionPlatform = position.Items.FirstOrDefault(p => p is RoutePositionPlatform) as RoutePositionPlatform;

            return positionPlatform != null ?
                new ConnexionzPlatformET(positionPlatform) :
                null;
        }
    }
}