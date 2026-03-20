using RouteOps.Application.Interfaces;

namespace RouteOps.Infrastructure.Services;

public class WhatsAppNotificationService : INotificationService
{
    public Task SendWhatsAppAsync(string phone, string message, CancellationToken ct = default)
    {
        // TODO: integrar con API de WhatsApp Business o Twilio
        Console.WriteLine($"[WhatsApp] → {phone}: {message}");
        return Task.CompletedTask;
    }

    public Task SendPushAsync(string clientToken, string title, string body, CancellationToken ct = default)
    {
        // TODO: integrar con Firebase FCM
        Console.WriteLine($"[FCM] → {clientToken}: {title} — {body}");
        return Task.CompletedTask;
    }
}
