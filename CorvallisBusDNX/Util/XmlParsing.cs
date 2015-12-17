using CorvallisBusDNX.Models.Connexionz;
using System;
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

        public static IEnumerable<ConnexionzPlatform> ParseConnextionzPlatforms(Stream xmlStream)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This converts the XML nodes corresponding to a Corvallis Bus Route Destination into Platform objects.
        /// </summary>
        private static IEnumerable<RouteDestination> ParseDestinations(IEnumerable<XElement> destinationsXml) =>
            destinationsXml.Select(destinationXml =>
            {
                string fullName = destinationXml.FirstAttribute.Value;

                // Only one pattern exists per platform.
                var patternXml = destinationXml.Elements().SingleOrDefault();

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
                string name = platformXml
                              .Attributes()
                              .SingleOrDefault(a => a.Name.LocalName == "Name")?.Value ?? "";
                string scheduleAdherenceTimePointText = platformXml
                                                        .Attributes()
                                                        .SingleOrDefault(a => a.Name.LocalName == "ScheduleAdheranceTimepoint")?.Value ?? "";
                string platformNo = platformXml
                                    .Attributes()
                                    .SingleOrDefault(a => a.Name.LocalName == "PlatformNo")?.Value ?? "";
                string platformTag = platformXml
                                     .Attributes()
                                     .SingleOrDefault(a => a.Name.LocalName == "PlatformTag")?.Value ?? "";

                return new RoutePlatform(name, scheduleAdherenceTimePointText, platformNo, platformTag);
            });
    }
}
