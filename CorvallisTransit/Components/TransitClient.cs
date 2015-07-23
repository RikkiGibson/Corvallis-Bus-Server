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
using CorvallisTransit.Models.Connexionz;
using System.Xml.Linq;
using CorvallisTransit.Models.GoogleTransit;

namespace CorvallisTransit.Components
{
    /// <summary>
    /// Static client that downloads and returns deserialized route and platform details.
    /// </summary>
    public static class TransitClient
    {
        private const int PLATFORM_WARNING_CUTOFF = 4;

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

        public static void InitializeAndUpdate()
        {
            // TODO: only get the route pattern if necessary
            //var tempRoutes = PatternToBusRoutes(ConnexionzClient.GetRoutePattern());

            //CheckForWarnings(tempRoutes);
            //Routes = tempRoutes;
            //foreach (var route in Routes)
            //{
            //    if (UpdateRoute != null)
            //    {
            //        UpdateRoute(route);
            //    }

            //}
        }

        internal static void UpdateClients()
        {
            if (UpdateRoute == null || Routes == null)
            {
                return;
            }

            foreach (var route in Routes)
            {
                UpdateRoute(route);
            }
        }

        /// <summary>
        /// Lazy-loaded static route and stop data for the /static route.
        /// </summary>
        public static Lazy<object> StaticData = new Lazy<object>(GetStaticData);

        /// <summary>
        /// Lazy-loaded set of google data parsed from downloaded and zipped csvs.
        /// 
        /// TODO - make this come from a blob and then a cache, NOT constantly re-downloaded.
        /// </summary>
        public static Lazy<List<GoogleRoute>> GoogleRoutes = new Lazy<List<GoogleRoute>>(GoogleTransitImport.DoTask);

        /// <summary>
        /// Performs route and stop lookups and builds the static data used in the /static route.
        /// </summary>
        private static object GetStaticData()
        {
            var routesTask = Task.Run(() => StorageManager.GetRoutesAsync());
            var stopsTask = Task.Run(() => StorageManager.GetStopsAsync());

            routesTask.Wait();
            stopsTask.Wait();

            return new
            {
                routes = routesTask.Result.ToDictionary(r => r.RouteNo),
                stops = stopsTask.Result.ToDictionary(s => s.ID)
            };
        }
    }
}