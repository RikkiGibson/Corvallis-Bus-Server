using API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DataAccess
{
    using ServerBusSchedule = Dictionary<string, IEnumerable<BusStopRouteSchedule>>;

    public class TransitRepository : ITransitRepository
    {
        private StorageManager _storageManager;
        private CacheManager _cacheManager;

        public TransitRepository(AppSettings settings)
        {
            _storageManager = new StorageManager(settings);
            _cacheManager = new CacheManager(settings);
        }

        public async Task<Dictionary<string, string>> GetPlatformTagsAsync()
        {
            var cacheJson = await _cacheManager.GetPlatformTagsAsync();
            if (string.IsNullOrWhiteSpace(cacheJson))
            {
                var storageJson = await _storageManager.GetPlatformTagsAsync();
                _cacheManager.SetPlatformTags(storageJson);
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(storageJson);
            }

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(cacheJson);
        }

        public async Task<ServerBusSchedule> GetScheduleAsync()
        {
            var cacheJson = await _cacheManager.GetScheduleAsync();
            if (string.IsNullOrWhiteSpace(cacheJson))
            {
                var storageJson = await _storageManager.GetScheduleAsync();
                _cacheManager.SetSchedule(storageJson);
                return JsonConvert.DeserializeObject<ServerBusSchedule>(storageJson);
            }

            return JsonConvert.DeserializeObject<ServerBusSchedule>(cacheJson);
        }

        public async Task<List<BusRoute>> GetRoutesAsync()
        {
            var cacheJson = await _cacheManager.GetRoutesAsync();
            if (string.IsNullOrWhiteSpace(cacheJson))
            {
                var storageJson = await _storageManager.GetRoutesAsync();
                _cacheManager.SetRoutes(storageJson);
                return JsonConvert.DeserializeObject<List<BusRoute>>(storageJson);
            }

            return JsonConvert.DeserializeObject<List<BusRoute>>(cacheJson);
        }

        public async Task<List<BusStop>> GetStopsAsync()
        {
            var cacheJson = await _cacheManager.GetStopsAsync();
            if (string.IsNullOrWhiteSpace(cacheJson))
            {
                var storageJson = await _storageManager.GetStopsAsync();
                _cacheManager.SetStops(storageJson);
                return JsonConvert.DeserializeObject<List<BusStop>>(storageJson);
            }

            return JsonConvert.DeserializeObject<List<BusStop>>(cacheJson);
        }

        public async Task<string> GetStaticDataAsync()
        {
            var cacheJson = await _cacheManager.GetStaticDataAsync();
            if (string.IsNullOrWhiteSpace(cacheJson))
            {
                var storageJson = await _storageManager.GetStaticDataAsync();
                _cacheManager.SetStaticData(storageJson);
                return storageJson;
            }

            return cacheJson;
        }

        public void SetPlatformTags(Dictionary<string, string> platformTags)
        {
            var platformTagsJson = JsonConvert.SerializeObject(platformTags);
            _storageManager.SetPlatformTags(platformTagsJson);
            _cacheManager.SetPlatformTags(platformTagsJson);
        }
        
        public void SetRoutes(List<BusRoute> routes)
        {
            var routesJson = JsonConvert.SerializeObject(routes);
            _storageManager.SetRoutes(routesJson);
            _cacheManager.SetRoutes(routesJson);
        }

        public void SetStops(List<BusStop> stops)
        {
            var stopsJson = JsonConvert.SerializeObject(stops);
            _storageManager.SetStops(stopsJson);
            _cacheManager.SetStops(stopsJson);
        }

        public void SetStaticData(string staticDataJson)
        {
            _storageManager.SetStaticData(staticDataJson);
            _cacheManager.SetStaticData(staticDataJson);
        }

        public void SetSchedule(ServerBusSchedule schedule)
        {
            var scheduleJson = JsonConvert.SerializeObject(schedule);
            _storageManager.SetSchedule(scheduleJson);
            _cacheManager.SetSchedule(scheduleJson);
        }
    }
}
