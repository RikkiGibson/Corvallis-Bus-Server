using CorvallisTransit.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CorvallisTransit.Models
{
    public class BusRoute
    {
        public string Destination { get; set; }

        public string RouteNo { get; set; }

        public List<BusRouteStop> Stops { get; set; }

        public int SubRouteOrder { get; set; }

        public bool RouteTimeWarning { get; set; }

        public bool UpdatedSuccessfully { get; set; }

        public List<BusRouteStop> OrderedStops
        {
            get
            {
                return Stops
                    .Where(s => s.Eta != 0)
                    .OrderBy(s => s, TransitComparers.TransitComparer)
                    .ToList();
            }
        }

        public ClientData ClientData
        {
            get
            {
                return new ClientData()
                    {
                        num = RouteNo,
                        updateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time")),
                        warningClasses = string.Format("{0} {1}",
                            RouteTimeWarning ? "suspiciousTime" : string.Empty,
                            UpdatedSuccessfully ? string.Empty : "failedToUpdate"),
                        stops = OrderedStops.Select(st =>
                            new
                            {
                                eta = st.Eta,
                                model = st.StopModel
                            }
                        )
                    };
            }
        }
    }
}