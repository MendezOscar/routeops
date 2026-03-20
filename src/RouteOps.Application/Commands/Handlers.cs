// src/RouteOps.Application/Commands/Orders/ApproveOrderHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using RouteOps.Application.Interfaces;
using RouteOps.Domain.Entities;
using RouteOps.Domain.Enums;

namespace RouteOps.Application.Commands.Orders;

/// Handler de ApproveOrderCommand:
/// 1. Valida stock de cada ítem
/// 2. Valida límite de crédito del cliente (si pago = crédito)
/// 3. Descuenta stock + registra movimientos
/// 4. Crea la Sale
/// 5. Si es crédito, crea el Credit con fecha de vencimiento
/// 6. Aprueba el Order
public sealed class ApproveOrderHandler(IRouteOpsDbContext db)
    : IRequestHandler<ApproveOrderCommand>
{
    public async Task Handle(ApproveOrderCommand cmd, CancellationToken ct)
    {
        // ── Cargar pedido con ítems y cliente ──────────────────
        var order = await db.Orders
            .Include(o => o.Items)
            .Include(o => o.Client)
                .ThenInclude(c => c.Credits)
            .FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct)
            ?? throw new KeyNotFoundException($"Pedido {cmd.OrderId} no encontrado.");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException(
                $"El pedido debe estar en 'Pending' para aprobarse. Estado actual: {order.Status}");

        // ── Validar stock ──────────────────────────────────────
        var productIds = order.Items.Select(i => i.ProductId).ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var stockErrors = order.Items
            .Where(i => !products.TryGetValue(i.ProductId, out var p) || p.Stock < i.Quantity)
            .Select(i => $"{products.GetValueOrDefault(i.ProductId)?.Name ?? i.ProductId.ToString()}: " +
                         $"stock={products.GetValueOrDefault(i.ProductId)?.Stock ?? 0}, requerido={i.Quantity}")
            .ToList();

        if (stockErrors.Count > 0)
            throw new InvalidOperationException(
                "Stock insuficiente:\n" + string.Join("\n", stockErrors));

        // ── Validar límite de crédito ──────────────────────────
        if (cmd.PayMethod == PayMethod.Credit)
        {
            var available = order.Client.AvailableCredit;
            if (available < order.Total)
                throw new InvalidOperationException(
                    $"Crédito disponible insuficiente. Disponible: {available:C}, pedido: {order.Total:C}");
        }

        // ── Descontar stock + registrar movimientos ────────────
        foreach (var item in order.Items)
        {
            var product = products[item.ProductId];
            product.AdjustStock(-item.Quantity);

            db.InventoryMovements.Add(new InventoryMovement
            {
                ProductId = product.Id,
                Type      = MovementType.Out,
                Quantity  = item.Quantity,
                Reason    = "sale",
                Reference = order.Id.ToString(),
            });
        }

        // ── Crear Sale ─────────────────────────────────────────
        var sale = Sale.Create(order, cmd.PayMethod);
        db.Sales.Add(sale);

        // ── Crear Credit si aplica ─────────────────────────────
        if (cmd.PayMethod == PayMethod.Credit)
        {
            var credit = Credit.Create(sale, order.Client.CreditDays);
            db.Credits.Add(credit);
        }

        // ── Aprobar pedido ─────────────────────────────────────
        order.Approve(cmd.DriverId);

        await db.SaveChangesAsync(ct);
    }
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.Application/Commands/Credits/ApplyPaymentHandler.cs
// ─────────────────────────────────────────────────────────
public sealed class ApplyPaymentHandler(IRouteOpsDbContext db)
    : IRequestHandler<ApplyPaymentCommand, Guid>
{
    public async Task Handle(ApplyPaymentCommand cmd, CancellationToken ct)
    {
        var credit = await db.Credits
            .Include(c => c.Payments)
            .FirstOrDefaultAsync(c => c.Id == cmd.CreditId, ct)
            ?? throw new KeyNotFoundException($"Crédito {cmd.CreditId} no encontrado.");

        var payment = credit.ApplyPayment(
            cmd.Amount, cmd.Method, cmd.Reference, cmd.Notes);

        db.CreditPayments.Add(payment);
        await db.SaveChangesAsync(ct);
        return payment.Id;
    }
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.Application/Commands/Inventory/ReceivePurchaseOrderHandler.cs
// ─────────────────────────────────────────────────────────
public sealed class ReceivePurchaseOrderHandler(IRouteOpsDbContext db)
    : IRequestHandler<ReceivePurchaseOrderCommand>
{
    public async Task Handle(ReceivePurchaseOrderCommand cmd, CancellationToken ct)
    {
        var po = await db.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == cmd.PurchaseOrderId, ct)
            ?? throw new KeyNotFoundException($"Orden de compra {cmd.PurchaseOrderId} no encontrada.");

        if (po.Status == PurchaseOrderStatus.Received)
            throw new InvalidOperationException("Esta orden ya fue recibida.");

        var productIds = po.Items.Select(i => i.ProductId).ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        foreach (var item in po.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product)) continue;

            product.AdjustStock(item.Quantity);

            db.InventoryMovements.Add(new InventoryMovement
            {
                ProductId = product.Id,
                Type      = MovementType.In,
                Quantity  = item.Quantity,
                Reason    = "purchase",
                Reference = po.Id.ToString(),
            });
        }

        po.Receive();
        await db.SaveChangesAsync(ct);
    }
}
