# DDD Showcase API - MVP Document

## Project Overview

An **educational REST API** demonstrating Domain-Driven Design (DDD) patterns, Clean Architecture, and best practices vs. anti-patterns in .NET with **.NET Aspire**. Features a **simplified order management system** - complex enough to show key concepts, simple enough to grasp quickly.

---

## Goals

### Primary Goals
1. **DDD Tactical Patterns**: Entities, Value Objects, Aggregates, Domain Services, Repositories, Domain Events
2. **Clean Architecture**: Clear layer separation (Domain → Application → Infrastructure → Presentation)
3. **Patterns vs. Anti-Patterns**: Side-by-side examples of correct vs. incorrect implementations
4. **Domain/Persistence Separation**: Persistence-ignorant domain models with EF Core adapters
5. **Modern Cloud-Native**: .NET Aspire for observability, service discovery, and orchestration

### Educational Focus
- Keep examples concise and focused
- Clear inline documentation explaining patterns
- Quick setup with Aspire tooling
- Practical reference for learning teams

---

## Domain: Order Management System

### Bounded Context
**Sales Context** - Managing customer orders from creation to fulfillment

### Core Use Cases (Concise MVP)
1. **Create Order** - Place order with items
2. **Add/Remove Items** - Modify draft orders only
3. **Confirm Order** - Finalize order (Draft → Confirmed)
4. **Cancel Order** - Cancel before shipping
5. **Get Order Details** - Retrieve order info

### Out of Scope
- Payment processing, inventory, auth, reporting, multiple addresses

---

## Ubiquitous Language

| Term | Definition |
|------|------------|
| **Order** | A customer's request to purchase products |
| **Order Item** | A line item in an order (product + quantity) |
| **Draft Order** | Newly created order, can be modified |
| **Confirmed Order** | Order finalized by customer, awaiting shipment |
| **Shipped Order** | Order dispatched to customer |
| **Cancelled Order** | Order terminated before shipping |
| **Customer** | Person placing orders (simplified: just ID and level) |
| **Product** | Item available for purchase |
| **Discount** | Price reduction based on customer level or promotion |
| **Money** | Amount with currency (Value Object) |

---

## Domain Model

### Aggregates

#### **Order Aggregate** (Root: Order)
```
Order (Aggregate Root)
├── OrderId (Value Object)
├── CustomerId (Value Object/Reference)
├── OrderStatus (Enum: Draft, Confirmed, Shipped, Cancelled)
├── OrderItems (Collection - Part of Aggregate)
│   ├── OrderItem (Entity)
│   │   ├── Product Reference
│   │   ├── Quantity
│   │   └── Price at time of order
├── Subtotal (Money - Value Object)
├── DiscountAmount (Money - Value Object)
├── Total (Money - Value Object)
├── CreatedAt (DateTime)
└── ConfirmedAt (DateTime?)

Business Rules:
- Only Draft orders can be modified
- Orders must have at least 1 item to be confirmed
- Cancelled orders cannot be modified
- Shipped orders cannot be cancelled
- Total = Subtotal - Discount
```

#### **Customer Aggregate** (Root: Customer)
```
Customer (Aggregate Root)
├── CustomerId (Value Object)
├── Email (Value Object)
├── CustomerLevel (Enum: Regular, Silver, Gold)
└── RegistrationDate (DateTime)

Business Rules:
- Email must be unique and valid
- Customer level determines discount eligibility
```

#### **Product Aggregate** (Root: Product) - Simplified
```
Product (Aggregate Root)
├── ProductId (Value Object)
├── Name
├── Price (Money - Value Object)
└── IsAvailable (bool)

Business Rules:
- Price must be positive
- Unavailable products cannot be added to orders
```

### Value Objects
- **Money**: Amount + Currency (immutable)
- **OrderId, CustomerId, ProductId**: Strongly-typed IDs
- **Email**: Validated email address

### Domain Services
- **PricingService**: Calculate discounts based on customer level and order total
- **OrderConfirmationService**: Complex validation logic involving multiple aggregates

### Domain Events (In-Memory Only)
- **OrderConfirmedEvent** - Raised when order is confirmed (educational example)
- Events dispatched in-memory, not persisted
- Shows pattern without event sourcing complexity

---

## Architecture Layers

