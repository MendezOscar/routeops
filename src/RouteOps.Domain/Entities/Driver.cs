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
