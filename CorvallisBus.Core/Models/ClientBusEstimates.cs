using System;
using System.Collections.Generic;
using System.Text;

namespace CorvallisBus.Core.Models
{
    public sealed class ClientBusEstimates : Lookup<int, ClientStopEstimates>
    {
        public ClientBusEstimates(IDictionary<int, ClientStopEstimates> dict) : base(dict) { }

        public ClientStopEstimates this[int stopId] => _dict[stopId];
    }
}
