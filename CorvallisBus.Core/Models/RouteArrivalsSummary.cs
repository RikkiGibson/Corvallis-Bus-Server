using CorvallisBus.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorvallisBus.Core.Models
{
    public class RouteArrivalsSummary : IEquatable<RouteArrivalsSummary>
    {
        [JsonProperty("routeName")]
        public string RouteName { get; set; }

        [JsonProperty("arrivalsSummary")]
        public string ArrivalsSummary { get; set; }

        [JsonProperty("scheduleSummary")]
        public string ScheduleSummary { get; set; }

        public RouteArrivalsSummary() { }

        public RouteArrivalsSummary(string routeName, List<BusArrivalTime> routeArrivalTimes, DateTimeOffset currentTime)
        {
            RouteName = routeName;
            ArrivalsSummary = ToEstimateSummary(routeArrivalTimes, currentTime);
            ScheduleSummary = ToScheduleSummary(routeArrivalTimes, currentTime);
        }

        public static string ToEstimateSummary(List<BusArrivalTime> arrivals, DateTimeOffset currentTime)
        {
            switch (arrivals.Count)
            {
                case 0: return "No arrivals!";
                case 1: return ArrivalTimeDescription(arrivals[0], currentTime, isFirstElement: true);
                default: return ArrivalTimeDescription(arrivals[0], currentTime, isFirstElement: true) + ", then " +
                        ArrivalTimeDescription(arrivals[1], currentTime, isFirstElement: false);
            }
        }

        private const string DATE_FORMAT = "h:mm tt";
        private static string ArrivalTimeDescription(BusArrivalTime relativeArrivalTime, DateTimeOffset currentTime, bool isFirstElement)
        {
            Debug.Assert(relativeArrivalTime.MinutesFromNow > 0);

            int minutes = relativeArrivalTime.MinutesFromNow;
            if (minutes == 1)
            {
                return "1 minute";
            }
            else if (relativeArrivalTime.IsEstimate &&
                     minutes >= 2 &&
                     minutes <= TransitManager.ESTIMATES_MAX_ADVANCE_MINUTES)
            {
                return $"{minutes} minutes";
            }
            else if (!relativeArrivalTime.IsEstimate &&
                     minutes <= TransitManager.ESTIMATES_MAX_ADVANCE_MINUTES)
            {
                return isFirstElement
                    ? "Over 30 minutes"
                    : "over 30 minutes";
            }
            else
            {
                var arrivalTime = currentTime.AddMinutes(minutes);
                return arrivalTime.ToString(DATE_FORMAT);
            }
        }

        public static string ToScheduleSummary(List<BusArrivalTime> arrivals, DateTimeOffset currentTime)
        {
            if (arrivals.Count >= 0 && arrivals.Count <= 2)
            {
                return string.Empty;
            }

            var lastTime = currentTime.AddMinutes(arrivals.Last().MinutesFromNow);
            var lastTimeDescription = lastTime.ToString(DATE_FORMAT);

            if (arrivals.Count == 3)
            {
                return "Last arrival at " + lastTimeDescription; 
            }

            // Check for whether there's a regular half-hourly or hourly arrival pattern.
            // If not, exit the loop early.
            bool isHourly = true;
            bool isHalfHourly = true;
            for (int i = 1; i < arrivals.Count - 1 && (isHourly || isHalfHourly); i++)
            {
                int difference = arrivals[i + 1].MinutesFromNow - arrivals[i].MinutesFromNow;
                isHourly = isHourly && difference >= 50 && difference <= 70;
                isHalfHourly = isHalfHourly && difference >= 20 && difference <= 40; 
            }

            if (isHourly)
            {
                return "Hourly until " + lastTimeDescription;
            }
            else if (isHalfHourly)
            {
                return "Every 30 minutes until " + lastTimeDescription;
            }
            else
            {
                return "Last arrival at " + lastTimeDescription;
            }
        }

        public bool Equals(RouteArrivalsSummary other)
        {
            return RouteName == other.RouteName &&
                ArrivalsSummary == other.ArrivalsSummary &&
                ScheduleSummary == other.ScheduleSummary;
        }
    }
}
