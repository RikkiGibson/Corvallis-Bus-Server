using CorvallisBusDNX.Models.Connexionz;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CorvallisBusDNX.Util
{
    /// <summary>
    /// Contains parsing logic to deal with the monstrosities that are CTS XML feeds.
    /// </summary>
    public static class ConnexionzXmlParser
    {
        /// <summary>
        /// Given an stream of Corvallis Bus XML Data, this generates the Connexionz Route data.
        /// </summary>
        public static IEnumerable<ConnexionzRoute> ParseConnexionzRoutes(Stream xmlStream)
        {
            var xdoc = XDocument.Load(xmlStream);

            // This gets us into the "Project" tag where everything we want exists
            var routeXmlElements = xdoc.Root.Elements().Last().Elements();

            return routeXmlElements.Select(routeXmlElement =>
            {
                string routeNo = routeXmlElement.FirstAttribute.Value;

                var destinationsXml = routeXmlElement.Elements();
                var destinations = ParseDestinations(destinationsXml);

                return new ConnexionzRoute(new Route(routeNo, destinations));
            });
        }

        public static ConnexionzPlatformET ParseConnextionzPlatforms(Stream xmlStream)
        {
            var xdoc = XDocument.Load(xmlStream);

            // This gets us to the set of platform XML nodes.
            var platformsXml = xdoc.Root.Elements().Skip(1);

            return ParsePlatformET(platformsXml);
        }

        private static ConnexionzPlatformET ParsePlatformET(IEnumerable<XElement> platformsXml)
        {
            // There's only ever one ... the XML just happens to indicate there's a list.
            var platformXml = platformsXml.First();

            string platformTag = platformXml.Attribute("PlatformTag")?.Value ?? "";

            var routesXml = platformXml.Elements();

            var routeET = ParseRouteET(routesXml).ToList();

            return new ConnexionzPlatformET(platformTag, routeET);
        }

        private static IEnumerable<ConnexionzRouteET> ParseRouteET(IEnumerable<XElement> routesXml) =>
            routesXml.Select(routeXml =>
            {
                string routeNo = routeXml.Attribute("RouteNo")?.Value ?? "";

                var etas = routeXml.Elements().Elements()          // Need to get the children's children
                           .Where(e => e.Name.LocalName == "Trip") // The namespace here screws up Element("Trip")
                           .Select(e => e.Attribute("ETA").Value)
                           .Select(etaString => int.Parse(etaString)).ToList();

                return new ConnexionzRouteET(routeNo, etas);
            });

        /// <summary>
        /// This converts the XML nodes corresponding to a Corvallis Bus Route Destination into Platform objects.
        /// </summary>
        private static IEnumerable<RouteDestination> ParseDestinations(IEnumerable<XElement> destinationsXml) =>
            destinationsXml.Select(destinationXml =>
            {
                string fullName = destinationXml.FirstAttribute.Value;

                // Only one pattern exists per platform.
                var patternXml = destinationXml.Elements().SingleOrDefault();

                // The "mif" is the big string that's really a space-separated CSV
                // of the Lat/Longs the route travels.
                //
                // No idea why it's called "mif", but whatever.
                string mif = patternXml.Elements().Skip(1).First().Value;

                // This gets us to the tag where we can get actual platform data
                var platformsXml = patternXml.Elements().Skip(2);

                var platforms = ParsePlatformsForRoutes(platformsXml);

                var mifAndPlatform = new RoutePattern(mif, platforms);

                return new RouteDestination(fullName, mifAndPlatform);
            });

        /// <summary>
        /// This converts the XML nodes corresponding to Corvallis Bus Platforms into Platform objects.
        /// </summary>
        private static IEnumerable<RoutePlatform> ParsePlatformsForRoutes(IEnumerable<XElement> platformsXml) =>
            platformsXml.Select(platformXml =>
            {
                string name = platformXml.Attribute("Name")?.Value ?? "";
                string scheduleAdherenceTimePointText = platformXml.Attribute("ScheduleAdheranceTimepoint")?.Value ?? "";
                string platformNo = platformXml.Attribute("PlatformNo")?.Value ?? "";
                string platformTag = platformXml.Attribute("PlatformTag")?.Value ?? "";

                return new RoutePlatform(name, scheduleAdherenceTimePointText, platformNo, platformTag);
            });
    }
}
