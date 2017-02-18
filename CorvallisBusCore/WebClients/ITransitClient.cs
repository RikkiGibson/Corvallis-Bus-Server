using API.Models;
using API.Models.Connexionz;
using CorvallisBusCore.Models;
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
        BusSystemData LoadTransitData();
        Task<ConnexionzPlatformET> GetEta(int platformTag);
    }
}
