namespace DDDPlayground.Domain.ValueObjects;

/// <summary>
/// Strongly-typed ID for Customer aggregate.
/// </summary>
public sealed record CustomerId
{
    public Guid Value { get; init; }

    public CustomerId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CustomerId cannot be empty", nameof(value));
        }

        Value = value;
    }

    public static CustomerId New() => new(Guid.NewGuid());

    public static CustomerId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(CustomerId customerId) => customerId.Value;
}
