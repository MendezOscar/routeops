using MediatR;

namespace RouteOps.Application.Commands.Orders;

public record RejectOrderCommand(Guid OrderId, string Reason) : IRequest;

public record SendToReceptionCommand(Guid OrderId) : IRequest;

public record MarkDeliveredCommand(Guid OrderId) : IRequest;
