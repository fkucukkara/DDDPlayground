namespace DDDPlayground.Infrastructure.Persistence.Models;

/// <summary>
/// EF Core persistence entity for Order.
/// Separate from domain model - demonstrates domain/persistence separation.
/// 
/// Key Pattern: Separate Persistence Model
/// - Different class from domain Order
/// - Contains EF-specific concerns (navigation properties, etc.)
/// - Can evolve independently from domain
/// - Mapped to/from domain via OrderMapper
/// </summary>
public class OrderEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    
    // Money flattened to primitive properties
    public decimal SubtotalAmount { get; set; }
    public string SubtotalCurrency { get; set; } = "USD";
    
    public decimal DiscountAmount { get; set; }
    public string DiscountCurrency { get; set; } = "USD";
    
    public decimal TotalAmount { get; set; }
    public string TotalCurrency { get; set; } = "USD";
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    
    // EF Core navigation property (not in domain model)
    public List<OrderItemEntity> Items { get; set; } = new();
}
