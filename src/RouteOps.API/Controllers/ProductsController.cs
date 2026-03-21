using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RouteOps.Application.Queries.Products;

namespace RouteOps.API.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? search, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductsQuery(search), ct);
        return Ok(result);
    }
}
