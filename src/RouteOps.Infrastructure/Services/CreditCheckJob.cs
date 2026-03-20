using Hangfire;
using Microsoft.EntityFrameworkCore;
using RouteOps.Application.Interfaces;
using RouteOps.Domain.Entities;
using RouteOps.Domain.Enums;

namespace RouteOps.Infrastructure.Services;

public class CreditCheckJob(IRouteOpsDbContext db, INotificationService notifService)
{
    [AutomaticRetry(Attempts = 2)]
    public async Task RunAsync()
    {
        var credits = await db.Credits
            .Include(c => c.Client)
            .Where(c => c.Balance > 0)
            .ToListAsync();

        foreach (var credit in credits)
        {
            credit.MarkOverdueIfApplicable();

            var daysLeft = credit.DaysUntilDue;
            if (daysLeft is 1 or 3 or 7 || daysLeft <= 0)
            {
                var msg = daysLeft <= 0
                    ? $"🚨 Hola {credit.Client.Name.Split(' ')[0]}, tu crédito vence HOY. " +
                      $"Saldo: ${credit.Balance:N2}. Contáctanos para coordinar tu pago. — RouteOps"
                    : $"📅 Hola {credit.Client.Name.Split(' ')[0]}, tu crédito vence en {daysLeft} día(s). " +
                      $"Saldo: ${credit.Balance:N2}. Vencimiento: {credit.DueDate}. — RouteOps";

                if (credit.Client.Phone is not null)
                    await notifService.SendWhatsAppAsync(credit.Client.Phone, msg);

                db.Notifications.Add(Notification.Create(
                    type:      daysLeft <= 0 ? NotifType.CreditOverdue : NotifType.CreditDueSoon,
                    channel:   NotifChannel.WhatsApp,
                    message:   msg,
                    clientId:  credit.ClientId,
                    creditId:  credit.Id
                ));
            }
        }

        await db.SaveChangesAsync();
    }
}
