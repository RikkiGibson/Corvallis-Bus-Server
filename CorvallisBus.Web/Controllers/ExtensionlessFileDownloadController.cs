using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace CorvallisBusWeb.Controllers
{
    /// <summary>
    /// Workaround for the Handoff requirement to deliver an extensionless file.
    /// https://developer.apple.com/library/ios/documentation/UserExperience/Conceptual/Handoff/AdoptingHandoff/AdoptingHandoff.html#//apple_ref/doc/uid/TP40014338-CH2-SW10
    /// </summary>
    [Route("")]
    public class ExtensionlessFileDownloadController : Controller
    {
        private readonly IHostingEnvironment _env;

        public ExtensionlessFileDownloadController(IHostingEnvironment env)
        {
            _env = env;
        }

        [HttpGet("apple-app-site-association")]
        public ActionResult Index()
        {
            
            string filePath = Path.Combine(_env.WebRootPath, "apple-handoff-json");
            try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "application/pkcs7-mime");
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}