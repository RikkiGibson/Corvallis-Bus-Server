using CorvallisTransit.Models;
using CorvallisTransit.Models.Connexionz;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CorvallisTransit.Components
{
    public static class CacheManager
    {
        private const string ROUTES_KEY = "routes";
        private const string STOPS_KEY = "stops";
        private const string PLATFORM_TAGS_KEY = "platformTags";

        private static Lazy<ConnectionMultiplexer> lazyConn = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["RedisCacheConnectionString"]);
        });

        /// <summary>
        /// Connection to the Redis cache.
        /// </summary>
        public static ConnectionMultiplexer Connection => lazyConn.Value;

        public static void WipeStaticData()
        {
            var cache = Connection.GetDatabase();

            cache.StringSet(ROUTES_KEY, "");
            cache.StringSet(STOPS_KEY, "");
            cache.StringSet(PLATFORM_TAGS_KEY, "");
        }

        /// <summary>
        /// Gets static routes from the cache.  Grabs the blob data if the cache is empty.
        /// </summary>
        public static async Task<List<BusRoute>> GetStaticRoutesAsync()
        {
            var cache = Connection.GetDatabase();

            var json = await cache.StringGetAsync(ROUTES_KEY);
            if (string.IsNullOrEmpty(json))
            {
                json = await StorageManager.GetStaticRouteDataAsync();
                cache.StringSet(ROUTES_KEY, json);
            }

            return JsonConvert.DeserializeObject<List<BusRoute>>(json);
        }

        /// <summary>
        /// Gets static stops from the cache.  Grabs the blob data if the cache is empty.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<BusStop>> GetStaticStopsAsync()
        {
            var cache = Connection.GetDatabase();

            var json = await cache.StringGetAsync(STOPS_KEY);
            if (string.IsNullOrEmpty(json))
            {
                json = await StorageManager.GetStaticStopDataAsync();
                cache.StringSet(STOPS_KEY, json);
            }

            return JsonConvert.DeserializeObject<List<BusStop>>(json);
        }

        /// <summary>
        /// Gets the platform tag dictionary from the cache.  Grabs the blob data if the cache is empty.
        /// </summary>
        public static async Task<Dictionary<string, string>> GetPlatformTagsAsync()
        {
            var cache = Connection.GetDatabase();

            var json = await cache.StringGetAsync(PLATFORM_TAGS_KEY);
            if (string.IsNullOrWhiteSpace(json))
            {
                json = await StorageManager.GetPlatformTagsAsync();
                cache.StringSet(PLATFORM_TAGS_KEY, json);
            }

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        /// <summary>
        /// Extracts a platform ETA from the cache.  Handles entry management.  Note that this is a synchronous call!
        /// </summary>
        public static ConnexionzPlatformET GetEta(string platformTag)
        {
            var cache = Connection.GetDatabase();

            ConnexionzPlatformET arrival;

            var json = cache.StringGet(platformTag);
            if (string.IsNullOrWhiteSpace(json))
            {
                return UpdateCacheAndReturnETA(platformTag, cache);
            }
            else
            {
                arrival = JsonConvert.DeserializeObject<ConnexionzPlatformET>(json);
                
                // It's save to assume that a bus will be arriving to a different stop
                // after one minute, so we update when that is the case.
                if ((DateTime.Now - arrival.LastUpdated).Minutes >= 1)
                {
                    return UpdateCacheAndReturnETA(platformTag, cache);
                }
                else
                {
                    return arrival;
                }
            }
        }

        /// <summary>
        /// Computes the new ETA info for the given Connexionz Stop, and puts that in the cache.
        /// </summary>
        private static ConnexionzPlatformET UpdateCacheAndReturnETA(string platformTag, IDatabase cache)
        {
            var newArrival = ConnexionzClient.GetPlatformEta(platformTag);
            var json = JsonConvert.SerializeObject(newArrival);

            cache.StringSet(platformTag, json);

            return newArrival;
        }
    }
}