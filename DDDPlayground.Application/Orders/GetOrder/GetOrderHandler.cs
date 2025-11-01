using DDDPlayground.Domain.Orders;
using DDDPlayground.Domain.ValueObjects;
using MediatR;

namespace DDDPlayground.Application.Orders.GetOrder;

/// <summary>
/// Handler for GetOrderQuery.
/// Simple read operation returning a DTO.
/// </summary>
public sealed class GetOrderHandler : IRequestHandler<GetOrderQuery, OrderResponse?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderResponse?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var orderId = new OrderId(request.OrderId);
        
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        return order == null ? null : OrderResponse.FromDomain(order);
    }
}
