using RouteOps.Domain.Enums;

namespace RouteOps.Domain.Entities;

public class InventoryMovement
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProductId { get; private set; }
    public MovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public string? Reason { get; private set; }
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public Product Product { get; private set; } = default!;

    // EF Core + creación directa desde handlers
    public InventoryMovement() { }
}
