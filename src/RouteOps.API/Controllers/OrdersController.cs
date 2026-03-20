using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RouteOps.Application.Commands.Orders;
using RouteOps.Application.Queries.Orders;
using RouteOps.Domain.Enums;

namespace RouteOps.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPatch("{id:guid}/reception")]
    public async Task<IActionResult> SendToReception(Guid id, CancellationToken ct)
    {
        await mediator.Send(new SendToReceptionCommand(id), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveOrderRequest req, CancellationToken ct)
    {
        await mediator.Send(new ApproveOrderCommand(id, req.DriverId, req.PayMethod), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectOrderRequest req, CancellationToken ct)
    {
        await mediator.Send(new RejectOrderCommand(id, req.Reason), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/delivered")]
    public async Task<IActionResult> MarkDelivered(Guid id, CancellationToken ct)
    {
        await mediator.Send(new MarkDeliveredCommand(id), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrderByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}

public record ApproveOrderRequest(Guid DriverId, PayMethod PayMethod);
public record RejectOrderRequest(string Reason);
