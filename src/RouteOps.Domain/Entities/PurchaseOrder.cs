using RouteOps.Domain.Enums;

namespace RouteOps.Domain.Entities;

public class PurchaseOrder
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Supplier { get; private set; } = default!;
    public PurchaseOrderStatus Status { get; private set; } = PurchaseOrderStatus.Draft;
    public decimal Total { get; private set; }
    public string? Notes { get; private set; }
    public DateOnly? ExpectedAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<PurchaseItem> Items { get; private set; } = [];

    private PurchaseOrder() { }

    public static PurchaseOrder Create(string supplier, string? notes = null,
        DateOnly? expectedAt = null) =>
        new()
        {
            Supplier   = supplier.Trim(),
            Notes      = notes,
            ExpectedAt = expectedAt,
        };

    public void Receive()
    {
        Status    = PurchaseOrderStatus.Received;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class PurchaseItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public int ReceivedQty { get; private set; }
    public decimal Subtotal => Quantity * UnitCost;

    public Product Product { get; private set; } = default!;

    private PurchaseItem() { }
}