### Layer Structure (with .NET Aspire)
```
DDDPlayground/
├── DDDPlayground.AppHost/                # Aspire orchestration
│   └── Program.cs
│
├── DDDPlayground.ServiceDefaults/        # Aspire shared config
│   └── Extensions.cs
│
├── DDDPlayground.Domain/                 # Pure business logic (zero dependencies)
│   ├── Orders/
│   │   ├── Order.cs                      # Aggregate root
│   │   ├── OrderItem.cs                  # Entity
│   │   ├── OrderStatus.cs                # Enum
│   │   └── IOrderRepository.cs           # Interface only
│   ├── ValueObjects/
│   │   ├── Money.cs
│   │   ├── OrderId.cs
│   │   └── Email.cs
│   ├── Services/
│   │   └── PricingService.cs             # Domain service
│   ├── Events/
│   │   └── OrderConfirmedEvent.cs
│   └── Exceptions/
│       └── DomainException.cs
│
├── DDDPlayground.Application/            # Use cases (CQRS)
│   ├── Orders/
│   │   ├── CreateOrder/
│   │   │   ├── CreateOrderCommand.cs
│   │   │   └── CreateOrderHandler.cs
│   │   ├── ConfirmOrder/
│   │   └── GetOrder/
│   │       ├── GetOrderQuery.cs
│   │       ├── GetOrderHandler.cs
│   │       └── OrderResponse.cs          # Manual mapping (no AutoMapper)
│   └── Common/
│       └── IUnitOfWork.cs
│
├── DDDPlayground.Infrastructure/         # EF Core, repositories
│   ├── Persistence/
│   │   ├── Models/                       # Separate persistence models
│   │   │   ├── OrderEntity.cs            # EF Core entity
│   │   │   ├── OrderItemEntity.cs        # EF Core entity
│   │   │   └── CustomerEntity.cs
│   │   ├── Mappers/                      # Manual domain ↔ persistence
│   │   │   ├── OrderMapper.cs
│   │   │   └── CustomerMapper.cs
│   │   ├── AppDbContext.cs               # Uses *Entity classes
│   │   ├── Configurations/
│   │   │   └── OrderEntityConfiguration.cs
│   │   └── Repositories/
│   │       └── OrderRepository.cs        # Maps between layers
│   └── DependencyInjection.cs
│
├── DDDPlayground.ApiService/             # Minimal API
│   ├── Endpoints/
│   │   ├── OrderEndpoints.cs             # MapGroup + handlers
│   │   └── EndpointExtensions.cs         # Registration helpers
│   ├── Models/
│   │   └── OrderRequest.cs               # API request/response DTOs
│   ├── Program.cs                        # Aspire + Minimal API setup
│   └── appsettings.json
│
└── DDDPlayground.Domain.Tests/           # Unit tests
    └── OrderTests.cs
```

---

## API Endpoints (Minimal API Style)

### Orders Group - `/orders`
```csharp
var orders = app.MapGroup("/orders")
    .WithTags("Orders")
    .WithOpenApi();

orders.MapPost("/", CreateOrder);                    // Create new order
orders.MapGet("/{id}", GetOrder);                    // Get order details
orders.MapPost("/{id}/items", AddOrderItem);         // Add item to order
orders.MapDelete("/{id}/items/{itemId}", RemoveOrderItem);
orders.MapPost("/{id}/confirm", ConfirmOrder);       // Confirm order
orders.MapPost("/{id}/cancel", CancelOrder);         // Cancel order
```

### Benefits of Minimal API Approach
- **Less ceremony**: No controllers, direct route handlers
- **Type-safe**: TypedResults for compile-time safety
- **Testable**: Handlers are static methods, easy to unit test
- **Performance**: Reduced allocations, faster startup
- **Clean separation**: Handlers delegate to MediatR (Application layer)

---

## Patterns to Demonstrate

### ✅ Good Patterns (Primary Implementation)

1. **Rich Domain Model**
   - Encapsulated state with private setters
   - Business logic in domain entities
   - Validation in constructors/methods

2. **Aggregate Pattern**
   - Order as aggregate root controlling OrderItems
   - Consistency boundary enforced
   - Repository access only through root

3. **Value Objects**
   - Money with currency
   - Strongly-typed IDs
   - Email validation

