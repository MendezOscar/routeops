using MediatR;

namespace RouteOps.Application.Commands.Credits;

public record ApplyPaymentCommand(
    Guid CreditId,
    decimal Amount,
    string Method,
    string? Reference,
    string? Notes
) : IRequest<Guid>;
