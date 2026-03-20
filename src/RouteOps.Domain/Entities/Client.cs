// src/RouteOps.Domain/Entities/Client.cs
namespace RouteOps.Domain.Entities;

public class Client
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = default!;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? Zone { get; private set; }
    public decimal CreditLimit { get; private set; }
    public int CreditDays { get; private set; }   // 8 | 15 | 30 | 45 | 60
    public bool Active { get; private set; } = true;
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Order> Orders { get; private set; } = [];
    public ICollection<Credit> Credits { get; private set; } = [];
    public ICollection<Notification> Notifications { get; private set; } = [];

    private Client() { } // EF Core

    public static Client Create(
        string name, string? phone, string? email,
        string? address, string? zone,
        decimal creditLimit, int creditDays)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (creditLimit < 0) throw new ArgumentOutOfRangeException(nameof(creditLimit));
        if (!new[] { 8, 15, 30, 45, 60 }.Contains(creditDays))
            throw new ArgumentException("creditDays debe ser 8, 15, 30, 45 o 60.", nameof(creditDays));

        return new Client
        {
            Name = name.Trim(),
            Phone = phone?.Trim(),
            Email = email?.Trim(),
            Address = address?.Trim(),
            Zone = zone?.Trim(),
            CreditLimit = creditLimit,
            CreditDays = creditDays,
        };
    }

    public void Update(string name, string? phone, string? email,
        string? address, string? zone, decimal creditLimit, int creditDays)
    {
        Name = name.Trim();
        Phone = phone?.Trim();
        Email = email?.Trim();
        Address = address?.Trim();
        Zone = zone?.Trim();
        CreditLimit = creditLimit;
        CreditDays = creditDays;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() { Active = false; UpdatedAt = DateTime.UtcNow; }

    /// Saldo pendiente calculado desde los créditos cargados
    public decimal CurrentBalance =>
        Credits.Where(c => c.Balance > 0).Sum(c => c.Balance);

    /// Crédito disponible
    public decimal AvailableCredit => CreditLimit - CurrentBalance;

    public bool CanPlaceOrder(decimal orderTotal) =>
        AvailableCredit >= orderTotal;
}
