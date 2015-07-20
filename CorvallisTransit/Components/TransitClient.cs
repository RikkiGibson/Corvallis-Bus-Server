using CorvallisTransit.Models;
using CorvallisTransit.Components;
using GoogleMaps.LocationServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CorvallisTransit.Components
{
    /// <summary>
    /// Static client that downloads and returns deserialized route and platform details.
    /// </summary>
    public static class TransitClient
    {
        private const int PLATFORM_WARNING_CUTOFF = 4;
        private static DateTime expires;

        public static object StopsLocker { get; set; }
        private static object locker = new object();
        
        public static bool IsRunning { get; set; }

        static TransitClient()
        {
            IsRunning = false;
            StopsLocker = new object();
        }


        public delegate void OnRouteUpdate(BusRoute route);
        public static event OnRouteUpdate UpdateRoute;

        public static List<BusRoute> Routes { get; private set; }

        /// <summary>
        /// Updates the route information.
        /// </summary>
        /// <param name="xmlPattern">The XML pattern.</param>
        /// <param name="xmlRoutes">The XML routes.</param>
        /// <param name="routeModels">The route models.</param>
        /// <param name="threads">The threads.</param>
        private static void UpdateRouteInformation(RoutePattern xmlPattern, IEnumerable<RoutePatternProjectRoute> xmlRoutes, List<BusRoute> routeModels, List<Task> threads)
        {
            // update the expiration date for the route information
            expires = DateTime.Parse(xmlPattern.Content.Expires);
            foreach (var route in xmlRoutes.GroupBy(r => r.RouteNo))
            {
                // routestops are the association object between their individual stops and multiple routes
                List<BusRouteStop> stopAssociations = new List<BusRouteStop>();


                BusRoute model = new BusRoute()
                {
                    RouteNo = route.Key,
                    RouteTimeWarning = false,
                    Stops = stopAssociations
                };


                var pattern = route.SelectMany(r => r.Destination)
                                   .Select(d => d.Pattern.Where(p => p.Name.Equals(route.Key, StringComparison.CurrentCultureIgnoreCase)))
                                   .SelectMany(pt => pt.SelectMany(pl => pl.Platform.Select(p => p)));

                int stopCount = 0;

                foreach (var platform in pattern)
                {
                    // we need to capture the current platform because in this foreach loop it will change the object in the thread
                    // when we change the pointer
                    var stopClosure = platform;


                    // build this stop, they call them 'platforms
                    BusStop stopModel = new BusStop()
                    {
                        Name = stopClosure.Name,
                        Id = stopClosure.PlatformNo,
                        StopTag = stopClosure.PlatformTag

                    };

                    // this is the association their system uses, ugh
                    BusRouteStop routeAssociation = new BusRouteStop()
                    {
                        RouteModel = model,
                        StopModel = stopModel,
                        StopPosition = stopCount++
                    };

                    threads.Add(Task.Run(() =>
                    {
                        GetPlatformGps(stopAssociations, stopClosure, stopModel, routeAssociation);
                    }));

                }

                // we'll give up after 20 seconds of trying to get all the gps details
                Task.WaitAll(threads.ToArray(), TransitConstants.ThreadTimeout);

                // order our stops using our custom comparer, logic inside!
                model.Stops = model.Stops.Distinct(TransitComparers.TransitComparer).ToList();

                routeModels.Add(model);
            }
        }

        /// <summary>
        /// Gets the platform GPS.
        /// </summary>
        /// <param name="stopAssociations">The stop associations.</param>
        /// <param name="stopClosure">The stop closure.</param>
        /// <param name="stopModel">The stop model.</param>
        /// <param name="routeAssociation">The route association.</param>
        private static void GetPlatformGps(List<BusRouteStop> stopAssociations, Platform stopClosure, BusStop stopModel, BusRouteStop routeAssociation)
        {
            //TODO: all this information should be obtained in the initial platforms get.
            // get the platform gps position
            var platformDetails = ConnexionzClient.GetPlatform(stopClosure.PlatformTag);

            if (platformDetails.Position != null)
            {
                stopModel.Lat = platformDetails.Position.Lat;
                stopModel.Long = platformDetails.Position.Long;
            }
            lock (locker)
            {
                stopAssociations.Add(routeAssociation);
            }
        }

        /// <summary>
        /// Updates the stop eta.
        /// </summary>
        /// <param name="stopClosure">The stop closure.</param>
        private static void UpdateStopEta(BusRouteStop stopClosure)
        {
            var stopEtaDetails = ConnexionzClient.GetPlatformEta(stopClosure.StopModel.StopTag);
            var platform = stopEtaDetails.GetPlatform(stopClosure);

            // if we don't have the platform, set Eta to 0, same if we don't have the detail
            if (platform != null)
            {
                var detail = platform.Route.FirstOrDefault(rt => rt.RouteNo == stopClosure.RouteModel.RouteNo);
                if (detail != null)
                {
                    stopClosure.Eta = detail.Destination.Trip.ETA;
                }
                else
                {
                    stopClosure.Eta = 0;
                }
            }
            else
            {
                stopClosure.Eta = 0;
            }
        }

        /// <summary>
        /// Gets the routes from a route pattern obtained from xml.
        /// </summary>
        /// <param name="xmlPattern">The XML pattern.</param>
        /// <returns>All of the bus routes, with their associated stops.</returns>
        private static List<BusRoute> PatternToBusRoutes(RoutePattern xmlPattern)
        {
            IEnumerable<RoutePatternProjectRoute> xmlRoutes = xmlPattern.Project.Route;
            List<BusRoute> routeModels = new List<BusRoute>();
            List<Task> threads = new List<Task>();

            // first, do we need to get new route details?
            // only download the route info if either we don't have any
            // or we have passed their specified 'expiration' date
            if (Routes == null || !Routes.Any() || DateTime.Now >= expires)
                UpdateRouteInformation(xmlPattern, xmlRoutes, routeModels, threads);
            else
            {
                // otherwise, lets get a temporary list from our current routes
                routeModels = Routes;
            }

            // update all the etas for all the stops for all the routes
            // TODO: parallelize this?
            foreach (var route in routeModels)
            {
                foreach (var stop in route.Stops.OrderBy(s => s, TransitComparers.TransitComparer))
                {
                    // since we're threading, more closure
                    var stopClosure = stop;
                    threads.Add(Task.Run(() =>
                    {
                        UpdateStopEta(stopClosure);
                    }));
                }
                route.UpdatedSuccessfully = Task.WaitAll(threads.ToArray(), 40000);


                // we have a lot of stops, use the comparer to organize them by eta/position
                route.Stops = route.Stops.Distinct(TransitComparers.TransitComparer)
                                         .OrderBy(s => s, TransitComparers.TransitComparer)
                                         .ToList();
            }


            Routes = routeModels.OrderBy(rm => rm.RouteNo).ToList();

            return Routes;
        }
        public static void InitializeAndUpdate()
        {
            // TODO: only get the route pattern if necessary
            var tempRoutes = PatternToBusRoutes(ConnexionzClient.GetRoutePattern());

            CheckForWarnings(tempRoutes);
            Routes = tempRoutes;
            foreach (var route in Routes)
            {
                if (UpdateRoute != null)
                {
                    UpdateRoute(route);
                }
                
            }
        }

        /// <summary>
        /// Checks for warnings.
        /// </summary>
        /// <param name="tempRoutes">The temporary routes used before we assign them to our routes variable.</param>
        private static void CheckForWarnings(List<BusRoute> tempRoutes)
        {
            foreach (var route in tempRoutes)
            {

                var platforms = route.Stops;
                var laterPlatforms = platforms.OrderBy(pt => pt.StopPosition).Skip(1).ToList();
                var laterPlatformsWithEta = laterPlatforms.SkipWhile(p => p.Eta == 0);
                if (laterPlatforms.IndexOf(laterPlatformsWithEta.FirstOrDefault()) > 0 && laterPlatformsWithEta.Where(p => p.Eta > 0).All(p => p.Eta > PLATFORM_WARNING_CUTOFF))
                {
                    route.RouteTimeWarning = true;
                }
                else
                {
                    route.RouteTimeWarning = false;
                }

            }
        }


        internal static void UpdateClients()
        {
            if (UpdateRoute == null)
            {
                return;
            }

            foreach (var route in Routes)
            {
                UpdateRoute(route);
            }
        }
    }
}

