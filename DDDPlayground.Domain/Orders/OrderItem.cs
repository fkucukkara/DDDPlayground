using DDDPlayground.Domain.Exceptions;
using DDDPlayground.Domain.ValueObjects;

namespace DDDPlayground.Domain.Orders;

/// <summary>
/// Entity within the Order aggregate representing a line item.
/// Not an aggregate root - can only be accessed through Order.
/// </summary>
public sealed class OrderItem
{
    // Private setter enforces encapsulation
    public ProductId ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }

    // Calculated property
    public Money LineTotal => UnitPrice * Quantity;

    // Private constructor - can only be created through factory method
    private OrderItem(ProductId productId, int quantity, Money unitPrice)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    /// <summary>
    /// Factory method enforcing business rules.
    /// </summary>
    public static OrderItem Create(ProductId productId, int quantity, Money unitPrice)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero");
        }

        if (unitPrice.Amount <= 0)
        {
            throw new DomainException("Unit price must be greater than zero");
        }

        return new OrderItem(productId, quantity, unitPrice);
    }

    /// <summary>
    /// Business behavior: Update quantity.
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero");
        }

        Quantity = newQuantity;
    }
}
