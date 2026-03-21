using MediatR;
using Microsoft.EntityFrameworkCore;
using RouteOps.Application.Interfaces;
using RouteOps.Domain.Enums;

namespace RouteOps.Application.Queries.Dashboard;

public sealed class GetDashboardHandler(IRouteOpsDbContext db)
    : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    public async Task<DashboardDto> Handle(
        GetDashboardQuery request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var totalSales = await db.Sales
            .SumAsync(s => s.Total, ct);

        var activePortfolio = await db.Credits
            .Where(c => c.Balance > 0)
            .SumAsync(c => c.Balance, ct);

        var overdueCredits = await db.Credits
            .CountAsync(c => c.Balance > 0 && c.DueDate < today, ct);

        var activeClients = await db.Clients
            .CountAsync(c => c.Active, ct);

        var newOrders = await db.Orders
            .CountAsync(o => o.Status == OrderStatus.New, ct);

        var pendingOrders = await db.Orders
            .CountAsync(o => o.Status == OrderStatus.Pending, ct);

        var lowStock = await db.Products
            .Where(p => p.Active && p.Stock <= p.MinStock)
            .Select(p => new LowStockDto(p.Id, p.Name, p.Sku, p.Stock, p.MinStock))
            .ToListAsync(ct);

        return new DashboardDto(
            TotalSales:       totalSales,
            ActivePortfolio:  activePortfolio,
            OverdueCredits:   overdueCredits,
            ActiveClients:    activeClients,
            NewOrders:        newOrders,
            PendingOrders:    pendingOrders,
            LowStockProducts: lowStock
        );
    }
}
