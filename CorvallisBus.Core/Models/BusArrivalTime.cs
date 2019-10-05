using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorvallisBus.Core.Models
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
            if (IsEstimate && !other.IsEstimate)
            {
                return -1;
            }

            if (!IsEstimate && other.IsEstimate)
            {
                return 1;
            }

            return MinutesFromNow - other.MinutesFromNow;
        }

        [JsonProperty("minutesFromNow")]
        public int MinutesFromNow { get; }
        
        [JsonProperty("isEstimate")]
        public bool IsEstimate { get; }

        public override string ToString() => $"{{ MinutesFromNow = {MinutesFromNow}, IsEstimate = {IsEstimate} }}";
    }
}
