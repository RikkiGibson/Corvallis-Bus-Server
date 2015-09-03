using API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DataAccess
{
    using ServerBusSchedule = Dictionary<int, IEnumerable<BusStopRouteSchedule>>;

    public class TransitRepository : ITransitRepository
    {
        private StorageManager _storageManager;
        private CacheManager _cacheManager;

        public TransitRepository(AppSettings settings)
        {
            _storageManager = new StorageManager(settings);
            _cacheManager = new CacheManager(settings);
        }

        public async Task<Dictionary<int, int>> GetPlatformTagsAsync()
        {
            var cacheJson = await _cacheManager.GetPlatformTagsAsync();
            if (string.IsNullOrWhiteSpace(cacheJson))
            {
                var storageJson = await _storageManager.GetPlatformTagsAsync();
                _cacheManager.SetPlatformTags(storageJson);
                return JsonConvert.DeserializeObject<Dictionary<int, int>>(storageJson);
            }

            return JsonConvert.DeserializeObject<Dictionary<int, int>>(cacheJson);
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

        public void SetPlatformTags(Dictionary<int, int> platformTags)
        {
            var platformTagsJson = JsonConvert.SerializeObject(platformTags);
            _storageManager.SetPlatformTags(platformTagsJson);
            _cacheManager.SetPlatformTags(platformTagsJson);
        }

        public void SetStaticData(BusStaticData staticData)
        {
            var staticDataJson = JsonConvert.SerializeObject(staticData);
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
