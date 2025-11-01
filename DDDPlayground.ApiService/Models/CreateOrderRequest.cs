namespace DDDPlayground.ApiService.Models;

/// <summary>
/// API request DTO for creating an order.
/// Separate from application DTOs - demonstrates API layer independence.
/// </summary>
public sealed record CreateOrderRequest
{
    public required Guid CustomerId { get; init; }
    public required List<CreateOrderItemRequest> Items { get; init; }
}

/// <summary>
/// API request DTO for creating an order item.
/// </summary>
public sealed record CreateOrderItemRequest
{
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public string Currency { get; init; } = "USD";
}
