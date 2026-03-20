// src/RouteOps.Infrastructure/Persistence/RouteOpsDbContext.cs
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

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(typeof(RouteOpsDbContext).Assembly);
        base.OnModelCreating(mb);
    }
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.Infrastructure/Persistence/Configurations/ClientConfiguration.cs
// ─────────────────────────────────────────────────────────
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RouteOps.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> b)
    {
        b.ToTable("clients");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        b.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(30);
        b.Property(x => x.Email).HasColumnName("email").HasMaxLength(150);
        b.Property(x => x.Address).HasColumnName("address");
        b.Property(x => x.Zone).HasColumnName("zone").HasMaxLength(80);
        b.Property(x => x.CreditLimit).HasColumnName("credit_limit").HasPrecision(12, 2);
        b.Property(x => x.CreditDays).HasColumnName("credit_days");
        b.Property(x => x.Active).HasColumnName("active");
        b.Property(x => x.Notes).HasColumnName("notes");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        b.HasMany(x => x.Orders).WithOne(o => o.Client).HasForeignKey(o => o.ClientId);
        b.HasMany(x => x.Credits).WithOne(c => c.Client).HasForeignKey(c => c.ClientId);
    }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("orders");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasColumnName("status");
        b.Property(x => x.Subtotal).HasColumnName("subtotal").HasPrecision(12, 2);
        b.Property(x => x.Iva).HasColumnName("iva").HasPrecision(12, 2);
        b.Property(x => x.Total).HasColumnName("total").HasPrecision(12, 2);
        b.Property(x => x.WeightKg).HasColumnName("weight_kg").HasPrecision(8, 3);
        b.Property(x => x.Lat).HasColumnName("lat").HasPrecision(10, 7);
        b.Property(x => x.Lng).HasColumnName("lng").HasPrecision(10, 7);
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        b.HasMany(x => x.Items).WithOne().HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Sale).WithOne(s => s.Order).HasForeignKey<Sale>(s => s.OrderId);
    }
}

public class CreditConfiguration : IEntityTypeConfiguration<Credit>
{
    public void Configure(EntityTypeBuilder<Credit> b)
    {
        b.ToTable("credits");
        b.HasKey(x => x.Id);
        b.Property(x => x.Total).HasColumnName("total").HasPrecision(12, 2);
        b.Property(x => x.Balance).HasColumnName("balance").HasPrecision(12, 2);
        b.Property(x => x.DueDate).HasColumnName("due_date");
        b.Property(x => x.Status).HasConversion<string>().HasColumnName("status");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        b.HasMany(x => x.Payments).WithOne().HasForeignKey(p => p.CreditId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice para job nocturno
        b.HasIndex(x => x.DueDate).HasFilter("balance > 0");
    }
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.Infrastructure/Services/RouteOptimizer2Opt.cs
// Algoritmo 2-opt portado desde el prototipo JS
// ─────────────────────────────────────────────────────────
using RouteOps.Application.Interfaces;

namespace RouteOps.Infrastructure.Services;

public class RouteOptimizer2Opt : IRouteOptimizer
{
    private static readonly (double Lat, double Lng) Warehouse = (19.4326, -99.1332);

    public IReadOnlyList<RouteStop> Optimize(IEnumerable<RouteStop> stops)
    {
        var pts = stops.ToList();
        if (pts.Count <= 1) return pts;

        // Nearest Neighbor para ruta inicial
        var route = NearestNeighbor(pts);

        // 2-opt
        bool improved = true;
        while (improved)
        {
            improved = false;
            for (int i = 0; i < route.Count - 1; i++)
            {
                for (int j = i + 2; j < route.Count; j++)
                {
                    var newRoute = TwoOptSwap(route, i, j);
                    if (TotalDistance(newRoute) < TotalDistance(route) - 0.0001)
                    {
                        route = newRoute;
                        improved = true;
                    }
                }
            }
        }

        return route;
    }

    private static List<RouteStop> NearestNeighbor(List<RouteStop> pts)
    {
        var unvisited = new List<RouteStop>(pts);
        var path = new List<RouteStop>();
        var current = Warehouse;

        while (unvisited.Count > 0)
        {
            var nearest = unvisited.MinBy(p => Dist(current.Lat, current.Lng, p.Lat, p.Lng))!;
            path.Add(nearest);
            current = (nearest.Lat, nearest.Lng);
            unvisited.Remove(nearest);
        }
        return path;
    }

    private static List<RouteStop> TwoOptSwap(List<RouteStop> route, int i, int j)
    {
        var result = new List<RouteStop>(route[..( i + 1)]);
        var segment = route[(i + 1)..(j + 1)];
        segment.Reverse();
        result.AddRange(segment);
        result.AddRange(route[(j + 1)..]);
        return result;
    }

    private double TotalDistance(List<RouteStop> pts)
    {
        if (pts.Count == 0) return 0;
        double d = Dist(Warehouse.Lat, Warehouse.Lng, pts[0].Lat, pts[0].Lng);
        for (int i = 1; i < pts.Count; i++)
            d += Dist(pts[i - 1].Lat, pts[i - 1].Lng, pts[i].Lat, pts[i].Lng);
        d += Dist(pts[^1].Lat, pts[^1].Lng, Warehouse.Lat, Warehouse.Lng);
        return d;
    }

    private static double Dist(double lat1, double lng1, double lat2, double lng2) =>
        Math.Sqrt(Math.Pow(lat1 - lat2, 2) + Math.Pow(lng1 - lng2, 2));
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.Infrastructure/Services/CreditCheckJob.cs
// Job nocturno con Hangfire — verifica vencimientos
// ─────────────────────────────────────────────────────────
using Hangfire;
using Microsoft.EntityFrameworkCore;
using RouteOps.Application.Interfaces;
using RouteOps.Domain.Entities;
using RouteOps.Domain.Enums;

namespace RouteOps.Infrastructure.Services;

public class CreditCheckJob(IRouteOpsDbContext db, INotificationService notifService)
{
    [AutomaticRetry(Attempts = 2)]
    public async Task RunAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var credits = await db.Credits
            .Include(c => c.Client)
            .Where(c => c.Balance > 0)
            .ToListAsync();

        foreach (var credit in credits)
        {
            // Marcar vencidos
            credit.MarkOverdueIfApplicable();

            // Notificar si vence en 1, 3 o 7 días
            var daysLeft = credit.DaysUntilDue;
            if (daysLeft is 1 or 3 or 7)
            {
                var msg = daysLeft == 0
                    ? $"⚠️ Hola {credit.Client.Name.Split(' ')[0]}, tu crédito {credit.Id.ToString()[..8].ToUpper()} vence HOY. Saldo: ${credit.Balance:N2}. Contáctanos para coordinar tu pago."
                    : $"📅 Hola {credit.Client.Name.Split(' ')[0]}, tu crédito vence en {daysLeft} día(s). Saldo: ${credit.Balance:N2}. Vencimiento: {credit.DueDate}.";

                if (credit.Client.Phone is not null)
                {
                    await notifService.SendWhatsAppAsync(credit.Client.Phone, msg);
                }

                db.Notifications.Add(Notification.Create(
                    type: daysLeft <= 0 ? NotifType.CreditOverdue : NotifType.CreditDueSoon,
                    channel: NotifChannel.WhatsApp,
                    message: msg,
                    clientId: credit.ClientId,
                    creditId: credit.Id
                ));
            }
        }

        await db.SaveChangesAsync();
    }
}
