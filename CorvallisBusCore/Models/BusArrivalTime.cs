using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorvallisBusCore.Models
{


    public struct BusArrivalTime : IComparable<BusArrivalTime>
    {
        public BusArrivalTime(int minutesFromNow, bool isEstimate)
        {
            MinutesFromNow = minutesFromNow;
            IsEstimate = isEstimate;
        }

        public static BusArrivalTime Min(BusArrivalTime a, BusArrivalTime b)
        {
            return a.MinutesFromNow < b.MinutesFromNow ? a : b;
        }

        public int CompareTo(BusArrivalTime other)
        {
            return MinutesFromNow - other.MinutesFromNow;
        }

        [JsonProperty("minutesFromNow")]
        public int MinutesFromNow { get; private set; }
        
        [JsonProperty("isEstimate")]
        public bool IsEstimate { get; private set; }
    }
}
