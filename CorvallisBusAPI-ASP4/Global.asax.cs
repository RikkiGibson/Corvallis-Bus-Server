using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace CorvallisBusAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var jsonFormatter = new JsonMediaTypeFormatter();
            GlobalConfiguration.Configuration.Services.Replace(typeof(IContentNegotiator), new JsonContentNegotiator(jsonFormatter));
        }
    }

    /// <summary>
    /// Shamelessly c/ped from stackoverflow.
    /// </summary>
    public class JsonContentNegotiator : IContentNegotiator
    {
        private readonly JsonMediaTypeFormatter _jsonFormatter;

        public JsonContentNegotiator(JsonMediaTypeFormatter formatter)
        {
            _jsonFormatter = formatter;
        }

        public ContentNegotiationResult Negotiate(
                Type type,
                HttpRequestMessage request,
                IEnumerable<MediaTypeFormatter> formatters)
        {
            return new ContentNegotiationResult(
                _jsonFormatter,
                new MediaTypeHeaderValue("application/json"));
        }
    }
}
