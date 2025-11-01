using DDDPlayground.Application.Common;
using DDDPlayground.Domain.Orders;
using DDDPlayground.Domain.ValueObjects;
using MediatR;

namespace DDDPlayground.Application.Orders.CreateOrder;

/// <summary>
/// Handler for CreateOrderCommand.
/// Orchestrates the use case of creating an order.
/// 
/// Responsibilities:
/// - Validate command
/// - Create domain aggregate
/// - Persist through repository
/// - Return response DTO
/// </summary>
public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Convert DTOs to domain value objects and entities
        var customerId = new CustomerId(request.CustomerId);
        
        var orderItems = request.Items.Select(item =>
            OrderItem.Create(
                new ProductId(item.ProductId),
                item.Quantity,
                new Money(item.UnitPrice, item.Currency)
            )
        ).ToList();

        // Create domain aggregate (business rules enforced here)
        var order = Order.Create(customerId, orderItems);

        // Persist aggregate
        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Manual mapping: Domain → Response DTO
        return OrderResponse.FromDomain(order);
    }
}
