using MediatR;

namespace RouteOps.Application.Queries.Orders;

public record OrderDetailDto(
    Guid Id,
    string ClientName,
    string Address,
    string Status,
    decimal Total,
    string? DriverName
);

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDetailDto?>;
