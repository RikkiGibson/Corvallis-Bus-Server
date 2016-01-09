using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace CorvallisBusWeb.Controllers
{
    /// <summary>
    /// Workaround for the Handoff requirement to deliver an extensionless file.
    /// https://developer.apple.com/library/ios/documentation/UserExperience/Conceptual/Handoff/AdoptingHandoff/AdoptingHandoff.html#//apple_ref/doc/uid/TP40014338-CH2-SW10
    /// </summary>
    [RoutePrefix("")]
    public class ExtensionlessFileDownloadController : Controller
    {
        // GET: ExtensionlessFileDownload
        [Route("apple-app-site-association")]
        public ActionResult Index()
        {
            string filePath = HostingEnvironment.MapPath("~/apple-handoff-json");
            try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "application/pkcs7-mime");
            }
            catch
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }
    }
}