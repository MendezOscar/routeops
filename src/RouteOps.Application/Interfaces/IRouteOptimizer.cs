// src/RouteOps.Application/Interfaces/IRouteOptimizer.cs
namespace RouteOps.Application.Interfaces;

public record RouteStop(Guid OrderId, double Lat, double Lng);

public interface IRouteOptimizer
{
    IReadOnlyList<RouteStop> Optimize(IEnumerable<RouteStop> stops);
}