using CorvallisTransit.Models;
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

        /// <summary>
        /// Gets static routes from the cache.  Grabs the blob data if the cache is empty.
        /// </summary>
        public static async Task<List<BusRoute>> GetStaticRoutesAsync()
        {
            var database = Connection.GetDatabase();

            var json = await database.StringGetAsync(ROUTES_KEY);
            if (string.IsNullOrEmpty(json))
            {
                json = await StorageManager.GetStaticRouteDataAsync();
                database.StringSet(ROUTES_KEY, json);
            }

            return JsonConvert.DeserializeObject<List<BusRoute>>(json);
        }

        /// <summary>
        /// Gets static stops from the cache.  Grabs the blob data if the cache is empty.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<BusStop>> GetStaticStopsAsync()
        {
            var database = Connection.GetDatabase();

            var json = await database.StringGetAsync(STOPS_KEY);
            if (string.IsNullOrEmpty(json))
            {
                json = await StorageManager.GetStaticStopDataAsync();
                database.StringSet(STOPS_KEY, json);
            }

            return JsonConvert.DeserializeObject<List<BusStop>>(json);
        }

        /// <summary>
        /// Gets the platform tag dictionary from the cache.  Grabs the blob data if the cache is empty.
        /// </summary>
        public static async Task<Dictionary<string, string>> GetPlatformTagsAsync()
        {
            var database = Connection.GetDatabase();

            var json = await database.StringGetAsync(PLATFORM_TAGS_KEY);
            if (string.IsNullOrWhiteSpace(json))
            {
                json = await StorageManager.GetPlatformTagsAsync();
                database.StringSet(PLATFORM_TAGS_KEY, json);
            }

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
    }
}