using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CorvallisBus.Core.Models;
using HtmlAgilityPack;

namespace CorvallisBus.Core.WebClients
{
    public static class ServiceAlertsClient
    {
        static readonly Uri FEED_URL = new Uri("https://www.corvallisoregon.gov/news?field_microsite_tid=581");
        static readonly HttpClient httpClient = new HttpClient();
        public static async Task<List<ServiceAlert>> GetServiceAlerts()
        {
            var responseStream = await httpClient.GetStreamAsync(FEED_URL);
            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(responseStream);

            var alerts = htmlDocument.DocumentNode.SelectNodes("//tbody/tr")
                .Select(row => ParseRow(row))
                .ToList();

            alerts.Insert(0, new ServiceAlert(
                title: "App Update for Upcoming CTS Schedule",
                publishDate: "2019-09-09T00:00:00-07:00",
                link: "https://rikkigibson.github.io/corvallisbus"));

            return alerts;
        }

        private static ServiceAlert ParseRow(HtmlNode row)
        {
            var anchor = row.Descendants("a").First();
            var relativeLink = anchor.Attributes["href"].Value;
            var link = new Uri(FEED_URL, relativeLink).ToString();
            var title = anchor.InnerText;

            var publishDate = row.Descendants("span")
                .First(node => node.HasClass("date-display-single"))
                .Attributes["content"]
                .Value;

            return new ServiceAlert(title, publishDate, link);
        }
    }
}
