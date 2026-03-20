using MediatR;

namespace RouteOps.Application.Queries.Credits;

public record UpcomingDueDto(
    Guid CreditId,
    Guid ClientId,
    string ClientName,
    string? ClientPhone,
    decimal Balance,
    DateOnly DueDate,
    int DaysUntilDue
);

public record GetUpcomingDuesQuery(int DaysAhead = 7) : IRequest<IReadOnlyList<UpcomingDueDto>>;
