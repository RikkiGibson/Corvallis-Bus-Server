using CorvallisTransit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CorvallisTransit.Components
{
    public class TransitComparer : IEqualityComparer<BusRouteStop>
    {
        private static TransitComparer staticEntity;

        public static TransitComparer Comparer { get { return staticEntity; } }

        static TransitComparer()
        {
            staticEntity = new TransitComparer();
        }

        public bool Equals(BusRouteStop x, BusRouteStop y)
        {
            return x.StopModel.Name.Equals(y.StopModel.Name);
        }

        public int GetHashCode(BusRouteStop obj)
        {
            return obj.StopModel.Name.GetHashCode();
        }
    }
}