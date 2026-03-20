using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RouteOps.Application.Queries.Routes;
using RouteOps.Application.Queries.Dashboard;

namespace RouteOps.API.Controllers;

[ApiController]
[Route("api/routes")]
[Authorize]
public class RoutesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] bool optimize = false, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetRoutesByDriverQuery(optimize), ct);
        return Ok(result);
    }
}

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await mediator.Send(new GetDashboardQuery(), ct);
        return Ok(result);
    }
}
