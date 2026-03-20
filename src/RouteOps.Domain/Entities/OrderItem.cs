namespace RouteOps.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Subtotal => Quantity * UnitPrice;
    public decimal WeightKg { get; private set; }

    public Product Product { get; private set; } = default!;

    private OrderItem() { }

    internal static OrderItem Create(Guid orderId, Product product, int quantity) =>
        new()
        {
            OrderId   = orderId,
            ProductId = product.Id,
            Quantity  = quantity,
            UnitPrice = product.Price,
            WeightKg  = product.WeightKg * quantity,
        };

    internal void UpdateQuantity(int newQty)
    {
        Quantity = newQty;
        WeightKg = UnitPrice * newQty;
    }
}
