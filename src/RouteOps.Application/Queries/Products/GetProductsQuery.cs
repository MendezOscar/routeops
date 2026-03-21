using MediatR;

namespace RouteOps.Application.Queries.Products;

public record ProductSummaryDto(
    string  Id,
    string  Name,
    string  Sku,
    string? Category,
    string  Icon,
    decimal Price,
    decimal Cost,
    decimal WeightKg,
    int     Stock,
    int     MinStock
);

public record GetProductsQuery(string? Search = null)
    : IRequest<IReadOnlyList<ProductSummaryDto>>;
