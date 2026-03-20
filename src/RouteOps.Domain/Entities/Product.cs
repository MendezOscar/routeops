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
            Name       = name.Trim(),
            Sku        = sku.Trim().ToUpperInvariant(),
            Price      = price,
            Cost       = cost,
            WeightKg   = weightKg,
            MinStock   = minStock,
            Icon       = icon ?? "📦",
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
        Name      = name.Trim();
        Price     = price;
        Cost      = cost;
        WeightKg  = weightKg;
        MinStock  = minStock;
        Icon      = icon ?? Icon;
        UpdatedAt = DateTime.UtcNow;
    }
}
