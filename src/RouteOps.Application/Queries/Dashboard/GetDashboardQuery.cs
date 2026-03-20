using MediatR;

namespace RouteOps.Application.Queries.Dashboard;

public record LowStockDto(Guid ProductId, string Name, string Sku, int Stock, int MinStock);

public record DashboardDto(
    decimal TotalSales,
    decimal ActivePortfolio,
    int OverdueCredits,
    int ActiveClients,
    int NewOrders,
    int PendingOrders,
    IReadOnlyList<LowStockDto> LowStockProducts
);

public record GetDashboardQuery : IRequest<DashboardDto>;
