using MediatR;
using Microsoft.EntityFrameworkCore;
using RouteOps.Application.Interfaces;

namespace RouteOps.Application.Queries.Orders;

public sealed class GetOrderByIdHandler(IRouteOpsDbContext db)
    : IRequestHandler<GetOrderByIdQuery, OrderDetailDto?>
{
    public async Task<OrderDetailDto?> Handle(
        GetOrderByIdQuery request, CancellationToken ct)
    {
        var o = await db.Orders
            .Include(o => o.Client)
            .Include(o => o.Driver)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

        if (o is null) return null;

        return new OrderDetailDto(
            Id:         o.Id.ToString(),
            ClientName: o.Client.Name,
            Address:    o.Address ?? "",
            Status:     o.Status.ToString(),
            Total:      o.Total,
            DriverName: o.Driver?.Name
        );
    }
}
