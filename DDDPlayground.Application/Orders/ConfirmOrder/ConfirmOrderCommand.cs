using MediatR;

namespace DDDPlayground.Application.Orders.ConfirmOrder;

/// <summary>
/// Command to confirm an order (Draft → Confirmed state transition).
/// </summary>
public sealed record ConfirmOrderCommand : IRequest<OrderResponse>
{
    public Guid OrderId { get; init; }

    public ConfirmOrderCommand(Guid orderId)
    {
        OrderId = orderId;
    }
}
