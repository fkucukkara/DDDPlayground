namespace DDDPlayground.Domain.ValueObjects;

/// <summary>
/// Strongly-typed ID for Order aggregate.
/// Prevents primitive obsession and provides type safety.
/// </summary>
public sealed record OrderId
{
    public Guid Value { get; init; }

    public OrderId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("OrderId cannot be empty", nameof(value));
        }

        Value = value;
    }

    public static OrderId New() => new(Guid.NewGuid());

    public static OrderId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    // Implicit conversion for convenience
    public static implicit operator Guid(OrderId orderId) => orderId.Value;
}
