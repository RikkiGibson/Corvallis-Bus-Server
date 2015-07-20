using CorvallisTransit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CorvallisTransit.Components
{
    public class TransitComparers : IComparer<BusRouteStop>, IEqualityComparer<BusRouteStop>
    {
        private static TransitComparers staticEntity;

        /// <summary>
        /// Gets the bus route stop comparer.
        /// </summary>
        /// <value>
        /// The bus route stop comparer.
        /// </value>
        public static TransitComparers TransitComparer { get { return staticEntity; } }

        static TransitComparers()
        {
            staticEntity = new TransitComparers();
        }

        public int Compare(BusRouteStop x, BusRouteStop y)
        {
            if (x.Eta != y.Eta)
            {
                return x.Eta.CompareTo(y.Eta);
            }
            else
            {
                return x.StopPosition.CompareTo(y.StopPosition);
            }
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