4. **Domain Events (In-Memory)**
   - OrderConfirmedEvent raised by aggregate
   - MediatR notifications for in-process side effects
   - Educational demo without persistence complexity

5. **Repository Pattern**
   - Interface in domain, implementation in infrastructure
   - Aggregate-focused methods (not generic CRUD)

6. **Persistence Ignorance with Separation**
   - Domain models: Pure business logic, no persistence attributes
   - Persistence models: Separate EF Core entities in Infrastructure
   - Manual mappers: Explicit mapping between domain ↔ persistence
   - Shows true separation (not just hiding with Fluent API)

7. **CQRS Lite**
   - Commands for writes (CreateOrderCommand)
   - Queries for reads (GetOrderByIdQuery)
   - DTOs for API responses

8. **Unit of Work**
   - Transaction boundary per use case
   - SaveChangesAsync called once per request

### ❌ Anti-Patterns (Separate Branch/Examples)

Create a separate folder `AntiPatterns/` showing:

1. **Anemic Domain Model**
   - Public setters everywhere
   - All logic in services
   - Example: `AnemicOrder.cs`

2. **Domain Models with EF Attributes**
   - `[Key]`, `[Column]`, `[Required]` on domain classes
   - Persistence concerns polluting business logic
   - Example: `OrderWithEFAttributes.cs`

3. **No Separation - Single Model**
   - Using EF entities directly in business logic
   - Domain logic coupled to persistence
   - Example: `OrderEntityAsAggregate.cs`

4. **Wrong Aggregate Boundaries**
   - Customer containing all Orders
   - Example: `CustomerWithAllOrders.cs`

5. **Generic Repository**
   - `IRepository<T>` exposing IQueryable
   - Example: `GenericRepository.cs`

6. **Technical Concerns in Domain**
   - Logging/email in domain methods
   - Example: `OrderWithInfrastructure.cs`

---

## Technology Stack

### Core
- **.NET 9.0** + **ASP.NET Core Minimal APIs**
- **.NET Aspire** - Orchestration, observability, service defaults

### Persistence
- **Entity Framework Core 9.0**
- **PostgreSQL** (via Aspire hosting)
- **In-Memory Database** for tests

### Libraries
- **MediatR** - CQRS commands/queries
- **FluentValidation** - Input validation
- **Manual DTO Mapping** - No AutoMapper (educational clarity)
- **OpenTelemetry** - Distributed tracing (Aspire built-in)
- **Scalar/OpenAPI** - Modern API documentation

### Minimal API Patterns
- **MapGroup** - Organize related endpoints with common prefixes
- **TypedResults** - Type-safe responses for better testability
- **Route handlers** - Extracted methods (not lambdas) for unit testing
- **DTO pattern** - Prevent over-posting, separate domain from API concerns

### Testing
- **xUnit** - Test framework
- **FluentAssertions** - Readable assertions

---

## Key Features

### 1. Minimal API Best Practices
- **MapGroup** for endpoint organization
- **TypedResults** for type-safe responses
- **Static handlers** extracted for testability
- **OpenAPI/Scalar** for documentation

### 2. Validation
- Domain validation in aggregates
- FluentValidation for input DTOs
- Custom domain exceptions

### 3. Error Handling
- Exception handling middleware
- Problem Details (RFC 7807) responses
- Domain exceptions → HTTP status codes

### 4. Testing
- Unit tests for domain logic
- Handler tests (testable static methods)
- Integration tests with WebApplicationFactory

### 5. Observability (Aspire)
- OpenTelemetry traces
- Metrics dashboard
- Structured logging

---

## Database Schema

```sql
-- Orders Table
CREATE TABLE Orders (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    Status VARCHAR(20) NOT NULL,
    SubtotalAmount DECIMAL(18,2) NOT NULL,
    SubtotalCurrency VARCHAR(3) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL,
    DiscountCurrency VARCHAR(3) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    TotalCurrency VARCHAR(3) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    ConfirmedAt DATETIME2 NULL
);

-- OrderItems Table
CREATE TABLE OrderItems (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    OrderId UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Quantity INT NOT NULL,
    UnitPriceAmount DECIMAL(18,2) NOT NULL,
    UnitPriceCurrency VARCHAR(3) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id)
);

-- Customers Table
CREATE TABLE Customers (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    CustomerLevel VARCHAR(20) NOT NULL,
    RegistrationDate DATETIME2 NOT NULL
);

-- Products Table
CREATE TABLE Products (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    PriceAmount DECIMAL(18,2) NOT NULL,
    PriceCurrency VARCHAR(3) NOT NULL,
    IsAvailable BIT NOT NULL
);
```

