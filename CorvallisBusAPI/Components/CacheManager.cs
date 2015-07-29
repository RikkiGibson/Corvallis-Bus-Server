using API.Models;
using API.Models.Connexionz;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Components
{
    public class CacheManager
    {
        // todo: centralize this
        private const string ROUTES_KEY = "routes";
        private const string STOPS_KEY = "stops";
        private const string PLATFORM_TAGS_KEY = "platformTags";
        private const string SCHEDULE_KEY = "schedule";

        private string _connectionString;
        private string _routeskey;
        private string _stopsKey;
        private string _platformsKey;
        private string _scheduleKey;

        private ConnectionMultiplexer _conn;
        private StorageManager _storageManager;

        /// <summary>
        /// Connection to the Redis cache.
        /// </summary>
        private ConnectionMultiplexer Connection
        {
            get
            {
                if (_conn == null)
                {
                    _conn = ConnectionMultiplexer.Connect(_connectionString);
                }

                return _conn;
            }
        }

        public CacheManager(AppSettings appSettings, StorageManager storageManager)
        {
            _connectionString = appSettings.RedisConnectionString;
            _routeskey = appSettings.RoutesKey;
            _stopsKey = appSettings.StopsKey;
            _platformsKey = appSettings.PlatformsKey;
            _scheduleKey = appSettings.SchedulesKey;
            _storageManager = storageManager;
        }

        internal void ClearCache()
        {
            var cache = Connection.GetDatabase();

            cache.StringSet(_routeskey, string.Empty);
            cache.StringSet(_stopsKey, string.Empty);
            cache.StringSet(_platformsKey, string.Empty);
        }

        /// <summary>
        /// Gets static routes from the cache.  Grabs the blob data if the cache is empty.
        /// </summary>
        public async Task<List<BusRoute>> GetStaticRoutesAsync()
        {
            var cache = Connection.GetDatabase();

            var json = await cache.StringGetAsync(ROUTES_KEY);
            if (string.IsNullOrEmpty(json))
            {
                json = await _storageManager.GetStaticRouteDataAsync();
                cache.StringSet(_routeskey, json);
            }

            return JsonConvert.DeserializeObject<List<BusRoute>>(json);
        }

        /// <summary>
        /// Gets static stops from the cache.  Grabs the blob data if the cache is empty.
        /// </summary>
        /// <returns></returns>
        public async Task<List<BusStop>> GetStaticStopsAsync()
        {
            var cache = Connection.GetDatabase();

            var json = await cache.StringGetAsync(STOPS_KEY);
            if (string.IsNullOrEmpty(json))
            {
                json = await _storageManager.GetStaticStopDataAsync();
                cache.StringSet(_stopsKey, json);
            }

            return JsonConvert.DeserializeObject<List<BusStop>>(json);
        }

        /// <summary>
        /// Gets the platform tag dictionary from the cache.  Grabs the blob data if the cache is empty.
        /// </summary>
        public async Task<Dictionary<string, string>> GetPlatformTagsAsync()
        {
            var cache = Connection.GetDatabase();

            var json = await cache.StringGetAsync(PLATFORM_TAGS_KEY);
            if (string.IsNullOrWhiteSpace(json))
            {
                json = await _storageManager.GetPlatformTagsAsync();
                cache.StringSet(_platformsKey, json);
            }

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        /// <summary>
        /// Gets schedule data from the cache.  Handles entry management.
        /// </summary>
        public async Task<Dictionary<string, IEnumerable<BusStopRouteSchedule>>> GetScheduleAsync()
        {
            var cache = Connection.GetDatabase();

            var json = await cache.StringGetAsync(SCHEDULE_KEY);
            if (string.IsNullOrWhiteSpace(json))
            {
                json = await _storageManager.GetScheduleAsync();
                cache.StringSet(_scheduleKey, json);
            }

            return JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<BusStopRouteSchedule>>>(json);
        }

        /// <summary>
        /// Extracts a platform ETA from the cache.  Handles entry management.  Note that this is a synchronous call!
        /// </summary>
        public ConnexionzPlatformET GetEta(string platformTag)
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