---
description: 'DDD Playground - Educational showcase demonstrating DDD, Clean Architecture, and .NET Aspire'
applyTo: '**/*'
---

# DDD Playground Project Guide

This is an **educational showcase** of Domain-Driven Design (DDD) tactical patterns, Clean Architecture, and .NET Aspire. Focus: simplified order management system demonstrating key concepts through working code.

## Architecture & Layer Rules

### Clean Architecture Layers (Strict Dependency Flow)

```
Domain (core) → Application → Infrastructure → ApiService (presentation)
```

**CRITICAL: Domain layer has ZERO dependencies** - no EF Core, no MediatR, no external libraries.

1. **Domain** (`DDDPlayground.Domain/`) - Pure business logic
   - Aggregates (`Order`, `OrderItem`), Value Objects (`Money`, `OrderId`, `CustomerId`)
   - Domain Services (`PricingService`), Domain Events (`OrderCreatedEvent`)
   - Repository interfaces (`IOrderRepository`) - implemented in Infrastructure
   - NO persistence attributes, NO EF annotations, NO data access code

2. **Application** (`DDDPlayground.Application/`) - Use case orchestration
   - CQRS: Commands (`CreateOrderCommand`) for writes, Queries (`GetOrderQuery`) for reads
   - MediatR handlers coordinate workflows: validate → call domain → persist → publish events
   - Response DTOs (`OrderResponse`) with manual `FromDomain()` mapping

3. **Infrastructure** (`DDDPlayground.Infrastructure/`) - Persistence implementation
   - **Separate persistence entities** (`OrderEntity`, `OrderItemEntity`) - NOT domain models
   - Manual mappers (`OrderMapper.ToEntity()` / `ToDomain()`) - NO AutoMapper
   - Repository implementations (`OrderRepository`) - use EF Core internally, expose domain models
   - `AppDbContext` with `DbSet<OrderEntity>` (not `DbSet<Order>`)

4. **ApiService** (`DDDPlayground.ApiService/`) - HTTP endpoints
   - Minimal APIs with `MapGroup()`, `TypedResults`, static handlers
   - Request DTOs (`CreateOrderRequest`) prevent over-posting
   - Sends commands/queries via MediatR `ISender`

### Three-Model Separation Pattern

1. **Domain Models** - Rich business logic (`Order.cs` with private setters, business methods)
2. **Persistence Models** - EF Core entities (`OrderEntity.cs` optimized for database)
3. **API Models** - Request/Response DTOs (`CreateOrderRequest.cs`, `OrderResponse.cs`)

**Never mix these layers.** Map explicitly at boundaries.

## Running & Development Workflow

### Run Application (Always use Aspire AppHost)

```powershell
# From solution root - starts PostgreSQL container + API service
dotnet run --project src/DDDPlayground.AppHost/DDDPlayground.AppHost.csproj
```

Or use VS Code task: `Run Aspire AppHost`

Access:
- Aspire Dashboard: `http://localhost:15000` (traces, metrics, logs, pgAdmin)
- API Swagger: `https://localhost:7001/openapi`
- API endpoints: `https://localhost:7001/api/orders`

### Database Migrations

Database auto-migrates on startup in Development (see `Program.cs`). For manual migrations:

```powershell
# From solution root
dotnet ef migrations add <Name> --project src/DDDPlayground.Infrastructure --startup-project src/DDDPlayground.ApiService
dotnet ef database update --project src/DDDPlayground.Infrastructure --startup-project src/DDDPlayground.ApiService
```

**Migrations live in Infrastructure** - they reference `OrderEntity`, not `Order`.

## DDD Patterns Implementation Guide

### Aggregate Root Pattern (`Order.cs`)

```csharp
// ✅ Correct: Private constructor + Factory method
private Order() { } // EF Core reconstitution only
public static Order Create(CustomerId customerId, IEnumerable<OrderItem> items)
{
    // Enforce invariants at creation
    if (!items.Any()) throw new DomainException("Order must have items");
    // ... create and return
}
```

