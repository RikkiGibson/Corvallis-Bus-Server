using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Represents the schedule for a route taken from Google Transit.
/// </summary>
namespace CorvallisTransit.Models.GoogleTransit
{
    public class GoogleRouteSchedule
    {
        public string RouteNo { get; set; }

        public List<GoogleDaySchedule> Days { get; set;}
    }
}