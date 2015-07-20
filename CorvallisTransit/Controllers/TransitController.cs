using CorvallisTransit.Components;
using CorvallisTransit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace CorvallisTransit.Controllers
{
    [RoutePrefix("transit")]
    public class TransitController : ApiController
    {
        // GET api/transit

        [HttpGet]
        [Route("")]
        public JsonResult<List<ClientData>> Get()
        {
            return Json(TransitClient.Routes.Select(rt => rt.ClientData).ToList());
        }

        [HttpGet]
        [Route("{routeNo}")]
        public JsonResult<ClientData> Get(string routeNo)
        {
               return Json(TransitClient.Routes.FirstOrDefault(rt => rt.RouteNo == routeNo).ClientData);
        }
    }
}