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

    public Order Order { get; private set; } = default!;
    public Client Client { get; private set; } = default!;
    public Credit? Credit { get; private set; }

    private Sale() { }

    public static Sale Create(Order order, PayMethod payMethod, string? notes = null)
    {
        if (order.Status != OrderStatus.Approved)
            throw new InvalidOperationException(
                "Solo se puede generar una venta de un pedido aprobado.");

        return new Sale
        {
            OrderId    = order.Id,
            ClientId   = order.ClientId,
            PayMethod  = payMethod,
            Subtotal   = order.Subtotal,
            Iva        = order.Iva,
            Total      = order.Total,
            Notes      = notes,
        };
    }
}
