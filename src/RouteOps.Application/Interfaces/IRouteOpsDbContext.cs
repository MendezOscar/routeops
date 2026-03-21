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
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}