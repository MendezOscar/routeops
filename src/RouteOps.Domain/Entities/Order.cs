// src/RouteOps.Domain/Entities/Order.cs
using RouteOps.Domain.Enums;

namespace RouteOps.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ClientId { get; private set; }
    public Guid? DriverId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.New;
    public decimal Subtotal { get; private set; }
    public decimal Iva { get; private set; }
    public decimal Total { get; private set; }
    public decimal WeightKg { get; private set; }
    public string? Address { get; private set; }   // snapshot al crear
    public string? Zone { get; private set; }
    public decimal? Lat { get; private set; }
    public decimal? Lng { get; private set; }
    public string? Notes { get; private set; }
    public string? RejectedReason { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public Client Client { get; private set; } = default!;
    public Driver? Driver { get; private set; }
    public ICollection<OrderItem> Items { get; private set; } = [];
    public Sale? Sale { get; private set; }

    private Order() { }

    public static Order Create(Client client, string? address, string? zone,
        decimal? lat, decimal? lng, string? notes)
    {
        return new Order
        {
            ClientId = client.Id,
            Address = address ?? client.Address,
            Zone = zone ?? client.Zone,
            Lat = lat,
            Lng = lng,
            Notes = notes,
        };
    }

    public void AddItem(Product product, int quantity)
    {
        if (Status != OrderStatus.New)
            throw new InvalidOperationException("Solo se pueden agregar ítems a pedidos nuevos.");
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        var existing = Items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing is not null)
        {
            existing.UpdateQuantity(existing.Quantity + quantity);
        }
        else
        {
            Items.Add(OrderItem.Create(Id, product, quantity));
        }

        RecalculateTotals();
    }

    public void SendToReception()
    {
        EnsureStatus(OrderStatus.New);
        Status = OrderStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(Guid driverId)
    {
        EnsureStatus(OrderStatus.Pending);
        DriverId = driverId;
        Status = OrderStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        EnsureStatus(OrderStatus.Pending);
        Status = OrderStatus.Rejected;
        RejectedReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkEnRoute()
    {
        EnsureStatus(OrderStatus.Approved);
        Status = OrderStatus.EnRoute;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDelivered()
    {
        EnsureStatus(OrderStatus.EnRoute);
        Status = OrderStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Delivered or OrderStatus.Rejected)
            throw new InvalidOperationException("No se puede cancelar un pedido ya procesado.");
        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotals()
    {
        Subtotal = Items.Sum(i => i.Subtotal);
        Iva = Math.Round(Subtotal * 0.16m, 2);
        Total = Subtotal + Iva;
        WeightKg = Items.Sum(i => i.WeightKg);
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureStatus(OrderStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException(
                $"Se esperaba estado '{expected}', pero el pedido está en '{Status}'.");
    }
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.Domain/Entities/OrderItem.cs
// ─────────────────────────────────────────────────────────
namespace RouteOps.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }   // snapshot del precio
    public decimal Subtotal => Quantity * UnitPrice;
    public decimal WeightKg { get; private set; }

    // Navigation
    public Product Product { get; private set; } = default!;

    private OrderItem() { }

    internal static OrderItem Create(Guid orderId, Product product, int quantity)
    {
        return new OrderItem
        {
            OrderId = orderId,
            ProductId = product.Id,
            Quantity = quantity,
            UnitPrice = product.Price,
            WeightKg = product.WeightKg * quantity,
        };
    }

    internal void UpdateQuantity(int newQty)
    {
        Quantity = newQty;
        WeightKg = UnitPrice * newQty; // recalcula
    }
}
