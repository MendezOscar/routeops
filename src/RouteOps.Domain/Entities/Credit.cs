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

    public Sale Sale { get; private set; } = default!;
    public Client Client { get; private set; } = default!;
    public ICollection<CreditPayment> Payments { get; private set; } = [];

    private Credit() { }

    public static Credit Create(Sale sale, int creditDays)
    {
        if (sale.PayMethod != PayMethod.Credit)
            throw new InvalidOperationException(
                "Solo ventas a crédito generan un registro de crédito.");

        return new Credit
        {
            SaleId   = sale.Id,
            ClientId = sale.ClientId,
            Total    = sale.Total,
            Balance  = sale.Total,
            DueDate  = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(creditDays)),
        };
    }

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
        Status  = Balance == 0 ? CreditStatus.Paid : CreditStatus.Partial;
        UpdatedAt = DateTime.UtcNow;

        return payment;
    }

    public void MarkOverdueIfApplicable()
    {
        if (Balance > 0 && DueDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            Status    = CreditStatus.Overdue;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public int DaysUntilDue =>
        DueDate.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;

    public bool IsOverdue  => Balance > 0 && DaysUntilDue < 0;
    public bool IsDueSoon  => Balance > 0 && DaysUntilDue is >= 0 and <= 7;
}
