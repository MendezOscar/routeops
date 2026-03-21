using Microsoft.EntityFrameworkCore;
using RouteOps.Application.Interfaces;
using RouteOps.Domain.Entities;

namespace RouteOps.Infrastructure.Persistence;

public class RouteOpsDbContext(DbContextOptions<RouteOpsDbContext> options)
    : DbContext(options), IRouteOpsDbContext
{
    public DbSet<Client>            Clients            => Set<Client>();
    public DbSet<Driver>            Drivers            => Set<Driver>();
    public DbSet<Product>           Products           => Set<Product>();
    public DbSet<Order>             Orders             => Set<Order>();
    public DbSet<OrderItem>         OrderItems         => Set<OrderItem>();
    public DbSet<Sale>              Sales              => Set<Sale>();
    public DbSet<Credit>            Credits            => Set<Credit>();
    public DbSet<CreditPayment>     CreditPayments     => Set<CreditPayment>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<PurchaseOrder>     PurchaseOrders     => Set<PurchaseOrder>();
    public DbSet<PurchaseItem>      PurchaseItems      => Set<PurchaseItem>();
    public DbSet<Notification>      Notifications      => Set<Notification>();
    public DbSet<User> Users => Set<User>();


    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(typeof(RouteOpsDbContext).Assembly);
        base.OnModelCreating(mb);
    }
}
