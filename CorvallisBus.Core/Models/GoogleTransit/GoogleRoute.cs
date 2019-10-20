using CsvHelper.Configuration.Attributes;
using System.Collections.Generic;

namespace CorvallisBus.Core.Models.GoogleTransit
{
    /// <summary>
    /// Representation of a CTS Route from Google Transit data.
    /// </summary>
    public class GoogleRoute
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string RouteNo { get; }

        public string Name { get; }

        public string Color { get; }

        public string Url { get; }

        public List<LatLong> Shape { get; }

        public List<int> Path { get; }

        public GoogleRoute(
            string routeNo,
            string name,
            string color,
            string url,
            List<LatLong> shape,
            List<int> path)
        {
            RouteNo = routeNo;
            Name = name;
            Color = color;
            Url = url;
            Shape = shape;
            Path = path;
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}