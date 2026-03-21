using MediatR;
using Microsoft.EntityFrameworkCore;
using RouteOps.Application.Interfaces;

namespace RouteOps.Application.Queries.Clients;

public sealed class GetClientsHandler(IRouteOpsDbContext db)
    : IRequestHandler<GetClientsQuery, IReadOnlyList<ClientSummaryDto>>
{
    public async Task<IReadOnlyList<ClientSummaryDto>> Handle(
        GetClientsQuery request, CancellationToken ct)
    {
        var query = db.Clients
            .Where(c => c.Active)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(s) ||
                (c.Phone != null && c.Phone.Contains(s)) ||
                (c.Zone  != null && c.Zone.ToLower().Contains(s)));
        }

        var clients = await query
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        var clientIds = clients.Select(c => c.Id).ToList();

        var balances = await db.Credits
            .Where(cr => clientIds.Contains(cr.ClientId) && cr.Balance > 0)
            .GroupBy(cr => cr.ClientId)
            .Select(g => new { ClientId = g.Key, Balance = g.Sum(cr => cr.Balance) })
            .ToDictionaryAsync(x => x.ClientId, x => x.Balance, ct);

        var orderCounts = await db.Orders
            .Where(o => clientIds.Contains(o.ClientId))
            .GroupBy(o => o.ClientId)
            .Select(g => new { ClientId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClientId, x => x.Count, ct);

        return clients.Select(c => new ClientSummaryDto(
            Id:             c.Id.ToString(),
            Name:           c.Name,
            Phone:          c.Phone,
            Email:          c.Email,
            Address:        c.Address,
            Zone:           c.Zone,
            CreditLimit:    c.CreditLimit,
            CreditDays:     c.CreditDays,
            CurrentBalance: balances.GetValueOrDefault(c.Id, 0),
            TotalOrders:    orderCounts.GetValueOrDefault(c.Id, 0)
        )).ToList();
    }
}
