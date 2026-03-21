using MediatR;

namespace RouteOps.Application.Queries.Orders;

public record OrderItemDto(
    string  ProductId,
    string  ProductName,
    string  ProductIcon,
    int     Quantity,
    decimal UnitPrice
);

public record OrderSummaryDto(
    string           Id,
    string           ClientId,
    string           ClientName,
    string           Address,
    string           Zone,
    string           Status,
    decimal          Total,
    decimal          WeightKg,
    string?          DriverName,
    string?          Phone,
    string?          Notes,
    string?          RejectedReason,
    DateTime         CreatedAt,
    List<OrderItemDto> Items
);

public record GetOrdersQuery(string? StatusFilter = null)
    : IRequest<IReadOnlyList<OrderSummaryDto>>;
