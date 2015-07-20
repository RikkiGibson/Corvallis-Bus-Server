using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CorvallisTransit.Models
{
    /// <summary>
    /// Extensions for RoutePatterns
    /// </summary>
    public partial class RoutePattern
    {
        public RoutePatternContent Content
        {
            get { return Items.First() as RoutePatternContent; }
        }
        public RoutePatternProject Project
        {
            get { return Items.Skip(1).FirstOrDefault() as RoutePatternProject; }
        }
    }
}