using MediatR;

namespace DDDPlayground.Application.Orders.GetOrder;

/// <summary>
/// Query to retrieve an order by ID.
/// 
/// Key CQRS Pattern: Query
/// - Represents a request for data
/// - Does not change state
/// - Returns a DTO
/// </summary>
public sealed record GetOrderQuery : IRequest<OrderResponse?>
{
    public Guid OrderId { get; init; }

    public GetOrderQuery(Guid orderId)
    {
        OrderId = orderId;
    }
}
