using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using CorvallisTransit.Components;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class TransitController : Controller
    {
        [HttpGet]
        [Route("static")]
        public async Task<string> GetStaticData()
        {
            var staticData = await TransitClient.GetStaticData();
            return JsonConvert.SerializeObject(staticData);
        }
    }
}
