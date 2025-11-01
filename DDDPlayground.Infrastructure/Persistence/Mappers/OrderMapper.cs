using DDDPlayground.Domain.Orders;
using DDDPlayground.Domain.ValueObjects;
using DDDPlayground.Infrastructure.Persistence.Models;

namespace DDDPlayground.Infrastructure.Persistence.Mappers;

/// <summary>
/// Manual mapper demonstrating explicit domain ↔ persistence transformation.
/// 
/// Key Pattern: Explicit Mapping (No AutoMapper)
/// - Complete control over transformation logic
/// - Clear understanding of how data moves between layers
/// - Easier to debug and maintain for educational purposes
/// - Demonstrates separation of concerns
/// </summary>
public static class OrderMapper
{
    /// <summary>
    /// Converts domain Order to persistence OrderEntity.
    /// Used when saving to database.
    /// </summary>
    public static OrderEntity ToEntity(Order order)
    {
        return new OrderEntity
        {
            Id = order.Id.Value,
            CustomerId = order.CustomerId.Value,
            Status = order.Status.ToString(),
            SubtotalAmount = order.Subtotal.Amount,
            SubtotalCurrency = order.Subtotal.Currency,
            DiscountAmount = order.Discount.Amount,
            DiscountCurrency = order.Discount.Currency,
            TotalAmount = order.Total.Amount,
            TotalCurrency = order.Total.Currency,
            CreatedAt = order.CreatedAt,
            ConfirmedAt = order.ConfirmedAt,
            Items = order.Items.Select(OrderItemMapper.ToEntity).ToList()
        };
    }

    /// <summary>
    /// Converts persistence OrderEntity to domain Order.
    /// Used when loading from database.
    /// Uses Order.Reconstitute to avoid business rule validation.
    /// </summary>
    public static Order ToDomain(OrderEntity entity)
    {
        var items = entity.Items.Select(OrderItemMapper.ToDomain).ToList();

        return Order.Reconstitute(
            OrderId.From(entity.Id),
            CustomerId.From(entity.CustomerId),
            Enum.Parse<OrderStatus>(entity.Status),
            items,
            new Money(entity.SubtotalAmount, entity.SubtotalCurrency),
            new Money(entity.DiscountAmount, entity.DiscountCurrency),
            new Money(entity.TotalAmount, entity.TotalCurrency),
            entity.CreatedAt,
            entity.ConfirmedAt
        );
    }
}
