using MediatR;

namespace DDDPlayground.Application.Orders.CreateOrder;

/// <summary>
/// Command to create a new order.
/// 
/// Key CQRS Pattern: Command
/// - Represents an intent to change state
/// - Contains all data needed for the operation
/// - Returns the created aggregate or result
/// </summary>
public sealed record CreateOrderCommand : IRequest<OrderResponse>
{
    public Guid CustomerId { get; init; }
    public List<CreateOrderItemDto> Items { get; init; } = new();
}

public sealed record CreateOrderItemDto
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string Currency { get; init; } = "USD";
}
