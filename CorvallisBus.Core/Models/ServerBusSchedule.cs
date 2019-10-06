using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Contains the entire bus system schedule (all times of day, all days of the week).
    /// </summary>
    public sealed class ServerBusSchedule : Lookup<int, List<BusStopRouteSchedule>>
    {
        public ServerBusSchedule(Dictionary<int, List<BusStopRouteSchedule>> dict) : base(dict) { }

        public ServerBusSchedule() : base(new Dictionary<int, List<BusStopRouteSchedule>>()) { }

        public bool Contains(int stopId) => _dict.ContainsKey(stopId);

        public List<BusStopRouteSchedule> this[int stopId]
        {
            get => _dict[stopId];
            set => _dict[stopId] = value;
        }

        public void Add(int stopId, List<BusStopRouteSchedule> stopSchedule)
        {
            _dict.Add(stopId, stopSchedule);
        }
    }
}
