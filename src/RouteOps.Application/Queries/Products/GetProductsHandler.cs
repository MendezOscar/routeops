using MediatR;
using Microsoft.EntityFrameworkCore;
using RouteOps.Application.Interfaces;

namespace RouteOps.Application.Queries.Products;

public sealed class GetProductsHandler(IRouteOpsDbContext db)
    : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductSummaryDto>>
{
    public async Task<IReadOnlyList<ProductSummaryDto>> Handle(
        GetProductsQuery request, CancellationToken ct)
    {
        var query = db.Products
            .Where(p => p.Active)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(s) ||
                p.Sku.ToLower().Contains(s));
        }

        var products = await query
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

        return products.Select(p => new ProductSummaryDto(
            Id:       p.Id.ToString(),
            Name:     p.Name,
            Sku:      p.Sku,
            Category: null,
            Icon:     p.Icon,
            Price:    p.Price,
            Cost:     p.Cost,
            WeightKg: p.WeightKg,
            Stock:    p.Stock,
            MinStock: p.MinStock
        )).ToList();
    }
}
