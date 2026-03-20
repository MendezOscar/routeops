// ─────────────────────────────────────────────────────────
// src/RouteOps.Application/Interfaces/IRouteOpsDbContext.cs
// ─────────────────────────────────────────────────────────
using Microsoft.EntityFrameworkCore;
using RouteOps.Domain.Entities;

namespace RouteOps.Application.Interfaces;

public interface IRouteOpsDbContext
{
    DbSet<Client>              Clients              { get; }
    DbSet<Driver>              Drivers              { get; }
    DbSet<Product>             Products             { get; }
    DbSet<Order>               Orders               { get; }
    DbSet<OrderItem>           OrderItems           { get; }
    DbSet<Sale>                Sales                { get; }
    DbSet<Credit>              Credits              { get; }
    DbSet<CreditPayment>       CreditPayments       { get; }
    DbSet<InventoryMovement>   InventoryMovements   { get; }
    DbSet<PurchaseOrder>       PurchaseOrders       { get; }
    DbSet<PurchaseItem>        PurchaseItems        { get; }
    DbSet<Notification>        Notifications        { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.Application/Interfaces/INotificationService.cs
// ─────────────────────────────────────────────────────────
using RouteOps.Domain.Enums;

namespace RouteOps.Application.Interfaces;

public interface INotificationService
{
    Task SendWhatsAppAsync(string phone, string message, CancellationToken ct = default);
    Task SendPushAsync(string clientToken, string title, string body, CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.Application/Interfaces/IRouteOptimizer.cs
// ─────────────────────────────────────────────────────────
namespace RouteOps.Application.Interfaces;

public record RouteStop(Guid OrderId, double Lat, double Lng);

public interface IRouteOptimizer
{
    /// Devuelve los stops en el orden óptimo (2-opt TSP)
    IReadOnlyList<RouteStop> Optimize(IEnumerable<RouteStop> stops);
}

// ═══════════════════════════════════════════════════════════
// COMMANDS
// ═══════════════════════════════════════════════════════════

// ─── Orders ────────────────────────────────────────────────

// src/RouteOps.Application/Commands/Orders/CreateOrderCommand.cs
using MediatR;

namespace RouteOps.Application.Commands.Orders;

public record CreateOrderItemDto(Guid ProductId, int Quantity);

public record CreateOrderCommand(
    Guid ClientId,
    string? Address,
    string? Zone,
    decimal? Lat,
    decimal? Lng,
    string? Notes,
    IReadOnlyList<CreateOrderItemDto> Items
) : IRequest<Guid>;

// src/RouteOps.Application/Commands/Orders/ApproveOrderCommand.cs
public record ApproveOrderCommand(
    Guid OrderId,
    Guid DriverId,
    PayMethod PayMethod    // si es Credit se crea el crédito
) : IRequest;

// src/RouteOps.Application/Commands/Orders/RejectOrderCommand.cs
public record RejectOrderCommand(Guid OrderId, string Reason) : IRequest;

// ─── Credits ───────────────────────────────────────────────

// src/RouteOps.Application/Commands/Credits/ApplyPaymentCommand.cs
public record ApplyPaymentCommand(
    Guid CreditId,
    decimal Amount,
    string Method,
    string? Reference,
    string? Notes
) : IRequest<Guid>;

// ─── Inventory ─────────────────────────────────────────────

// src/RouteOps.Application/Commands/Inventory/ReceivePurchaseOrderCommand.cs
public record ReceivePurchaseOrderCommand(Guid PurchaseOrderId) : IRequest;

// src/RouteOps.Application/Commands/Inventory/AdjustStockCommand.cs
public record AdjustStockCommand(
    Guid ProductId,
    int NewStock,
    string? Notes
) : IRequest;

// ═══════════════════════════════════════════════════════════
// QUERIES
// ═══════════════════════════════════════════════════════════

// src/RouteOps.Application/Queries/Credits/GetUpcomingDuesQuery.cs
using MediatR;

namespace RouteOps.Application.Queries.Credits;

public record UpcomingDueDto(
    Guid CreditId,
    Guid ClientId,
    string ClientName,
    string? ClientPhone,
    decimal Balance,
    DateOnly DueDate,
    int DaysUntilDue
);

public record GetUpcomingDuesQuery(int DaysAhead = 7) : IRequest<IReadOnlyList<UpcomingDueDto>>;

// src/RouteOps.Application/Queries/Dashboard/GetDashboardQuery.cs
public record DashboardDto(
    decimal TotalSales,
    decimal ActivePortfolio,
    int OverdueCredits,
    int ActiveClients,
    int NewOrders,
    int PendingOrders,
    IReadOnlyList<LowStockDto> LowStockProducts
);

public record LowStockDto(Guid ProductId, string Name, string Sku, int Stock, int MinStock);

public record GetDashboardQuery : IRequest<DashboardDto>;

// src/RouteOps.Application/Queries/Routes/GetRoutesByDriverQuery.cs
public record DriverRouteDto(
    Guid DriverId,
    string DriverName,
    string ColorHex,
    IReadOnlyList<RouteOrderDto> Orders
);

public record RouteOrderDto(
    Guid OrderId,
    string ClientName,
    string Address,
    double Lat,
    double Lng,
    int StopNumber   // 0 = sin optimizar
);

public record GetRoutesByDriverQuery(bool Optimize = false) : IRequest<IReadOnlyList<DriverRouteDto>>;
