using DDDPlayground.Domain.Orders;

namespace DDDPlayground.Application.Orders;

/// <summary>
/// Response DTO for Order operations.
/// Manual mapping from domain model - no AutoMapper for educational clarity.
/// 
/// Key Pattern: Data Transfer Object (DTO)
/// - Separates API contract from domain model
/// - Prevents over-posting
/// - Allows domain model to evolve independently
/// </summary>
public sealed record OrderResponse
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public MoneyDto Subtotal { get; init; } = null!;
    public MoneyDto Discount { get; init; } = null!;
    public MoneyDto Total { get; init; } = null!;
    public List<OrderItemDto> Items { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime? ConfirmedAt { get; init; }

    /// <summary>
    /// Manual mapping from domain aggregate to DTO.
    /// Explicit and educational - shows exactly what's being transformed.
    /// </summary>
    public static OrderResponse FromDomain(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id.Value,
            CustomerId = order.CustomerId.Value,
            Status = order.Status.ToString(),
            Subtotal = MoneyDto.FromDomain(order.Subtotal),
            Discount = MoneyDto.FromDomain(order.Discount),
            Total = MoneyDto.FromDomain(order.Total),
            Items = order.Items.Select(OrderItemDto.FromDomain).ToList(),
            CreatedAt = order.CreatedAt,
            ConfirmedAt = order.ConfirmedAt
        };
    }
}

public sealed record OrderItemDto
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public MoneyDto UnitPrice { get; init; } = null!;
    public MoneyDto LineTotal { get; init; } = null!;

    public static OrderItemDto FromDomain(OrderItem item)
    {
        return new OrderItemDto
        {
            ProductId = item.ProductId.Value,
            Quantity = item.Quantity,
            UnitPrice = MoneyDto.FromDomain(item.UnitPrice),
            LineTotal = MoneyDto.FromDomain(item.LineTotal)
        };
    }
}

public sealed record MoneyDto
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;

    public static MoneyDto FromDomain(DDDPlayground.Domain.ValueObjects.Money money)
    {
        return new MoneyDto
        {
            Amount = money.Amount,
            Currency = money.Currency
        };
    }
}
