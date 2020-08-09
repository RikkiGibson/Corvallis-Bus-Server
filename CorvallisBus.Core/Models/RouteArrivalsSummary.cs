﻿using CorvallisBus.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorvallisBus.Core.Models
{
    public record RouteArrivalsSummary(
        [property: JsonProperty("routeName")]
        string RouteName,

        [property: JsonProperty("arrivalsSummary")]
        string ArrivalsSummary,

        [property: JsonProperty("scheduleSummary")]
        string ScheduleSummary
        )
    {

        public static RouteArrivalsSummary Create(string routeName, List<BusArrivalTime> routeArrivalTimes, DateTimeOffset currentTime)
        {
            return new RouteArrivalsSummary(
                routeName,
                ToEstimateSummary(routeArrivalTimes, currentTime),
                ToScheduleSummary(routeArrivalTimes, currentTime));
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
            if (arrivals.Count <= 2)
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
            bool isTwoHourly = true;
            bool isHourly = true;
            bool isHalfHourly = true;
            for (int i = 1; i < arrivals.Count - 1 && (isTwoHourly || isHourly || isHalfHourly); i++)
            {
                int difference = arrivals[i + 1].MinutesFromNow - arrivals[i].MinutesFromNow;
                isTwoHourly = isTwoHourly && difference >= 110 && difference <= 130;
                isHourly = isHourly && difference >= 50 && difference <= 70;
                isHalfHourly = isHalfHourly && difference >= 20 && difference <= 40;
            }

            if (isTwoHourly)
            {
                return "Every 2 hours until " + lastTimeDescription;
            }
            else if (isHourly)
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

        // TODO: use the auto-generated ToString() once it ships
        public override string ToString()
        {
            return $@"{{ RouteName = ""{RouteName}"", ArrivalsSummary = ""{ArrivalsSummary}"", ScheduleSummary = ""{ScheduleSummary}"" }}";
        }
    }
}
