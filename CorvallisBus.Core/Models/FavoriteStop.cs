using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorvallisBus.Core.Models
{
    public record FavoriteStop(
        int Id,
        string Name,
        List<string> RouteNames,
        double Lat,
        double Long,
        double DistanceFromUser,
        bool IsNearestStop
        );
}
