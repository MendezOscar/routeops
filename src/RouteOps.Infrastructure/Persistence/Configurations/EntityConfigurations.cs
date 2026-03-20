using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RouteOps.Domain.Entities;
using RouteOps.Domain.Enums;

namespace RouteOps.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> b)
    {
        b.ToTable("clients");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Phone).HasMaxLength(30);
        b.Property(x => x.Email).HasMaxLength(150);
        b.Property(x => x.CreditLimit).HasPrecision(12, 2);
        b.Property(x => x.Zone).HasMaxLength(80);
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
        b.Property(x => x.Status).HasConversion<string>();
        b.Property(x => x.Subtotal).HasPrecision(12, 2);
        b.Property(x => x.Iva).HasPrecision(12, 2);
        b.Property(x => x.Total).HasPrecision(12, 2);
        b.Property(x => x.WeightKg).HasPrecision(8, 3);
        b.Property(x => x.Lat).HasPrecision(10, 7);
        b.Property(x => x.Lng).HasPrecision(10, 7);
        b.HasMany(x => x.Items).WithOne().HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Sale).WithOne(s => s.Order)
            .HasForeignKey<Sale>(s => s.OrderId);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.ToTable("order_items");
        b.HasKey(x => x.Id);
        b.Property(x => x.UnitPrice).HasPrecision(12, 2);
        b.Property(x => x.WeightKg).HasPrecision(8, 3);
        b.Ignore(x => x.Subtotal);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("products");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Sku).HasMaxLength(60).IsRequired();
        b.Property(x => x.Price).HasPrecision(12, 2);
        b.Property(x => x.Cost).HasPrecision(12, 2);
        b.Property(x => x.WeightKg).HasPrecision(6, 3);
        b.HasIndex(x => x.Sku).IsUnique();
        b.Ignore(x => x.MarginPercent);
        b.Ignore(x => x.IsLowStock);
        b.Ignore(x => x.IsOutOfStock);
    }
}

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> b)
    {
        b.ToTable("sales");
        b.HasKey(x => x.Id);
        b.Property(x => x.PayMethod).HasConversion<string>();
        b.Property(x => x.Subtotal).HasPrecision(12, 2);
        b.Property(x => x.Iva).HasPrecision(12, 2);
        b.Property(x => x.Total).HasPrecision(12, 2);
    }
}

public class CreditConfiguration : IEntityTypeConfiguration<Credit>
{
    public void Configure(EntityTypeBuilder<Credit> b)
    {
        b.ToTable("credits");
        b.HasKey(x => x.Id);
        b.Property(x => x.Total).HasPrecision(12, 2);
        b.Property(x => x.Balance).HasPrecision(12, 2);
        b.Property(x => x.Status).HasConversion<string>();
        b.HasMany(x => x.Payments).WithOne().HasForeignKey(p => p.CreditId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.DueDate).HasFilter("balance > 0");
        b.Ignore(x => x.DaysUntilDue);
        b.Ignore(x => x.IsOverdue);
        b.Ignore(x => x.IsDueSoon);
    }
}

public class CreditPaymentConfiguration : IEntityTypeConfiguration<CreditPayment>
{
    public void Configure(EntityTypeBuilder<CreditPayment> b)
    {
        b.ToTable("credit_payments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Amount).HasPrecision(12, 2);
        b.Property(x => x.Method).HasMaxLength(40);
        b.Property(x => x.Reference).HasMaxLength(150);
    }
}

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> b)
    {
        b.ToTable("drivers");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.ColorHex).HasMaxLength(7);
    }
}

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> b)
    {
        b.ToTable("inventory_movements");
        b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasConversion<string>();
        b.Property(x => x.Reason).HasMaxLength(80);
        b.Property(x => x.Reference).HasMaxLength(80);
    }
}

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> b)
    {
        b.ToTable("purchase_orders");
        b.HasKey(x => x.Id);
        b.Property(x => x.Supplier).HasMaxLength(150).IsRequired();
        b.Property(x => x.Status).HasConversion<string>();
        b.Property(x => x.Total).HasPrecision(12, 2);
        b.HasMany(x => x.Items).WithOne().HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PurchaseItemConfiguration : IEntityTypeConfiguration<PurchaseItem>
{
    public void Configure(EntityTypeBuilder<PurchaseItem> b)
    {
        b.ToTable("purchase_items");
        b.HasKey(x => x.Id);
        b.Property(x => x.UnitCost).HasPrecision(12, 2);
        b.Ignore(x => x.Subtotal);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("notifications");
        b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasConversion<string>();
        b.Property(x => x.Channel).HasConversion<string>();
        b.HasIndex(x => x.ScheduledAt).HasFilter("sent = false");
    }
}
