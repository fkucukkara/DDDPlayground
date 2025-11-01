namespace DDDPlayground.Domain.Orders;

/// <summary>
/// Represents the lifecycle state of an order.
/// Business rules are enforced based on these states.
/// </summary>
public enum OrderStatus
{
    Draft,
    Confirmed,
    Shipped,
    Cancelled
}
