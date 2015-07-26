using System;
using System.Linq;
using System.Web.Mvc;
using CorvallisTransit.Models;
using CorvallisTransit.Components;

namespace CorvallisTransit.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult BusSchedule()
        {
            try
            {
                // we're only showing the stops that we know anything about
                var routes = TransitClient.Routes;

                if (routes != null)
                {
                    return View(routes);
                }
                else
                {
                    throw new ArgumentNullException("Routes was null!");
                }
            }
            catch (Exception)
            {
                // do logging or something later, give a good view
                return View();
            }
        }

        /// <summary>
        /// Returns the BusRoute table info from a route number.
        /// </summary>
        /// <param name="id">The route no.</param>
        /// <returns></returns>
        public PartialViewResult BusRoute(string id)
        {
            return BusRouteFromModel(TransitClient.Routes.FirstOrDefault(r => r.RouteNo == id));
        }

        /// <summary>
        /// Returns the BusRoute table info from a model
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        public PartialViewResult BusRouteFromModel(BusRoute route)
        {
            
            return PartialView("BusRoutePartial",route);
        }
    }
}