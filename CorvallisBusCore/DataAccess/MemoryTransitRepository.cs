using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Models;
using Newtonsoft.Json;
using System.IO;

namespace API.DataAccess
{
    /// <summary>
    /// Transit repository which stores data in memory and in flat files for when memory is cleared.
    /// </summary>
    public class MemoryTransitRepository : ITransitRepository
    {
        private static Dictionary<int, int> s_platformTags;
        private static Dictionary<int, IEnumerable<BusStopRouteSchedule>> s_schedule;
        private static string s_staticData;

        private string filePath;
        public MemoryTransitRepository(string filePath)
        {
            this.filePath = filePath;
        }

        public Task<Dictionary<int, int>> GetPlatformTagsAsync()
        {
            if (s_platformTags == null)
            {
                s_platformTags = JsonConvert.DeserializeObject<Dictionary<int, int>>(File.ReadAllText(filePath + "/platformTags.json"));
            }
            return Task.FromResult(s_platformTags);
        }

        public Task<Dictionary<int, IEnumerable<BusStopRouteSchedule>>> GetScheduleAsync()
        {
            if (s_schedule == null)
            {
                s_schedule = JsonConvert.DeserializeObject<Dictionary<int, IEnumerable<BusStopRouteSchedule>>>(File.ReadAllText(filePath + "/schedule.json"));
            }
            return Task.FromResult(s_schedule);
        }

        public Task<string> GetStaticDataAsync()
        {
            if (s_staticData == null)
            {
                s_staticData = File.ReadAllText(filePath + "/staticData.json");
            }
            return Task.FromResult(s_staticData);
        }

        public void SetPlatformTags(Dictionary<int, int> platformTags)
        {
            s_platformTags = platformTags;
            File.WriteAllText(filePath + "/platformTags.json", JsonConvert.SerializeObject(platformTags));
        }

        public void SetSchedule(Dictionary<int, IEnumerable<BusStopRouteSchedule>> schedule)
        {
            s_schedule = schedule;
            File.WriteAllText(filePath + "/schedule.json", JsonConvert.SerializeObject(schedule));
        }

        public void SetStaticData(BusStaticData staticData)
        {
            s_staticData = JsonConvert.SerializeObject(staticData);
            File.WriteAllText(filePath + "/staticData.json", JsonConvert.SerializeObject(staticData));
        }
    }
}
