using API.Models;
using API.Models.Connexionz;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Components
{
    public static class CacheManager
    {
        // todo: centralize this
        private const string ROUTES_KEY = "routes";
        private const string STOPS_KEY = "stops";
        private const string PLATFORM_TAGS_KEY = "platformTags";
        private const string SCHEDULE_KEY = "schedule";

        // Nothing about Configuration is accurate right now, so this is why I'm hard-coding this crap.
        private const string REDIS_CACHE_CONN_STRING = "corvallisbus.redis.cache.windows.net,ssl=true,password=u7VehCkNYOxtrELm10+sGBtDsKzKFWi+t/OxHGeB4VY=";

        private static Lazy<ConnectionMultiplexer> lazyConn = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect(REDIS_CACHE_CONN_STRING);
        });

        /// <summary>
        /// Connection to the Redis cache.
        /// </summary>
        public static ConnectionMultiplexer Connection => lazyConn.Value;

        internal static void ClearCache()
        {
            var cache = Connection.GetDatabase();

            cache.StringSet(ROUTES_KEY, string.Empty);
            cache.StringSet(STOPS_KEY, string.Empty);
            cache.StringSet(PLATFORM_TAGS_KEY, string.Empty);
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

        public static async Task<Dictionary<string, IEnumerable<BusStopRouteSchedule>>> GetScheduleAsync()
        {
            var cache = Connection.GetDatabase();

            var json = await cache.StringGetAsync(SCHEDULE_KEY);
            if (string.IsNullOrWhiteSpace(json))
            {
                json = await StorageManager.GetScheduleAsync();
                cache.StringSet(SCHEDULE_KEY, json);
            }

            return JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<BusStopRouteSchedule>>>(json);
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