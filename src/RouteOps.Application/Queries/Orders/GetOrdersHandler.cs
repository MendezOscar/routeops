using MediatR;
using Microsoft.EntityFrameworkCore;
using RouteOps.Application.Interfaces;
using RouteOps.Domain.Enums;

namespace RouteOps.Application.Queries.Orders;

public sealed class GetOrdersHandler(IRouteOpsDbContext db)
    : IRequestHandler<GetOrdersQuery, IReadOnlyList<OrderSummaryDto>>
{
    public async Task<IReadOnlyList<OrderSummaryDto>> Handle(
     GetOrdersQuery request, CancellationToken ct)
    {
        var ordersQuery = db.Orders.AsQueryable();

        if (request.StatusFilter is not null &&
            Enum.TryParse<OrderStatus>(request.StatusFilter, true, out var status))
        {
            ordersQuery = ordersQuery.Where(o => o.Status == status);
        }

        // Sin Include — solo IDs y campos simples
        var orders = await ordersQuery
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new
            {
                o.Id,
                o.ClientId,
                o.DriverId,
                o.Status,
                o.Total,
                o.Address,
                o.Zone,
                o.Notes,
                o.RejectedReason,
                o.CreatedAt,
                o.WeightKg
            })
            .ToListAsync(ct);

        if (!orders.Any()) return new List<OrderSummaryDto>();

        // Cargar entidades relacionadas por separado
        var clientIds = orders.Select(o => o.ClientId).Distinct().ToList();
        var driverIds = orders.Select(o => o.DriverId).Where(d => d != null)
            .Select(d => d!.Value).Distinct().ToList();
        var orderIds = orders.Select(o => o.Id).ToList();

        var clients = await db.Clients
            .Where(c => clientIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, ct);

        var drivers = await db.Drivers
            .Where(d => driverIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, ct);

        var items = await db.OrderItems
            .Where(i => orderIds.Contains(i.OrderId))
            .ToListAsync(ct);

        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        return orders.Select(o => new OrderSummaryDto(
            Id: o.Id.ToString(),
            ClientId: o.ClientId.ToString(),
            ClientName: clients.TryGetValue(o.ClientId, out var c) ? c.Name : "",
            Address: o.Address ?? (c?.Address ?? ""),
            Zone: o.Zone ?? (c?.Zone ?? ""),
            Status: o.Status.ToString(),
            Total: o.Total,
            DriverName: o.DriverId.HasValue && drivers.TryGetValue(o.DriverId.Value, out var d) ? d.Name : null,
            Phone: c?.Phone,
            Notes: o.Notes,
            RejectedReason: o.RejectedReason,
            CreatedAt: o.CreatedAt,
            WeightKg: o.WeightKg,
            Items: items.Where(i => i.OrderId == o.Id).Select(i =>
            {
                products.TryGetValue(i.ProductId, out var p);
                return new OrderItemDto(
                    ProductId: i.ProductId.ToString(),
                    ProductName: p?.Name ?? "",
                    ProductIcon: p?.Icon ?? "📦",
                    Quantity: i.Quantity,
                    UnitPrice: i.UnitPrice
                );
            }).ToList()
        )).ToList();
    }
}
