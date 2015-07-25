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
        public static List<BusRoute> Routes { get; private set; }

        public static Lazy<List<GoogleRoute>> GoogleRoutes = new Lazy<List<GoogleRoute>>(GoogleTransitImport.DoTask);

        /// <summary>
        /// Performs route and stop lookups and builds the static data used in the /static route.
        /// </summary>
        public static async Task<object> GetStaticData()
        {
            var routes = await CacheManager.GetStaticRoutesAsync();
            var stops = await CacheManager.GetStaticStopsAsync();

            return new
            {
                routes = routes.ToDictionary(r => r.RouteNo),
                stops = stops.ToDictionary(s => s.ID)
            };
        }

        /// <summary>
        /// Gets the ETA info for a set of stop IDS.  Performs the calls to get the info in parallel,
        /// aggregating the data into a dictionary.
        /// </summary>
        public static async Task<object> GetEtas(IEnumerable<string> stopIds)
        {
            Dictionary<string, string> toPlatformTag = await CacheManager.GetPlatformTagsAsync();

            //Func<string, Tuple<string, ConnexionzPlatformET>> getEtaIfTagExists =
            //    id => Tuple.Create(id, toPlatformTag.ContainsKey(id) ?
            //                           await CacheManager.GetEta(toPlatformTag[id]) :
            //                           null);

            Func<string, Task<Tuple<string, ConnexionzPlatformET>>> getEtaIfTagExistsAsync = async stopId =>
            {
                return Tuple.Create(stopId, toPlatformTag.ContainsKey(stopId)
                                            ? await CacheManager.GetEta(toPlatformTag[stopId])
                                            : null);
            };

            var etas = stopIds.AsParallel()
                              .Select(getEtaIfTagExistsAsync);

            return etas.ToDictionary(
                eta => eta.Result.Item1,
                eta => eta.Result.Item2?.RouteEstimatedArrivals?.ToDictionary(
                    route => route.RouteNo,
                    route => route.EstimatedArrivalTime) ?? new object());
        }
    }
}