---

## Implementation Phases (Concise)

### Phase 1: Foundation
- [ ] Aspire AppHost + ServiceDefaults setup
- [ ] Domain layer: Order aggregate, Value Objects
- [ ] Unit tests for Order aggregate
- [ ] Anti-pattern examples (Anemic model)

### Phase 2: Application + Infrastructure
- [ ] MediatR handlers (Create, Confirm, GetOrder)
- [ ] EF Core configuration (persistence-ignorant mapping)
- [ ] PostgreSQL with Aspire
- [ ] Manual DTO mapping examples

### Phase 3: API + Observability
- [ ] REST endpoints
- [ ] Aspire dashboard (metrics, traces, logs)
- [ ] Error handling
- [ ] Inline documentation

### Phase 4: Educational Polish
- [ ] README with architecture diagrams
- [ ] Pattern vs. Anti-Pattern comparisons
- [ ] Code comments explaining DDD decisions

---

## Success Criteria

### Technical
- ✅ All layers properly separated with correct dependencies
- ✅ Domain layer has zero infrastructure dependencies
- ✅ >80% test coverage on domain logic
- ✅ All API endpoints functional with proper validation
- ✅ EF Core successfully persists rich domain models
- ✅ Domain events properly dispatched

### Educational
- ✅ Code is self-documenting with clear naming
- ✅ Comments explain "why" not "what"
- ✅ README includes architecture diagrams
- ✅ Anti-pattern examples clearly show what NOT to do
- ✅ Can be used as reference for team training

---

## Sample Implementation: Three Model Layers

### 1. Domain Model (Pure Business Logic)
```csharp
// DDDPlayground.Domain/Orders/Order.cs
public sealed class Order  // No EF attributes, no persistence concerns
{
    private readonly List<OrderItem> _items = new();
    
    public OrderId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public Money Total { get; private set; }
    
    // Factory method enforces business rules
    public static Order Create(CustomerId customerId, IEnumerable<OrderItem> items)
    {
        // Business validation here
        return new Order(OrderId.New(), customerId, items);
    }
    
    // Business behavior
    public void Confirm()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Only draft orders can be confirmed");
        
        Status = OrderStatus.Confirmed;
        // Raise domain event
    }
}
```

### 2. Persistence Model (Infrastructure)
```csharp
// DDDPlayground.Infrastructure/Persistence/Models/OrderEntity.cs
public class OrderEntity  // EF Core entity - different class!
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string TotalCurrency { get; set; } = "USD";
    public DateTime CreatedAt { get; set; }
    
    // EF navigation properties
    public List<OrderItemEntity> Items { get; set; } = new();
}

// DDDPlayground.Infrastructure/Persistence/Mappers/OrderMapper.cs
public static class OrderMapper
{
    // Domain → Persistence
    public static OrderEntity ToEntity(Order order)
    {
        return new OrderEntity
        {
            Id = order.Id.Value,
            CustomerId = order.CustomerId.Value,
            Status = order.Status.ToString(),
            TotalAmount = order.Total.Amount,
            TotalCurrency = order.Total.Currency,
            Items = order.Items.Select(OrderItemMapper.ToEntity).ToList()
        };
    }
    
    // Persistence → Domain
    public static Order ToDomain(OrderEntity entity)
    {
        // Reconstruct domain aggregate from persistence
        var items = entity.Items.Select(OrderItemMapper.ToDomain).ToList();
        return Order.Reconstitute(
            new OrderId(entity.Id),
            new CustomerId(entity.CustomerId),
            Enum.Parse<OrderStatus>(entity.Status),
            items,
            new Money(entity.TotalAmount, entity.TotalCurrency)
        );
    }
}
```

