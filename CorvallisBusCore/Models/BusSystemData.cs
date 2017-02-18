using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorvallisBusCore.Models
{
    using ServerBusSchedule = Dictionary<int, IEnumerable<BusStopRouteSchedule>>;

    /// <summary>
    /// Represents all of the ahead-of-time derived data about the bus system.
    /// </summary>
    public class BusSystemData
    {
        public BusStaticData StaticData { get; }
        public ServerBusSchedule Schedule { get; }

        /// <summary>
        /// Maps a platform number (5-digit number shown on real bus stop signs) to a platform tag (3-digit internal Connexionz identifier).
        /// </summary>
        public Dictionary<int, int> PlatformIdToPlatformTag { get; }

        public BusSystemData(
            BusStaticData staticData,
            ServerBusSchedule schedule,
            Dictionary<int, int> platformIdToPlatformTag)
        {
            StaticData = staticData;
            Schedule = schedule;
            PlatformIdToPlatformTag = platformIdToPlatformTag;
        }
    }
}
