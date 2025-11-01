namespace DDDPlayground.Domain.ValueObjects;

/// <summary>
/// Strongly-typed ID for Product aggregate.
/// </summary>
public sealed record ProductId
{
    public Guid Value { get; init; }

    public ProductId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ProductId cannot be empty", nameof(value));
        }

        Value = value;
    }

    public static ProductId New() => new(Guid.NewGuid());

    public static ProductId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ProductId productId) => productId.Value;
}
