using DDDPlayground.Application.Common;
using DDDPlayground.Domain.Exceptions;
using DDDPlayground.Domain.Orders;
using DDDPlayground.Domain.ValueObjects;
using MediatR;

namespace DDDPlayground.Application.Orders.ConfirmOrder;

/// <summary>
/// Handler for ConfirmOrderCommand.
/// Demonstrates how business behavior is invoked through aggregate methods.
/// </summary>
public sealed class ConfirmOrderHandler : IRequestHandler<ConfirmOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmOrderHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponse> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = new OrderId(request.OrderId);
        
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new DomainException($"Order {request.OrderId} not found");
        }

        // Business behavior invoked on aggregate
        order.Confirm();

        // Persist changes
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OrderResponse.FromDomain(order);
    }
}
