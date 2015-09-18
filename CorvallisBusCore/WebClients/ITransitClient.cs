using API.Models;
using API.Models.Connexionz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.WebClients
{
    using ServerBusSchedule = Dictionary<int, IEnumerable<BusStopRouteSchedule>>;

    public interface ITransitClient
    {
        List<BusStop> CreateStops();
        List<BusRoute> CreateRoutes();
        BusStaticData CreateStaticData();
        Dictionary<int, int> CreatePlatformTags();
        Task<ConnexionzPlatformET> GetEta(int platformTag);
        ServerBusSchedule CreateSchedule();
    }
}