- Control all access to child entities (`OrderItem`) through aggregate root
- Private setters, expose `IReadOnlyList<OrderItem>` to prevent external modification
- Business methods: `Confirm()`, `Cancel()`, `AddItem()` - NOT property setters
- Raise domain events: `RaiseDomainEvent(new OrderConfirmedEvent(...))`

### Value Objects Pattern (`Money.cs`, `OrderId.cs`)

```csharp
// ✅ Use record for value equality and immutability
public sealed record Money(decimal Amount, string Currency)
{
    // Operators for domain operations
    public static Money operator +(Money left, Money right) { ... }
}
```

- Immutable (`record` or `{ get; init; }`), validate in constructor
- Value equality (not reference), use `sealed record`
- Encapsulate validation: `new Money(-10, "USD")` throws exception

### Repository Pattern

```csharp
// Domain layer: IOrderRepository (works with domain Order)
Task<Order?> GetByIdAsync(OrderId id);
Task AddAsync(Order order);

// Infrastructure: OrderRepository implementation
public async Task<Order?> GetByIdAsync(OrderId id)
{
    var entity = await _context.Orders.Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == id.Value);
    return entity is null ? null : OrderMapper.ToDomain(entity); // Map!
}
```

- Interface in Domain, implementation in Infrastructure
- Public API uses domain models (`Order`), internal uses persistence models (`OrderEntity`)
- Aggregate-focused methods (not generic `IRepository<T>`)

### Domain Events Pattern

```csharp
// 1. Define in Domain/Events/
public sealed record OrderConfirmedEvent(OrderId OrderId, DateTime ConfirmedAt) : IDomainEvent;

// 2. Raise in aggregate
protected void RaiseDomainEvent(IDomainEvent domainEvent) { _domainEvents.Add(domainEvent); }

// 3. Publish after persistence (Application layer handler)
await _unitOfWork.SaveChangesAsync();
await _domainEventPublisher.PublishDomainEventsAsync(order);

// 4. Handle via MediatR notification (Application/Orders/EventHandlers/)
public class OrderConfirmedEventHandler : INotificationHandler<OrderConfirmedNotification> { ... }
```

Events raised during business operations, dispatched AFTER successful persistence.

### CQRS Lite with MediatR

```csharp
// Commands (writes) - Application/Orders/CreateOrder/
public sealed record CreateOrderCommand : IRequest<OrderResponse> { ... }
public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // 1. Map DTO → Domain
        var order = Order.Create(customerId, orderItems);
        // 2. Persist
        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
        // 3. Publish events
        await _domainEventPublisher.PublishDomainEventsAsync(order);
        // 4. Map Domain → Response DTO
        return OrderResponse.FromDomain(order);
    }
}

// Queries (reads) - Application/Orders/GetOrder/
public sealed record GetOrderQuery(Guid OrderId) : IRequest<OrderResponse?>;
```

Organize by feature folder: `CreateOrder/`, `ConfirmOrder/`, `GetOrder/`.

### Minimal API Endpoints

```csharp
// ApiService/Endpoints/OrderEndpoints.cs
public static RouteGroupBuilder MapOrderEndpoints(this IEndpointRouteBuilder routes)
{
    var group = routes.MapGroup("/api/orders").WithTags("Orders");
    
    group.MapPost("/", CreateOrderAsync)
        .Produces<OrderResponse>(201)
        .Produces<ProblemDetails>(400);
    
    return group;
}

// Static handler for performance
private static async Task<Results<Created<OrderResponse>, BadRequest<ProblemDetails>>> 
    CreateOrderAsync(CreateOrderRequest request, ISender sender, CancellationToken ct)
{
    var command = new CreateOrderCommand { ... }; // Map request → command
    var response = await sender.Send(command, ct);
    return TypedResults.Created($"/api/orders/{response.Id}", response);
}
```

Use `TypedResults` (not `Results.Ok()`), static handlers, `MapGroup()` for organization.

## Adding New Features (Step-by-Step)

### Example: Add "Ship Order" Feature

