using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RouteOps.Application.Queries.Clients;

namespace RouteOps.API.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? search, CancellationToken ct)
    {
        var result = await mediator.Send(new GetClientsQuery(search), ct);
        return Ok(result);
    }
}
