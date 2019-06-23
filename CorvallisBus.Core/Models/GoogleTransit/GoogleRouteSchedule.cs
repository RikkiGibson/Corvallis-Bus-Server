using System.Collections.Generic;

/// <summary>
/// Represents the schedule for a route taken from Google Transit.
/// </summary>
namespace CorvallisBus.Core.Models.GoogleTransit
{
    public class GoogleRouteSchedule
    {
        public string RouteNo { get; }

        public List<GoogleDaySchedule> Days { get; }

        public string ConnexionzName => GoogleRoute.ToConnexionzName(RouteNo);

        public GoogleRouteSchedule(
            string routeNo,
            List<GoogleDaySchedule> days)
        {
            RouteNo = routeNo;
            Days = days;
        }
    }
}