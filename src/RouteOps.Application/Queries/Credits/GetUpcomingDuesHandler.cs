using MediatR;
using Microsoft.EntityFrameworkCore;
using RouteOps.Application.Interfaces;

namespace RouteOps.Application.Queries.Credits;

public sealed class GetUpcomingDuesHandler(IRouteOpsDbContext db)
    : IRequestHandler<GetUpcomingDuesQuery, IReadOnlyList<UpcomingDueDto>>
{
    public async Task<IReadOnlyList<UpcomingDueDto>> Handle(
        GetUpcomingDuesQuery request, CancellationToken ct)
    {
        var today    = DateOnly.FromDateTime(DateTime.UtcNow);
        var deadline = today.AddDays(request.DaysAhead);

        var credits = await db.Credits
            .Include(c => c.Client)
            .Where(c => c.Balance > 0 && c.DueDate <= deadline)
            .OrderBy(c => c.DueDate)
            .ToListAsync(ct);

        return credits.Select(c => new UpcomingDueDto(
            CreditId:     c.Id,
            ClientId:     c.ClientId,
            ClientName:   c.Client.Name,
            ClientPhone:  c.Client.Phone,
            Balance:      c.Balance,
            DueDate:      c.DueDate,
            DaysUntilDue: c.DaysUntilDue
        )).ToList();
    }
}
