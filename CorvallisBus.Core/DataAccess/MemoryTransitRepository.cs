using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorvallisBus.Core.Models;
using Newtonsoft.Json;
using System.IO;

namespace CorvallisBus.Core.DataAccess
{
    /// <summary>
    /// Transit repository which stores data in memory and in flat files for when memory is cleared.
    /// </summary>
    public class MemoryTransitRepository : ITransitRepository
    {
        private static Dictionary<int, int>? s_platformTags;
        private static ServerBusSchedule? s_schedule;
        private static string? s_serializedStaticData;
        private static BusStaticData? s_staticData;

        private readonly string _platformTagsPath;
        private readonly string _schedulePath;

        public string StaticDataPath { get; }

        public MemoryTransitRepository(string filePath)
        {
            var folder = filePath + "/cache";
            Directory.CreateDirectory(folder);

            _platformTagsPath = folder + "/platformTags.json";
            _schedulePath = folder + "/schedule.json";
            StaticDataPath = folder + "/staticData.json";
        }

        public Task<Dictionary<int, int>> GetPlatformTagsAsync()
        {
            if (s_platformTags == null)
            {
                s_platformTags = JsonConvert.DeserializeObject<Dictionary<int, int>>(File.ReadAllText(_platformTagsPath));
            }
            return Task.FromResult(s_platformTags);
        }

        public Task<ServerBusSchedule> GetScheduleAsync()
        {
            if (s_schedule == null)
            {
                s_schedule = JsonConvert.DeserializeObject<ServerBusSchedule>(File.ReadAllText(_schedulePath));
            }
            return Task.FromResult(s_schedule);
        }

        public Task<string> GetSerializedStaticDataAsync()
        {
            if (s_serializedStaticData == null)
            {
                s_serializedStaticData = File.ReadAllText(StaticDataPath);
            }
            return Task.FromResult(s_serializedStaticData);
        }

        public async Task<BusStaticData> GetStaticDataAsync()
        {
            if (s_staticData == null)
            {
                s_staticData = JsonConvert.DeserializeObject<BusStaticData>(await GetSerializedStaticDataAsync());
            }
            return s_staticData;
        }

        public void SetPlatformTags(Dictionary<int, int> platformTags)
        {
            s_platformTags = platformTags;
            File.WriteAllText(_platformTagsPath, JsonConvert.SerializeObject(platformTags));
        }

        public void SetSchedule(ServerBusSchedule schedule)
        {
            s_schedule = schedule;
            File.WriteAllText(_schedulePath, JsonConvert.SerializeObject(schedule));
        }

        public void SetStaticData(BusStaticData staticData)
        {
            s_staticData = staticData;
            s_serializedStaticData = JsonConvert.SerializeObject(staticData);
            File.WriteAllText(StaticDataPath, JsonConvert.SerializeObject(staticData));
        }
    }
}
