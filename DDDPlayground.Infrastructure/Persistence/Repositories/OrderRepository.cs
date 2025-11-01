using DDDPlayground.Domain.Orders;
using DDDPlayground.Domain.ValueObjects;
using DDDPlayground.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace DDDPlayground.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation demonstrating domain/persistence layer separation.
/// 
/// Key Patterns:
/// - Implements domain interface (IOrderRepository in Domain layer)
/// - Works with domain models (Order) in its public API
/// - Uses persistence entities (OrderEntity) internally
/// - Performs explicit mapping between layers
/// - Encapsulates all EF Core concerns from domain
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id.Value, cancellationToken);

        return entity is null ? null : OrderMapper.ToDomain(entity);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId.Value)
            .ToListAsync(cancellationToken);

        return entities.Select(OrderMapper.ToDomain).ToList();
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        var entity = OrderMapper.ToEntity(order);
        await _context.Orders.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == order.Id.Value, cancellationToken);

        if (entity is null)
        {
            return;
        }

        // Update scalar properties
        entity.Status = order.Status.ToString();
        entity.SubtotalAmount = order.Subtotal.Amount;
        entity.SubtotalCurrency = order.Subtotal.Currency;
        entity.DiscountAmount = order.Discount.Amount;
        entity.DiscountCurrency = order.Discount.Currency;
        entity.TotalAmount = order.Total.Amount;
        entity.TotalCurrency = order.Total.Currency;
        entity.ConfirmedAt = order.ConfirmedAt;

        // Update items collection
        entity.Items.Clear();
        foreach (var item in order.Items)
        {
            entity.Items.Add(OrderItemMapper.ToEntity(item));
        }

        _context.Orders.Update(entity);
    }

    public async Task DeleteAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id.Value, cancellationToken);

        if (entity is not null)
        {
            _context.Orders.Remove(entity);
        }
    }
}
