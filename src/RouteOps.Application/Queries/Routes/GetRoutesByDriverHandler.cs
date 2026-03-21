using System.Linq;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RouteOps.Application.Interfaces;
using RouteOps.Domain.Enums;

namespace RouteOps.Application.Queries.Routes;

public sealed class GetRoutesByDriverHandler(
    IRouteOpsDbContext db,
    IRouteOptimizer optimizer)
    : IRequestHandler<GetRoutesByDriverQuery, IReadOnlyList<DriverRouteDto>>
{
    public async Task<IReadOnlyList<DriverRouteDto>> Handle(
        GetRoutesByDriverQuery request, CancellationToken ct)
    {
        var drivers = await db.Drivers
            .Where(d => d.Active)
            .ToListAsync(ct);

        var activeOrders = await db.Orders
            .Include(o => o.Client)
            .Where(o => o.DriverId != null &&
                        (o.Status == OrderStatus.Approved ||
                         o.Status == OrderStatus.EnRoute))
            .ToListAsync(ct);

        var result = new List<DriverRouteDto>();

        foreach (var driver in drivers)
        {
            var driverOrders = activeOrders
                .Where(o => o.DriverId == driver.Id)
                .ToList();

            var stops = driverOrders
                .Where(o => o.Lat.HasValue && o.Lng.HasValue)
                .Select(o => new RouteStop(
                    o.Id,
                    (double)o.Lat!.Value,
                    (double)o.Lng!.Value))
                .ToList();

            var ordered = request.Optimize && stops.Count > 1
                ? optimizer.Optimize(stops).ToList()
                : stops;

            var orderedIds = ordered.Select(s => s.OrderId).ToList(); // List<Guid>

            var routeOrders = orderedIds.Select((orderId, idx) =>
            {
                var order = driverOrders.First(o => o.Id == orderId); // Guid == Guid ✓
                return new RouteOrderDto(
                    OrderId: order.Id.ToString(),
                    ClientName: order.Client.Name,
                    Address: order.Address ?? order.Client.Address ?? "",
                    Lat: (double)(order.Lat ?? 0),
                    Lng: (double)(order.Lng ?? 0),
                    StopNumber: idx + 1
                );
            }).ToList();

            result.Add(new DriverRouteDto(
                DriverId: driver.Id.ToString(),
                DriverName: driver.Name,
                ColorHex: driver.ColorHex,
                Orders: routeOrders
            ));
        }

        return result;
    }
}
