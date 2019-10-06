using System;
using System.Collections.Generic;
using System.Text;

namespace CorvallisBus.Core.Models
{
    public sealed class ClientBusSchedule : Lookup<int, ClientStopSchedule>
    {
        public ClientBusSchedule(IDictionary<int, ClientStopSchedule> dict) : base(dict) { }

        public ClientStopSchedule GetStopSchedule(int stopId) => _dict[stopId];
    }
}