1. **Domain**: Add business logic to `Order.cs`
   ```csharp
   public void Ship()
   {
       if (Status != OrderStatus.Confirmed) throw new DomainException("Only confirmed orders can ship");
       Status = OrderStatus.Shipped;
       RaiseDomainEvent(new OrderShippedEvent(Id, DateTime.UtcNow));
   }
   ```

2. **Domain Events**: Create `OrderShippedEvent.cs` in `Domain/Events/`

3. **Application Command**: Create `ShipOrder/ShipOrderCommand.cs` and `ShipOrderHandler.cs`
   ```csharp
   public sealed record ShipOrderCommand(Guid OrderId) : IRequest<OrderResponse>;
   
   public sealed class ShipOrderHandler : IRequestHandler<ShipOrderCommand, OrderResponse>
   {
       public async Task<OrderResponse> Handle(ShipOrderCommand request, CancellationToken ct)
       {
           var order = await _orderRepository.GetByIdAsync(new OrderId(request.OrderId));
           order.Ship(); // Business logic
           await _orderRepository.UpdateAsync(order);
           await _unitOfWork.SaveChangesAsync();
           await _domainEventPublisher.PublishDomainEventsAsync(order);
           return OrderResponse.FromDomain(order);
       }
   }
   ```

4. **API Endpoint**: Add to `OrderEndpoints.cs`
   ```csharp
   group.MapPost("/{id:guid}/ship", ShipOrderAsync);
   ```

5. **Event Handler** (optional): Create `OrderShippedEventHandler.cs` for side effects

6. **Migration**: If persistence changes needed, add migration and update `OrderEntity`/configurations

## Common Mistakes to Avoid

❌ **DON'T** add EF Core attributes to domain models (`Order.cs`)
✅ **DO** use separate `OrderEntity` with EF configuration in Infrastructure

❌ **DON'T** use `AutoMapper` - manual mapping is intentional for education
✅ **DO** write explicit `ToDomain()` / `ToEntity()` methods in `Mappers/`

❌ **DON'T** reference Infrastructure from Domain
✅ **DO** define interfaces in Domain, implement in Infrastructure

❌ **DON'T** expose `List<OrderItem>` with public setter
✅ **DO** expose `IReadOnlyList<OrderItem>` with methods like `AddItem()`

❌ **DON'T** run API service directly - Docker/PostgreSQL won't be available
✅ **DO** run via Aspire AppHost (handles service orchestration)

❌ **DON'T** put business logic in handlers or controllers
✅ **DO** put business logic in domain aggregates and services

## Technology Stack

- **.NET 10.0** (requires SDK 10.0.100+, see `global.json`)
- **.NET Aspire 13.0** (orchestration, observability)
- **PostgreSQL** (Aspire-hosted container)
- **EF Core 10.0** (with separate persistence models)
- **MediatR 13.1** (CQRS commands/queries)
- **Minimal APIs** (ASP.NET Core)

## Key Files Reference

| File | Purpose |
|------|---------|
| `Domain/Orders/Order.cs` | Aggregate root - business rules, factory methods, encapsulation |
| `Infrastructure/Persistence/Models/OrderEntity.cs` | Separate EF entity for database |
| `Infrastructure/Persistence/Mappers/OrderMapper.cs` | Manual domain ↔ persistence mapping |
| `Application/Orders/CreateOrder/CreateOrderHandler.cs` | CQRS command handler pattern |
| `ApiService/Endpoints/OrderEndpoints.cs` | Minimal API with MapGroup/TypedResults |
| `AppHost/AppHost.cs` | Aspire orchestration - adds PostgreSQL + API service |
| `ApiService/Program.cs` | API setup - Aspire, DbContext, MediatR registration |

## Educational Focus

This project demonstrates **correct patterns through working code**, not just documentation. When adding features:

1. Follow existing patterns (study similar features first)
2. Maintain strict layer separation (Domain → Application → Infrastructure → API)
3. Write XML doc comments explaining the "why" for patterns
4. Prefer clarity over cleverness - this is for learning