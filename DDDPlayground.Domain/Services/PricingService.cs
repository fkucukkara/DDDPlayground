using DDDPlayground.Domain.ValueObjects;

namespace DDDPlayground.Domain.Services;

/// <summary>
/// Domain Service for pricing calculations.
/// Used when business logic doesn't naturally fit within a single aggregate.
/// 
/// Key DDD Pattern: Domain Service
/// - Stateless
/// - Contains domain logic that doesn't belong to a specific entity
/// - Works with domain objects (not DTOs or entities)
/// </summary>
public sealed class PricingService
{
    /// <summary>
    /// Calculate discount based on customer level and order subtotal.
    /// Business rules:
    /// - Regular: No discount
    /// - Silver: 5% discount
    /// - Gold: 10% discount
    /// </summary>
    public Money CalculateDiscount(CustomerLevel customerLevel, Money subtotal)
    {
        var discountPercentage = customerLevel switch
        {
            CustomerLevel.Regular => 0m,
            CustomerLevel.Silver => 0.05m,
            CustomerLevel.Gold => 0.10m,
            _ => throw new ArgumentException($"Unknown customer level: {customerLevel}", nameof(customerLevel))
        };

        var discountAmount = subtotal.Amount * discountPercentage;
        return new Money(discountAmount, subtotal.Currency);
    }
}

/// <summary>
/// Enum representing customer loyalty levels.
/// Affects discount eligibility.
/// </summary>
public enum CustomerLevel
{
    Regular,
    Silver,
    Gold
}
