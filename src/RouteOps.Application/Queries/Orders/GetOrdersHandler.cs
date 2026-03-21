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
        var query = db.Orders
            .Include(o => o.Client)
            .Include(o => o.Driver)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .AsQueryable();

        if (request.StatusFilter is not null &&
            Enum.TryParse<OrderStatus>(request.StatusFilter, true, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        return orders.Select(o => new OrderSummaryDto(
            Id:             o.Id.ToString(),
            ClientId:       o.ClientId.ToString(),
            ClientName:     o.Client.Name,
            Address:        o.Address ?? o.Client.Address ?? "",
            Zone:           o.Zone    ?? o.Client.Zone    ?? "",
            Status:         o.Status.ToString(),
            Total:          o.Total,
            WeightKg:       o.WeightKg,
            DriverName:     o.Driver?.Name,
            Phone:          o.Client.Phone,
            Notes:          o.Notes,
            RejectedReason: o.RejectedReason,
            CreatedAt:      o.CreatedAt,
            Items:          o.Items.Select(i => new OrderItemDto(
                ProductId:   i.ProductId.ToString(),
                ProductName: i.Product.Name,
                ProductIcon: i.Product.Icon,
                Quantity:    i.Quantity,
                UnitPrice:   i.UnitPrice
            )).ToList()
        )).ToList();
    }
}
