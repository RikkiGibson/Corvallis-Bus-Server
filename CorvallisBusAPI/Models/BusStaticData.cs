﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    /// <summary>
    /// Container for the set of static data which is expected to
    /// remain the same during a client application lifecycle.
    /// </summary>
    public class BusStaticData
    {
        public Dictionary<string, BusRoute> Routes { get; set; }
        public Dictionary<int, BusStop> Stops { get; set; }
    }
}
