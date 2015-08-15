using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DataAccess
{
    public class TransitRepository : ITransitRepository
    {
        private StorageManager _storageManager;
        private CacheManager _cacheManager;

        public TransitRepository(AppSettings settings)
        {
            _storageManager = new StorageManager(settings);
            _cacheManager = new CacheManager(settings);
        }

        public async Task<string> GetPlatformTagsAsync()
        {
            var cacheJson = await _cacheManager.GetPlatformTagsAsync();
            if (string.IsNullOrWhiteSpace(cacheJson))
            {
                var storageJson = await _storageManager.GetPlatformTagsAsync();
                _cacheManager.SetPlatformTags(storageJson);
                return storageJson;
            }

            return cacheJson;
        }

        public async Task<string> GetScheduleAsync()
        {
            var cacheJson = await _cacheManager.GetScheduleAsync();
            if (string.IsNullOrWhiteSpace(cacheJson))
            {
                var storageJson = await _storageManager.GetScheduleAsync();
                _cacheManager.SetSchedule(storageJson);
                return storageJson;
            }

            return cacheJson;
        }

        public async Task<string> GetRoutesAsync()
        {
            var cacheJson = await _cacheManager.GetRoutesAsync();
            if (string.IsNullOrWhiteSpace(cacheJson))
            {
                var storageJson = await _storageManager.GetRoutesAsync();
                _cacheManager.SetRoutes(storageJson);
                return storageJson;
            }

            return cacheJson;
        }

        public async Task<string> GetStopsAsync()
        {
            var cacheJson = await _cacheManager.GetStopsAsync();
            if (string.IsNullOrWhiteSpace(cacheJson))
            {
                var storageJson = await _storageManager.GetStopsAsync();
                _cacheManager.SetStops(storageJson);
                return storageJson;
            }

            return cacheJson;
        }

        public void SetPlatformTags(string platformTagsJson)
        {
            _storageManager.SetPlatformTags(platformTagsJson);
            _cacheManager.SetPlatformTags(platformTagsJson);
        }

        public void SetRoutes(string routesJson)
        {
            _storageManager.SetRoutes(routesJson);
            _cacheManager.SetRoutes(routesJson);
        }

        public void SetSchedule(string scheduleJson)
        {
            _storageManager.SetSchedule(scheduleJson);
            _cacheManager.SetSchedule(scheduleJson);
        }

        public void SetStops(string stopsJson)
        {
            _storageManager.SetStops(stopsJson);
            _cacheManager.SetStops(stopsJson);
        }
    }
}