### 3. API Model (Presentation)
```csharp
// DDDPlayground.ApiService/Models/OrderRequest.cs
public record CreateOrderRequest(Guid CustomerId, List<OrderItemRequest> Items);
public record OrderResponse(Guid Id, string Status, MoneyResponse Total);

// DDDPlayground.ApiService/Endpoints/OrderEndpoints.cs
public static class OrderEndpoints
{
    public static RouteGroupBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders").WithTags("Orders").WithOpenApi();
        group.MapPost("/", CreateOrder);
        group.MapGet("/{id}", GetOrder);
        return group;
    }

    static async Task<Results<Created<OrderResponse>, ValidationProblem>> CreateOrder(
        CreateOrderRequest request,  // API DTO
        IMediator mediator)
    {
        var command = new CreateOrderCommand(request.CustomerId, request.Items);
        var domainOrder = await mediator.Send(command);  // Returns domain model
        
        // Manual mapping: Domain → API Response
        var response = new OrderResponse(
            domainOrder.Id.Value,
            domainOrder.Status.ToString(),
            new MoneyResponse(domainOrder.Total.Amount, domainOrder.Total.Currency)
        );
        
        return TypedResults.Created($"/orders/{response.Id}", response);
    }
}
```

### Layer Separation Summary
```
API Request (OrderRequest) 
    ↓ manual mapping
Application Command (CreateOrderCommand)
    ↓ uses domain
Domain Model (Order aggregate) ← pure business logic
    ↓ repository saves
Persistence Model (OrderEntity) ← EF Core entity
    ↓ EF Core
Database
```

### Request/Response DTOs
```http
POST /orders
Content-Type: application/json

{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "items": [{ "productId": "8fa85f64-...", "quantity": 2 }]
}
```

**Response: 201 Created**
```json
{
  "id": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Draft",
  "total": { "amount": 1999.98, "currency": "USD" }
}
```

---

## Documentation Deliverables

1. **README.md** - Project overview, setup instructions
2. **ARCHITECTURE.md** - Detailed architecture explanation with diagrams
3. **PATTERNS.md** - DDD patterns used with code examples
4. **ANTI_PATTERNS.md** - Common mistakes with comparisons
5. **API.md** - Complete API documentation
6. **TESTING.md** - Testing strategy and examples

---

## Future Enhancements (Post-MVP)

- **Event Sourcing**: Persist domain events (separate advanced sample)
- **Outbox Pattern**: Reliable event publishing
- **Multiple Bounded Contexts**: Add Shipping, Inventory contexts
- **API Versioning**: Demonstrate backward compatibility
- **Authentication/Authorization**: JWT, role-based access
- **Caching**: Redis for read-side optimization
- **Message Bus**: RabbitMQ/Azure Service Bus for async communication
- **GraphQL**: Alternative API interface
- **Docker**: Containerization with docker-compose

---

## Estimated Effort (Educational MVP)

- **Development**: 2-3 weeks (focused implementation)
- **Documentation**: Inline + README
- **Total**: ~3 weeks for educational showcase

---

## Success Metrics

After completion, a developer should be able to:
1. Understand the difference between rich and anemic domain models
2. Implement proper aggregate boundaries
3. Separate domain logic from infrastructure concerns
4. Write testable domain code
5. Apply DDD patterns to their own projects
6. Recognize and avoid common anti-patterns

---

## Repository Structure

```
ddd-showcase-api/
├── README.md
├── docs/
│   ├── ARCHITECTURE.md
│   ├── PATTERNS.md
│   ├── ANTI_PATTERNS.md
│   └── diagrams/
├── src/
│   └── [layers as described above]
├── tests/
│   └── [test projects]
├── docker/
│   ├── Dockerfile
│   └── docker-compose.yml
└── scripts/
    ├── setup-db.sql
    └── seed-data.sql
```

---

## Getting Started

```powershell
# Clone repository
git clone <repo-url>

# Navigate to AppHost
cd DDDPlayground/DDDPlayground.AppHost

# Run with Aspire (starts API + PostgreSQL + Dashboard)
dotnet run

# Access:
# - API: https://localhost:7001
# - Aspire Dashboard: http://localhost:15000
# - Traces, Metrics, Logs all in dashboard
```

---

This MVP provides a comprehensive, educational demonstration of DDD principles while remaining practical and implementable. The focus is on clarity, best practices, and providing a reference implementation that teams can learn from and adapt to their needs.