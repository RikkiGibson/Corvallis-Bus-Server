using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorvallisBusCoreNetCore.Models
{
    public class RouteArrivalsSummary
    {
        [JsonProperty("routeName")]
        public string RouteName { get; set; }

        [JsonProperty("routeColor")]
        public string RouteColor { get; set; }

        [JsonProperty("arrivalsSummary")]
        public string ArrivalsSummary { get; set; }

        [JsonProperty("scheduleSummary")]
        public string ScheduleSummary { get; set; }

        public RouteArrivalsSummary(string routeName, string routeColor, List<int> routeArrivalTimes, DateTimeOffset currentTime)
        {
            RouteName = routeName;
            RouteColor = routeColor;
            ArrivalsSummary = ToEstimateSummary(routeArrivalTimes, currentTime);
            ScheduleSummary = ToScheduleSummary(routeArrivalTimes, currentTime);
        }

        public static string ToEstimateSummary(List<int> arrivals, DateTimeOffset currentTime)
        {
            switch (arrivals.Count)
            {
                case 0: return "No arrivals!";
                case 1: return ArrivalTimeDescription(arrivals[0], currentTime);
                default: return ArrivalTimeDescription(arrivals[0], currentTime) + ", " + ArrivalTimeDescription(arrivals[1], currentTime);
            }
        }

        private static string ArrivalTimeDescription(int minutes, DateTimeOffset currentTime)
        {
            if (minutes == 1)
            {
                return "1 minute";
            }
            else if (minutes >= 2 && minutes <= 30)
            {
                return $"{minutes} minutes";
            }
            else
            {
                var arrivalTime = currentTime.AddMinutes(minutes);
                return arrivalTime.ToString("hh:mm tt");
            }
        }

        public static string ToScheduleSummary(List<int> arrivals, DateTimeOffset currentTime)
        {
            if (arrivals.Count >= 0 && arrivals.Count <= 2)
            {
                return string.Empty;
            }

            var lastTime = currentTime.AddMinutes(arrivals.Last());
            var lastTimeDescription = lastTime.ToString("hh:mm tt");

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
                int difference = arrivals[i + 1] - arrivals[i];
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
    }
}
