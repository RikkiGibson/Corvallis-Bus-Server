using System.Collections.Generic;

/// <summary>
/// Represents the schedule for a route taken from Google Transit.
/// </summary>
namespace CorvallisBus.Core.Models.GoogleTransit
{
    public class GoogleRouteSchedule
    {
        public string RouteNo { get; set; }

        public List<GoogleDaySchedule> Days { get; set; }

        public string ConnexionzName => GoogleRoute.ToConnexionzName(RouteNo);
    }
}