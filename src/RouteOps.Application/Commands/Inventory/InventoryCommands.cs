using MediatR;

namespace RouteOps.Application.Commands.Inventory;

public record ReceivePurchaseOrderCommand(Guid PurchaseOrderId) : IRequest;

public record AdjustStockCommand(
    Guid ProductId,
    int NewStock,
    string? Notes
) : IRequest;
