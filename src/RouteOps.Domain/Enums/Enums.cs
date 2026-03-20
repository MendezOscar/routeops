// src/RouteOps.Domain/Enums/Enums.cs
namespace RouteOps.Domain.Enums;

public enum OrderStatus
{
    New,        // recién creado
    Pending,    // en recepción
    Approved,   // aprobado, listo para ruta
    Rejected,   // rechazado
    EnRoute,    // en camino
    Delivered,  // entregado
    Cancelled
}

public enum PayMethod
{
    Credit,     // a crédito
    Cash,       // contado
    Transfer,   // transferencia
    Card        // tarjeta
}

public enum CreditStatus
{
    Pending,    // sin abonos
    Partial,    // con abonos, saldo > 0
    Paid,       // liquidado
    Overdue     // vencido con saldo
}

public enum MovementType
{
    In,          // entrada (compra, ajuste +)
    Out,         // salida (venta, ajuste -)
    Adjustment   // ajuste manual
}

public enum PurchaseOrderStatus
{
    Draft,
    Sent,
    Partial,
    Received,
    Cancelled
}

public enum NotifType
{
    CreditDueSoon,
    CreditOverdue,
    LowStock,
    OrderApproved,
    OrderEnRoute,
    OrderDelivered
}

public enum NotifChannel
{
    WhatsApp,
    Sms,
    Push,
    Email,
    Internal
}
