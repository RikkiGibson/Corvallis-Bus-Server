using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DataAccess
{
    /// <summary>
    /// Maps a 5-digit stop ID to a list of route schedules.
    /// </summary>
    using ServerBusSchedule = Dictionary<string, IEnumerable<BusStopRouteSchedule>>;

    /// <summary>
    /// This interface abstracts over persistent and cache storage.
    /// It strictly produces and consumes JSON for efficiency.
    /// </summary>
    public interface ITransitRepository
    {
        // These aren't for direct client consumption but will be needed when
        // we start puking out pre-digested view models.
        // Should the static data have a named type instead?
        // Should there just be an overload for obtaining deserialized static data?
        Task<List<BusRoute>> GetRoutesAsync();
        Task<List<BusStop>> GetStopsAsync();

        /// <summary>
        /// Returns route and stop information intended for direct client consumption.
        /// </summary>
        Task<string> GetStaticDataAsync();

        Task<Dictionary<string, string>> GetPlatformTagsAsync();

        Task<ServerBusSchedule> GetScheduleAsync();

        //Task<string> GetEtasAsync(IEnumerable<string> stopIds);

        void SetRoutes(List<BusRoute> routes);
        void SetStops(List<BusStop> stops);

        void SetStaticData(string staticDataJson);

        void SetSchedule(ServerBusSchedule schedule);

        void SetPlatformTags(Dictionary<string, string> platformTags);

        //void SetEtas(string etasJson);
    }
}
