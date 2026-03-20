using MediatR;

namespace RouteOps.Application.Commands.Orders;

public record CreateOrderItemDto(Guid ProductId, int Quantity);

public record CreateOrderCommand(
    Guid ClientId,
    string? Address,
    string? Zone,
    decimal? Lat,
    decimal? Lng,
    string? Notes,
    IReadOnlyList<CreateOrderItemDto> Items
) : IRequest<Guid>;
