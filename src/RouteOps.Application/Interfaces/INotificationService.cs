namespace RouteOps.Application.Interfaces;

public interface INotificationService
{
    Task SendWhatsAppAsync(string phone, string message, CancellationToken ct = default);
    Task SendPushAsync(string clientToken, string title, string body, CancellationToken ct = default);
}
