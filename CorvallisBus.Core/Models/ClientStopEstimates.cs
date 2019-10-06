using System;
using System.Collections.Generic;
using System.Text;

namespace CorvallisBus.Core.Models
{
    public sealed class ClientStopEstimates : Lookup<string, List<int>>
    {
        public ClientStopEstimates(IDictionary<string, List<int>> dict) : base(dict) { }
        public ClientStopEstimates() : base(new Dictionary<string, List<int>>()) { }

        /// <summary>
        /// Gets the list of "minutes from now" estimates for the given route number.
        /// </summary>
        public List<int> GetEstimates(string routeNo) => _dict[routeNo];

        public bool ContainsRoute(string routeNo) => _dict.ContainsKey(routeNo);
    }
}
