
global using ServerBusSchedule =
    System.Collections.Generic.Dictionary<
        int,
        System.Collections.Generic.IEnumerable<CorvallisBus.Core.Models.BusStopRouteSchedule>>;

using CorvallisBus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Represents all of the ahead-of-time derived data about the bus system.
    /// </summary>
    /// <param name="PlatformIdToPlatformTag">
    /// Maps a platform number (5-digit number shown on real bus stop signs) to a platform tag (3-digit internal Connexionz identifier).
    /// </param>
    public record BusSystemData(
        BusStaticData StaticData,
        ServerBusSchedule Schedule,
        Dictionary<int, int> PlatformIdToPlatformTag);
}
