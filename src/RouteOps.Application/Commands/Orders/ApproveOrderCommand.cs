using MediatR;
using RouteOps.Domain.Enums;

namespace RouteOps.Application.Commands.Orders;

public record ApproveOrderCommand(
    Guid OrderId,
    Guid DriverId,
    PayMethod PayMethod
) : IRequest;
