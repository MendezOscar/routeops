using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RouteOps.Application.Commands.Credits;
using RouteOps.Application.Queries.Credits;

namespace RouteOps.API.Controllers;

[ApiController]
[Route("api/credits")]
[Authorize]
public class CreditsController(IMediator mediator) : ControllerBase
{
    [HttpGet("upcoming-dues")]
    public async Task<IActionResult> UpcomingDues(
        [FromQuery] int daysAhead = 7, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetUpcomingDuesQuery(daysAhead), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/payments")]
    public async Task<IActionResult> ApplyPayment(
        Guid id, [FromBody] ApplyPaymentCommand cmd, CancellationToken ct)
    {
        var paymentId = await mediator.Send(cmd with { CreditId = id }, ct);
        return Ok(new { paymentId });
    }
}
