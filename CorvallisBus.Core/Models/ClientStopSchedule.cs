using System;
using System.Collections.Generic;
using System.Text;

namespace CorvallisBus.Core.Models
{
    public sealed class ClientStopSchedule : Lookup<string, List<BusArrivalTime>>
    {
        public ClientStopSchedule(IDictionary<string, List<BusArrivalTime>> dict) : base(dict) { }

        public List<BusArrivalTime> GetRouteSchedule(string routeNo) => _dict[routeNo];
    }
}
