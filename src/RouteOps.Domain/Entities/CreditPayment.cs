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
            CreditId  = creditId,
            Amount    = amount,
            Method    = method,
            Reference = reference,
            Notes     = notes,
        };
}
