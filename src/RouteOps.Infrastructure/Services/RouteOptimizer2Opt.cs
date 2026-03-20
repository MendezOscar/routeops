using RouteOps.Application.Interfaces;

namespace RouteOps.Infrastructure.Services;

public class RouteOptimizer2Opt : IRouteOptimizer
{
    private static readonly (double Lat, double Lng) Warehouse = (19.4326, -99.1332);

    public IReadOnlyList<RouteStop> Optimize(IEnumerable<RouteStop> stops)
    {
        var pts = stops.ToList();
        if (pts.Count <= 1) return pts;

        var route = NearestNeighbor(pts);

        bool improved = true;
        while (improved)
        {
            improved = false;
            for (int i = 0; i < route.Count - 1; i++)
            {
                for (int j = i + 2; j < route.Count; j++)
                {
                    var newRoute = TwoOptSwap(route, i, j);
                    if (TotalDistance(newRoute) < TotalDistance(route) - 0.0001)
                    {
                        route    = newRoute;
                        improved = true;
                    }
                }
            }
        }
        return route;
    }

    private static List<RouteStop> NearestNeighbor(List<RouteStop> pts)
    {
        var unvisited = new List<RouteStop>(pts);
        var path      = new List<RouteStop>();
        var current   = Warehouse;

        while (unvisited.Count > 0)
        {
            var nearest = unvisited.MinBy(p => Dist(current.Lat, current.Lng, p.Lat, p.Lng))!;
            path.Add(nearest);
            current = (nearest.Lat, nearest.Lng);
            unvisited.Remove(nearest);
        }
        return path;
    }

    private static List<RouteStop> TwoOptSwap(List<RouteStop> route, int i, int j)
    {
        var result  = new List<RouteStop>(route[..(i + 1)]);
        var segment = route[(i + 1)..(j + 1)];
        segment.Reverse();
        result.AddRange(segment);
        result.AddRange(route[(j + 1)..]);
        return result;
    }

    private static double TotalDistance(List<RouteStop> pts)
    {
        if (pts.Count == 0) return 0;
        double d = Dist(Warehouse.Lat, Warehouse.Lng, pts[0].Lat, pts[0].Lng);
        for (int i = 1; i < pts.Count; i++)
            d += Dist(pts[i - 1].Lat, pts[i - 1].Lng, pts[i].Lat, pts[i].Lng);
        d += Dist(pts[^1].Lat, pts[^1].Lng, Warehouse.Lat, Warehouse.Lng);
        return d;
    }

    private static double Dist(double lat1, double lng1, double lat2, double lng2) =>
        Math.Sqrt(Math.Pow(lat1 - lat2, 2) + Math.Pow(lng1 - lng2, 2));
}
