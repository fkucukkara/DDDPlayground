using DDDPlayground.Domain.ValueObjects;

namespace DDDPlayground.Domain.Orders;

/// <summary>
/// Repository interface for Order aggregate.
/// Defined in Domain layer, implemented in Infrastructure layer.
/// 
/// Key DDD Pattern: Repository Pattern
/// - Interface in domain (persistence ignorant)
/// - Implementation in infrastructure
/// - Aggregate-focused (not generic CRUD)
/// - Works with domain entities, not database entities
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Get an order by its ID.
    /// </summary>
    Task<Order?> GetByIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new order.
    /// </summary>
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing order.
    /// </summary>
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders by customer ID.
    /// </summary>
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
}
