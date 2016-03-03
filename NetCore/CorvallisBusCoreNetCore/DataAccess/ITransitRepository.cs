using CorvallisBusCoreNetCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorvallisBusCoreNetCore.DataAccess
{
    /// <summary>
    /// Maps a 5-digit stop ID to a list of route schedules.
    /// </summary>
    using ServerBusSchedule = Dictionary<int, IEnumerable<BusStopRouteSchedule>>;

    /// <summary>
    /// This interface abstracts over persistent and cache storage.
    /// </summary>
    public interface ITransitRepository
    {
        /// <summary>
        /// Returns route and stop information intended for direct client consumption.
        /// This is specifically left as a string instead of a BusStaticData
        /// to eliminate the need for deserialization and reserialization.
        /// </summary>
        Task<string> GetSerializedStaticDataAsync();

        Task<BusStaticData> GetStaticDataAsync();

        Task<Dictionary<int, int>> GetPlatformTagsAsync();

        Task<ServerBusSchedule> GetScheduleAsync();

        void SetStaticData(BusStaticData staticData);

        void SetSchedule(ServerBusSchedule schedule);

        void SetPlatformTags(Dictionary<int, int> platformTags);
    }
}
