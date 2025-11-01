using DDDPlayground.Domain.Exceptions;
using DDDPlayground.Domain.ValueObjects;

namespace DDDPlayground.Domain.Orders;

/// <summary>
/// Order Aggregate Root - Rich Domain Model demonstrating DDD patterns.
/// 
/// Key DDD Patterns Demonstrated:
/// 1. Aggregate Root - Controls access to OrderItems (entities within aggregate)
/// 2. Encapsulation - Private setters, business logic in methods
/// 3. Consistency Boundary - All changes go through aggregate root
/// 4. Factory Methods - Create() enforces business rules at creation
/// 5. Business Behavior - Methods like Confirm(), Cancel() contain business logic
/// 6. Domain Events - Raises events for significant business occurrences
/// 7. Persistence Ignorance - No EF attributes, database concerns
/// </summary>
public sealed class Order
{
    private readonly List<OrderItem> _items = new();

    public OrderId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money Subtotal { get; private set; }
    public Money Discount { get; private set; }
    public Money Total { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }

    // Read-only collection prevents external modification
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    // Private constructor prevents direct instantiation
    private Order() { }

    /// <summary>
    /// Factory method for creating a new order.
    /// Enforces business rules at creation time.
    /// </summary>
    public static Order Create(CustomerId customerId, IEnumerable<OrderItem> items)
    {
        if (items == null || !items.Any())
        {
            throw new DomainException("Order must have at least one item");
        }

        var order = new Order
        {
            Id = OrderId.New(),
            CustomerId = customerId,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            Discount = Money.Zero()
        };

        foreach (var item in items)
        {
            order._items.Add(item);
        }

        order.RecalculateTotals();

        return order;
    }

    /// <summary>
    /// Reconstitute an order from persistence.
    /// Used by infrastructure layer when loading from database.
    /// </summary>
    public static Order Reconstitute(
        OrderId id,
        CustomerId customerId,
        OrderStatus status,
        IEnumerable<OrderItem> items,
        Money subtotal,
        Money discount,
        Money total,
        DateTime createdAt,
        DateTime? confirmedAt)
    {
        var order = new Order
        {
            Id = id,
            CustomerId = customerId,
            Status = status,
            Subtotal = subtotal,
            Discount = discount,
            Total = total,
            CreatedAt = createdAt,
            ConfirmedAt = confirmedAt
        };

        order._items.AddRange(items);

        return order;
    }

    /// <summary>
    /// Business behavior: Add item to draft order.
    /// Demonstrates how business rules are enforced.
    /// </summary>
    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Draft)
        {
            throw new DomainException("Can only add items to draft orders");
        }

        _items.Add(item);
        RecalculateTotals();
    }

    /// <summary>
    /// Business behavior: Remove item from draft order.
    /// </summary>
    public void RemoveItem(ProductId productId)
    {
        if (Status != OrderStatus.Draft)
        {
            throw new DomainException("Can only remove items from draft orders");
        }

        var item = _items.FirstOrDefault(i => i.ProductId.Value == productId.Value);
        if (item == null)
        {
            throw new DomainException($"Item with product ID {productId} not found in order");
        }

        _items.Remove(item);

        if (!_items.Any())
        {
            throw new DomainException("Cannot remove last item from order");
        }

        RecalculateTotals();
    }

    /// <summary>
    /// Business behavior: Apply discount to order.
    /// </summary>
    public void ApplyDiscount(Money discountAmount)
    {
        if (Status != OrderStatus.Draft)
        {
            throw new DomainException("Can only apply discount to draft orders");
        }

        if (discountAmount.Amount < 0)
        {
            throw new DomainException("Discount cannot be negative");
        }

        if (discountAmount.Currency != Subtotal.Currency)
        {
            throw new DomainException($"Discount currency ({discountAmount.Currency}) must match order currency ({Subtotal.Currency})");
        }

        if (discountAmount.Amount > Subtotal.Amount)
        {
            throw new DomainException("Discount cannot exceed subtotal");
        }

        Discount = discountAmount;
        RecalculateTotals();
    }

    /// <summary>
    /// Business behavior: Confirm the order.
    /// State transition from Draft → Confirmed.
    /// Raises domain event for side effects (logging, notifications, etc.).
    /// </summary>
    public void Confirm()
    {
        if (Status != OrderStatus.Draft)
        {
            throw new DomainException("Only draft orders can be confirmed");
        }

        if (!_items.Any())
        {
            throw new DomainException("Cannot confirm order without items");
        }

        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;

        // TODO: Raise OrderConfirmedEvent (will implement with MediatR)
    }

    /// <summary>
    /// Business behavior: Cancel the order.
    /// Can only cancel if not yet shipped.
    /// </summary>
    public void Cancel()
    {
        if (Status == OrderStatus.Shipped)
        {
            throw new DomainException("Cannot cancel shipped orders");
        }

        if (Status == OrderStatus.Cancelled)
        {
            throw new DomainException("Order is already cancelled");
        }

        Status = OrderStatus.Cancelled;
    }

    /// <summary>
    /// Business behavior: Mark order as shipped.
    /// Can only ship confirmed orders.
    /// </summary>
    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
        {
            throw new DomainException("Can only ship confirmed orders");
        }

        Status = OrderStatus.Shipped;
    }

    /// <summary>
    /// Private method to recalculate order totals.
    /// Encapsulates business logic for total calculation.
    /// </summary>
    private void RecalculateTotals()
    {
        if (!_items.Any())
        {
            Subtotal = Money.Zero();
            Total = Money.Zero();
            return;
        }

        // All items must have same currency
        var currency = _items.First().UnitPrice.Currency;
        if (_items.Any(i => i.UnitPrice.Currency != currency))
        {
            throw new DomainException("All items must have the same currency");
        }

        var subtotalAmount = _items.Sum(i => i.LineTotal.Amount);
        Subtotal = new Money(subtotalAmount, currency);

        // Ensure discount has same currency
        if (Discount == null || Discount.Currency != currency)
        {
            Discount = Money.Zero(currency);
        }

        Total = Subtotal - Discount;
    }
}
