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

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> b)
    {
        b.ToTable("drivers");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        b.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(30);
        b.Property(x => x.ColorHex).HasColumnName("color_hex").HasMaxLength(7);
        b.Property(x => x.Active).HasColumnName("active");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("orders");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ClientId).HasColumnName("client_id");
        b.Property(x => x.DriverId).HasColumnName("driver_id");
        b.Property(x => x.Status)
            .HasConversion(v => v.ToString().ToLower(),
                           v => Enum.Parse<OrderStatus>(v, true))
            .HasColumnName("status"); b.Property(x => x.Subtotal).HasColumnName("subtotal").HasPrecision(12, 2);
        b.Property(x => x.Iva).HasColumnName("iva").HasPrecision(12, 2);
        b.Property(x => x.Total).HasColumnName("total").HasPrecision(12, 2);
        b.Property(x => x.WeightKg).HasColumnName("weight_kg").HasPrecision(8, 3);
        b.Property(x => x.Address).HasColumnName("address");
        b.Property(x => x.Zone).HasColumnName("zone").HasMaxLength(80);
        b.Property(x => x.Lat).HasColumnName("lat").HasPrecision(10, 7);
        b.Property(x => x.Lng).HasColumnName("lng").HasPrecision(10, 7);
        b.Property(x => x.Notes).HasColumnName("notes");
        b.Property(x => x.RejectedReason).HasColumnName("rejected_reason");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
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
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.OrderId).HasColumnName("order_id");
        b.Property(x => x.ProductId).HasColumnName("product_id");
        b.Property(x => x.Quantity).HasColumnName("quantity");
        b.Property(x => x.UnitPrice).HasColumnName("unit_price").HasPrecision(12, 2);
        b.Property(x => x.WeightKg).HasColumnName("weight_kg").HasPrecision(8, 3);
        b.Ignore(x => x.Subtotal);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("products");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.CategoryId).HasColumnName("category_id");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        b.Property(x => x.Sku).HasColumnName("sku").HasMaxLength(60).IsRequired();
        b.Property(x => x.Price).HasColumnName("price").HasPrecision(12, 2);
        b.Property(x => x.Cost).HasColumnName("cost").HasPrecision(12, 2);
        b.Property(x => x.WeightKg).HasColumnName("weight_kg").HasPrecision(6, 3);
        b.Property(x => x.Stock).HasColumnName("stock");
        b.Property(x => x.MinStock).HasColumnName("min_stock");
        b.Property(x => x.Icon).HasColumnName("icon");
        b.Property(x => x.Active).HasColumnName("active");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
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
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.OrderId).HasColumnName("order_id");
        b.Property(x => x.ClientId).HasColumnName("client_id");
        b.Property(x => x.PayMethod)
            .HasConversion(v => v.ToString().ToLower(),
                        v => Enum.Parse<PayMethod>(v, true))
            .HasColumnName("pay_method");
        b.Property(x => x.Subtotal).HasColumnName("subtotal").HasPrecision(12, 2);
        b.Property(x => x.Iva).HasColumnName("iva").HasPrecision(12, 2);
        b.Property(x => x.Total).HasColumnName("total").HasPrecision(12, 2);
        b.Property(x => x.Notes).HasColumnName("notes");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}

public class CreditConfiguration : IEntityTypeConfiguration<Credit>
{
    public void Configure(EntityTypeBuilder<Credit> b)
    {
        b.ToTable("credits");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.SaleId).HasColumnName("sale_id");
        b.Property(x => x.ClientId).HasColumnName("client_id");
        b.Property(x => x.Total).HasColumnName("total").HasPrecision(12, 2);
        b.Property(x => x.Balance).HasColumnName("balance").HasPrecision(12, 2);
        b.Property(x => x.DueDate).HasColumnName("due_date");
        b.Property(x => x.Status)
            .HasConversion(v => v.ToString().ToLower(),
                        v => Enum.Parse<CreditStatus>(v, true))
            .HasColumnName("status");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        b.HasMany(x => x.Payments).WithOne().HasForeignKey(p => p.CreditId)
            .OnDelete(DeleteBehavior.Cascade);
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
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.CreditId).HasColumnName("credit_id");
        b.Property(x => x.Amount).HasColumnName("amount").HasPrecision(12, 2);
        b.Property(x => x.Method).HasColumnName("method").HasMaxLength(40);
        b.Property(x => x.Reference).HasColumnName("reference").HasMaxLength(150);
        b.Property(x => x.Notes).HasColumnName("notes");
        b.Property(x => x.PaidAt).HasColumnName("paid_at");
    }
}

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> b)
    {
        b.ToTable("inventory_movements");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ProductId).HasColumnName("product_id");
        b.Property(x => x.Type)
            .HasConversion(v => v.ToString().ToLower(),
                        v => Enum.Parse<MovementType>(v, true))
            .HasColumnName("type");
        b.Property(x => x.Quantity).HasColumnName("quantity");
        b.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(80);
        b.Property(x => x.Reference).HasColumnName("reference").HasMaxLength(80);
        b.Property(x => x.Notes).HasColumnName("notes");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> b)
    {
        b.ToTable("purchase_orders");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Supplier).HasColumnName("supplier").HasMaxLength(150).IsRequired();
        b.Property(x => x.Status)
            .HasConversion(v => v.ToString().ToLower(),
                        v => Enum.Parse<PurchaseOrderStatus>(v, true))
            .HasColumnName("status");
        b.Property(x => x.Total).HasColumnName("total").HasPrecision(12, 2);
        b.Property(x => x.Notes).HasColumnName("notes");
        b.Property(x => x.ExpectedAt).HasColumnName("expected_at");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
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
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.PurchaseOrderId).HasColumnName("purchase_order_id");
        b.Property(x => x.ProductId).HasColumnName("product_id");
        b.Property(x => x.Quantity).HasColumnName("quantity");
        b.Property(x => x.UnitCost).HasColumnName("unit_cost").HasPrecision(12, 2);
        b.Property(x => x.ReceivedQty).HasColumnName("received_qty");
        b.Ignore(x => x.Subtotal);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("notifications");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ClientId).HasColumnName("client_id");
        b.Property(x => x.CreditId).HasColumnName("credit_id");
        b.Property(x => x.OrderId).HasColumnName("order_id");
        b.Property(x => x.Type)
            .HasConversion(v => v.ToString().ToLower(),
                        v => Enum.Parse<NotifType>(v, true))
            .HasColumnName("type");
        b.Property(x => x.Channel)
            .HasConversion(v => v.ToString().ToLower(),
                        v => Enum.Parse<NotifChannel>(v, true))
            .HasColumnName("channel");
        b.Property(x => x.Message).HasColumnName("message");
        b.Property(x => x.Sent).HasColumnName("sent");
        b.Property(x => x.ScheduledAt).HasColumnName("scheduled_at");
        b.Property(x => x.SentAt).HasColumnName("sent_at");
        b.Property(x => x.Error).HasColumnName("error");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        b.Property(x => x.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
        b.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
        b.Property(x => x.Role).HasColumnName("role").HasMaxLength(40);
        b.Property(x => x.Active).HasColumnName("active");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        b.HasIndex(x => x.Email).IsUnique();
    }
}
