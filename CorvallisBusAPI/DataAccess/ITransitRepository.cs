using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DataAccess
{
    /// <summary>
    /// This interface abstracts over persistent and cache storage.
    /// It strictly produces and consumes JSON for efficiency.
    /// </summary>
    public interface ITransitRepository
    {
        Task<string> GetRoutesAsync();

        Task<string> GetStopsAsync();

        Task<string> GetPlatformTagsAsync();

        Task<string> GetScheduleAsync();

        //Task<string> GetEtasAsync(IEnumerable<string> stopIds);

        void SetRoutes(string routesJson);

        void SetStops(string stopsJson);

        void SetSchedule(string scheduleJson);

        void SetPlatformTags(string platformTagsJson);

        //void SetEtas(string etasJson);
    }
}
