using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CorvallisTransit.Models
{
    /// <summary>
    /// Extensions for generated RoutePositions
    /// </summary>
    public partial class RoutePosition
    {
        public RoutePositionContent ContentInfo
        {
            get
            {
                return Items.First() as RoutePositionContent;
            }
        }
    }
}