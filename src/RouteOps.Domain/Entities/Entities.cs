// src/RouteOps.Domain/Entities/Product.cs
namespace RouteOps.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? CategoryId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Sku { get; private set; } = default!;
    public decimal Price { get; private set; }
    public decimal Cost { get; private set; }
    public decimal WeightKg { get; private set; }
    public int Stock { get; private set; }
    public int MinStock { get; private set; }
    public string Icon { get; private set; } = "📦";
    public bool Active { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public decimal MarginPercent =>
        Price > 0 ? Math.Round((Price - Cost) / Price * 100, 1) : 0;

    public bool IsLowStock => Stock > 0 && Stock <= MinStock;
    public bool IsOutOfStock => Stock == 0;

    private Product() { }

    public static Product Create(string name, string sku, decimal price,
        decimal cost, decimal weightKg, int minStock, string? icon = null,
        Guid? categoryId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        if (cost < 0)  throw new ArgumentOutOfRangeException(nameof(cost));

        return new Product
        {
            Name = name.Trim(),
            Sku = sku.Trim().ToUpperInvariant(),
            Price = price,
            Cost = cost,
            WeightKg = weightKg,
            MinStock = minStock,
            Icon = icon ?? "📦",
            CategoryId = categoryId,
        };
    }

    public void AdjustStock(int delta)
    {
        var newStock = Stock + delta;
        if (newStock < 0)
            throw new InvalidOperationException(
                $"Stock insuficiente para '{Name}'. Disponible: {Stock}, requerido: {Math.Abs(delta)}.");
        Stock = newStock;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStock(int quantity)
    {
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        Stock = quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, decimal price, decimal cost,
        decimal weightKg, int minStock, string? icon)
    {
        Name = name.Trim();
        Price = price;
        Cost = cost;
        WeightKg = weightKg;
        MinStock = minStock;
        Icon = icon ?? Icon;
        UpdatedAt = DateTime.UtcNow;
    }
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.Domain/Entities/Driver.cs
// ─────────────────────────────────────────────────────────
namespace RouteOps.Domain.Entities;

public class Driver
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = default!;
    public string? Phone { get; private set; }
    public string ColorHex { get; private set; } = "#185FA5";
    public bool Active { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; private set; } = [];

    private Driver() { }

    public static Driver Create(string name, string? phone, string? colorHex = null) =>
        new() { Name = name.Trim(), Phone = phone?.Trim(), ColorHex = colorHex ?? "#185FA5" };
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.Domain/Entities/Sale.cs
// ─────────────────────────────────────────────────────────
using RouteOps.Domain.Enums;

namespace RouteOps.Domain.Entities;

public class Sale
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public Guid ClientId { get; private set; }
    public PayMethod PayMethod { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal Iva { get; private set; }
    public decimal Total { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public Order Order { get; private set; } = default!;
    public Client Client { get; private set; } = default!;
    public Credit? Credit { get; private set; }

    private Sale() { }

    public static Sale Create(Order order, PayMethod payMethod, string? notes = null)
    {
        if (order.Status != OrderStatus.Approved)
            throw new InvalidOperationException("Solo se puede generar una venta de un pedido aprobado.");

        return new Sale
        {
            OrderId = order.Id,
            ClientId = order.ClientId,
            PayMethod = payMethod,
            Subtotal = order.Subtotal,
            Iva = order.Iva,
            Total = order.Total,
            Notes = notes,
        };
    }
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.Domain/Entities/Credit.cs
// ─────────────────────────────────────────────────────────
using RouteOps.Domain.Enums;

namespace RouteOps.Domain.Entities;

public class Credit
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid SaleId { get; private set; }
    public Guid ClientId { get; private set; }
    public decimal Total { get; private set; }
    public decimal Balance { get; private set; }
    public DateOnly DueDate { get; private set; }
    public CreditStatus Status { get; private set; } = CreditStatus.Pending;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public Sale Sale { get; private set; } = default!;
    public Client Client { get; private set; } = default!;
    public ICollection<CreditPayment> Payments { get; private set; } = [];

    private Credit() { }

    public static Credit Create(Sale sale, int creditDays)
    {
        if (sale.PayMethod != PayMethod.Credit)
            throw new InvalidOperationException("Solo ventas a crédito generan un registro de crédito.");

        return new Credit
        {
            SaleId = sale.Id,
            ClientId = sale.ClientId,
            Total = sale.Total,
            Balance = sale.Total,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(creditDays)),
        };
    }

    /// Aplica un abono y actualiza balance y status.
    public CreditPayment ApplyPayment(decimal amount, string method,
        string? reference, string? notes)
    {
        if (Balance <= 0)
            throw new InvalidOperationException("Este crédito ya está liquidado.");
        if (amount <= 0 || amount > Balance)
            throw new ArgumentOutOfRangeException(nameof(amount),
                $"El monto debe ser mayor a 0 y menor o igual al saldo ({Balance:C}).");

        var payment = CreditPayment.Create(Id, amount, method, reference, notes);
        Payments.Add(payment);

        Balance = Math.Max(0, Balance - amount);
        Status = Balance == 0
            ? CreditStatus.Paid
            : CreditStatus.Partial;
        UpdatedAt = DateTime.UtcNow;

        return payment;
    }

    /// Marcar como vencido (llamado por el job nocturno)
    public void MarkOverdueIfApplicable()
    {
        if (Balance > 0 && DueDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            Status = CreditStatus.Overdue;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public int DaysUntilDue =>
        DueDate.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;

    public bool IsOverdue => Balance > 0 && DaysUntilDue < 0;
    public bool IsDueSoon  => Balance > 0 && DaysUntilDue is >= 0 and <= 7;
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.Domain/Entities/CreditPayment.cs
// ─────────────────────────────────────────────────────────
namespace RouteOps.Domain.Entities;

public class CreditPayment
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CreditId { get; private set; }
    public decimal Amount { get; private set; }
    public string Method { get; private set; } = default!;
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }
    public DateTime PaidAt { get; private set; } = DateTime.UtcNow;

    private CreditPayment() { }

    internal static CreditPayment Create(Guid creditId, decimal amount,
        string method, string? reference, string? notes) =>
        new()
        {
            CreditId = creditId,
            Amount = amount,
            Method = method,
            Reference = reference,
            Notes = notes,
        };
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.Domain/Entities/Notification.cs
// ─────────────────────────────────────────────────────────
using RouteOps.Domain.Enums;

namespace RouteOps.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? ClientId { get; private set; }
    public Guid? CreditId { get; private set; }
    public Guid? OrderId { get; private set; }
    public NotifType Type { get; private set; }
    public NotifChannel Channel { get; private set; }
    public string Message { get; private set; } = default!;
    public bool Sent { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? Error { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Notification() { }

    public static Notification Create(NotifType type, NotifChannel channel,
        string message, DateTime? scheduledAt = null,
        Guid? clientId = null, Guid? creditId = null, Guid? orderId = null) =>
        new()
        {
            Type = type,
            Channel = channel,
            Message = message,
            ScheduledAt = scheduledAt ?? DateTime.UtcNow,
            ClientId = clientId,
            CreditId = creditId,
            OrderId = orderId,
        };

    public void MarkSent()
    {
        Sent = true;
        SentAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Error = error;
        SentAt = DateTime.UtcNow;
    }
}
