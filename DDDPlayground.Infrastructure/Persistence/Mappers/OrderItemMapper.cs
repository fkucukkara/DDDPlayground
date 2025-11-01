using DDDPlayground.Domain.Orders;
using DDDPlayground.Domain.ValueObjects;
using DDDPlayground.Infrastructure.Persistence.Models;

namespace DDDPlayground.Infrastructure.Persistence.Mappers;

/// <summary>
/// Manual mapper for OrderItem entity transformations.
/// 
/// Note: OrderItem in domain has no Id property (it's a simple entity within aggregate).
/// OrderItemEntity in persistence needs an Id for EF Core, but domain doesn't care about it.
/// This demonstrates domain/persistence separation.
/// </summary>
public static class OrderItemMapper
{
    /// <summary>
    /// Converts domain OrderItem to persistence OrderItemEntity.
    /// Generates new Id since domain OrderItem doesn't have one.
    /// </summary>
    public static OrderItemEntity ToEntity(OrderItem item)
    {
        return new OrderItemEntity
        {
            Id = Guid.NewGuid(), // Generate Id for persistence
            ProductId = item.ProductId.Value,
            Quantity = item.Quantity,
            UnitPriceAmount = item.UnitPrice.Amount,
            UnitPriceCurrency = item.UnitPrice.Currency
        };
    }

    /// <summary>
    /// Converts persistence OrderItemEntity to domain OrderItem.
    /// Domain OrderItem doesn't need the persistence Id.
    /// </summary>
    public static OrderItem ToDomain(OrderItemEntity entity)
    {
        return OrderItem.Create(
            ProductId.From(entity.ProductId),
            entity.Quantity,
            new Money(entity.UnitPriceAmount, entity.UnitPriceCurrency)
        );
    }
}
