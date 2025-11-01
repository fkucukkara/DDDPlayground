using DDDPlayground.Domain.ValueObjects;

namespace DDDPlayground.Domain.Events;

/// <summary>
/// Domain Event raised when an order is confirmed.
/// 
/// Key DDD Pattern: Domain Events
/// - Represents something significant that happened in the domain
/// - Dispatched in-memory (not persisted for this MVP)
/// - Allows decoupled side effects (logging, notifications, etc.)
/// - Named in past tense (OrderConfirmed, not OrderConfirming)
/// </summary>
public sealed record OrderConfirmedEvent
{
    public OrderId OrderId { get; init; }
    public CustomerId CustomerId { get; init; }
    public Money Total { get; init; }
    public DateTime ConfirmedAt { get; init; }

    public OrderConfirmedEvent(OrderId orderId, CustomerId customerId, Money total, DateTime confirmedAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Total = total;
        ConfirmedAt = confirmedAt;
    }
}
