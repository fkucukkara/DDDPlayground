namespace DDDPlayground.Infrastructure.Persistence.Models;

/// <summary>
/// EF Core persistence entity for OrderItem.
/// Separate from domain OrderItem entity.
/// </summary>
public class OrderItemEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    
    // Money flattened to primitive properties
    public decimal UnitPriceAmount { get; set; }
    public string UnitPriceCurrency { get; set; } = "USD";
    
    // EF Core navigation property
    public OrderEntity Order { get; set; } = null!;
}