/// <summary>
/// Exposes methods for getting transit data from Connexionz.
/// </summary>
internal static class ConnexionzClient
{
    private static T GetEntity<T>(string url) where T : class
    {
        var serializer = new XmlSerializer(typeof(T));

        using (var client = new WebClient())
        {
            string s = client.DownloadString(url);

            TextReader reader = new StringReader(s);
            return serializer.Deserialize(reader) as T;
        }
    }

    // TODO: All the platforms can be downloaded in one get, but this only gets platforms one at a time. Too slow.
    internal static PlatformsPlatform GetPlatform(string platformTag)
    {
        if (platformTag == null)
        {
            return null;
        }
        string url = string.Format("http://www.corvallistransit.com/rtt/public/utility/file.aspx?contenttype=SQLXML&Name=Platform.rxml&PlatformTag={0}", platformTag);
        Platforms platformSet = GetEntity<Platforms>(url);

        return platformSet.Stops.FirstOrDefault();
    }

    internal static RoutePattern GetRoutePattern()
    {
        return GetEntity<RoutePattern>("http://www.corvallistransit.com/rtt/public/utility/file.aspx?contenttype=SQLXML&Name=RoutePattern.rxml");
    }

    internal static RoutePosition GetPlatformEta(string platformTag)
    {
        if (platformTag == null)
        {
            return null;
        }
        string url = string.Format("http://www.corvallistransit.com/rtt/public/utility/file.aspx?contenttype=SQLXML&Name=RoutePositionET.xml&PlatformTag={0}", platformTag);

        return GetEntity<RoutePosition>(url);
    }
}