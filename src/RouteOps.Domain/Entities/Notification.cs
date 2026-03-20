using RouteOps.Domain.Enums;

namespace RouteOps.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? ClientId { get; private set; }
    public Guid? CreditId { get; private set; }
    public Guid? OrderId { get; private set; }
    public NotifType Type { get; private set; }
    public NotifChannel Channel { get; private set; }
    public string Message { get; private set; } = default!;
    public bool Sent { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? Error { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Notification() { }

    public static Notification Create(NotifType type, NotifChannel channel,
        string message, DateTime? scheduledAt = null,
        Guid? clientId = null, Guid? creditId = null, Guid? orderId = null) =>
        new()
        {
            Type        = type,
            Channel     = channel,
            Message     = message,
            ScheduledAt = scheduledAt ?? DateTime.UtcNow,
            ClientId    = clientId,
            CreditId    = creditId,
            OrderId     = orderId,
        };

    public void MarkSent()   { Sent = true; SentAt = DateTime.UtcNow; }
    public void MarkFailed(string error) { Error = error; SentAt = DateTime.UtcNow; }
}
