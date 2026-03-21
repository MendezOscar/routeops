using MediatR;

namespace RouteOps.Application.Queries.Clients;

public record ClientSummaryDto(
    string  Id,
    string  Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Zone,
    decimal CreditLimit,
    int     CreditDays,
    decimal CurrentBalance,
    int     TotalOrders
);

public record GetClientsQuery(string? Search = null)
    : IRequest<IReadOnlyList<ClientSummaryDto>>;
