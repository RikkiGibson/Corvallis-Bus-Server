using CorvallisBus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorvallisBus.Core.Models
{
    using ServerBusSchedule = Dictionary<int, IEnumerable<BusStopRouteSchedule>>;

    /// <summary>
    /// Represents all of the ahead-of-time derived data about the bus system.
    /// </summary>
    public record BusSystemData(
        BusStaticData StaticData,
        ServerBusSchedule Schedule,

        /// <summary>
        /// Maps a platform number (5-digit number shown on real bus stop signs) to a platform tag (3-digit internal Connexionz identifier).
        /// </summary>
        Dictionary<int, int> PlatformIdToPlatformTag
        );
}
