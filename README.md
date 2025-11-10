# DDD Playground - Educational Showcase API

> An educational REST API demonstrating **Domain-Driven Design (DDD)** patterns, **Clean Architecture**, and **.NET best practices** using **.NET 9** and **.NET Aspire**.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![Aspire](https://img.shields.io/badge/.NET%20Aspire-9.5-512BD4)](https://learn.microsoft.com/dotnet/aspire/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## 📋 Table of Contents

- [Overview](#-overview)
- [Architecture](#️-architecture)
- [Key Patterns Demonstrated](#-key-patterns-demonstrated)
- [Technology Stack](#️-technology-stack)
- [Getting Started](#-getting-started)
- [Project Structure](#-project-structure)
- [API Endpoints](#-api-endpoints)
- [Learning Resources](#-learning-resources)

## 🎯 Overview

This project is an **educational showcase** of DDD tactical patterns, Clean Architecture principles, and modern .NET development practices. It implements a simplified **order management system** - complex enough to demonstrate key concepts, simple enough to understand quickly.

### Goals

- ✅ **DDD Tactical Patterns**: Aggregates, Entities, Value Objects, Domain Services, Repository Pattern, Domain Events
- ✅ **Clean Architecture**: Strict layer separation with proper dependency flow
- ✅ **Persistence Ignorance**: Domain models completely separated from EF Core entities
- ✅ **CQRS Lite**: Commands for writes, Queries for reads using MediatR
- ✅ **Minimal APIs**: Modern ASP.NET Core endpoints with MapGroup and TypedResults
- ✅ **.NET Aspire**: Cloud-native orchestration, observability, and developer experience

## 🏗️ Architecture

### Layer Overview

```text
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│              (DDDPlayground.ApiService)                  │
│         Minimal APIs • OpenAPI • HTTP Endpoints          │
└────────────────────┬────────────────────────────────────┘
                     │ depends on
┌────────────────────▼────────────────────────────────────┐
│                   Application Layer                      │
│              (DDDPlayground.Application)                 │
│       CQRS Handlers • DTOs • Use Case Orchestration      │
└────────────────────┬────────────────────────────────────┘
                     │ depends on
┌────────────────────▼────────────────────────────────────┐
│                     Domain Layer                         │
│                (DDDPlayground.Domain)                    │
│    Aggregates • Entities • Value Objects • Services      │
│              (Zero Infrastructure Dependencies)           │
└──────────────────────────────────────────────────────────┘
                     ▲
                     │ implements interfaces
┌────────────────────┴────────────────────────────────────┐
│                 Infrastructure Layer                     │
│             (DDDPlayground.Infrastructure)               │
│  EF Core • PostgreSQL • Repositories • Persistence Models│
└──────────────────────────────────────────────────────────┘
```

### Three-Model Layer Separation

This project demonstrates **true domain/persistence separation** with three distinct model layers:

1. **Domain Models** (`DDDPlayground.Domain`)
   - Pure business logic, zero persistence attributes
   - Rich domain model with encapsulation
   - Example: `Order` aggregate with private setters and business methods

2. **Persistence Models** (`DDDPlayground.Infrastructure/Persistence/Models`)
   - Separate EF Core entities (e.g., `OrderEntity`, `OrderItemEntity`)
   - Optimized for database storage
   - Different class hierarchy from domain

3. **API Models** (`DDDPlayground.ApiService/Models`)
   - Request/Response DTOs
   - Optimized for HTTP communication
   - Prevent over-posting

**Manual Mappers** in `DDDPlayground.Infrastructure/Persistence/Mappers` handle explicit transformation between layers.

## 🎓 Key Patterns Demonstrated

### Domain-Driven Design

- **Aggregate Root**: `Order` controls access to `OrderItem` entities
- **Value Objects**: `Money`, `OrderId`, `CustomerId` with immutability and value equality
- **Domain Services**: `PricingService` for discount calculation logic
- **Domain Events**: `OrderConfirmedEvent` for in-memory event dispatching
- **Repository Pattern**: `IOrderRepository` interface in domain, implementation in infrastructure

### Clean Architecture

- **Dependency Inversion**: Domain layer has zero dependencies
- **Persistence Ignorance**: No EF Core attributes in domain models
- **Manual Mapping**: Explicit `ToDomain()` and `ToEntity()` methods (no AutoMapper)
- **Use Case Orchestration**: MediatR handlers coordinate workflows

### Modern .NET Practices

- **Minimal APIs**: `MapGroup()`, `TypedResults`, static handlers for performance
- **CQRS Lite**: Separate commands (`CreateOrderCommand`) and queries (`GetOrderQuery`)
- **Unit of Work**: Transaction boundaries per use case
- **.NET Aspire**: Orchestration, PostgreSQL hosting, observability dashboard

## 🛠️ Technology Stack

| Category | Technology |
|----------|-----------|
| **Framework** | .NET 9.0 |
| **Orchestration** | .NET Aspire 9.5 |
| **API** | ASP.NET Core Minimal APIs |
| **Database** | PostgreSQL (Aspire-hosted) |
| **ORM** | Entity Framework Core 9.0 |
| **CQRS** | MediatR 13.1 |
| **Validation** | FluentValidation 12.0 |
| **Documentation** | OpenAPI/Swagger |

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (or later)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for PostgreSQL container)
- [Visual Studio 2022 17.12+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with C# extension
- [.NET Aspire workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling) installed

### Install .NET Aspire Workload

```powershell
dotnet workload update
dotnet workload install aspire
```

### Running the Application

#### Option 1: Visual Studio

1. Open `DDDPlayground.sln`
2. Set `DDDPlayground.AppHost` as the startup project
3. Press `F5` or click "Run"

#### Option 2: Command Line

```powershell
# Navigate to AppHost project
cd DDDPlayground.AppHost

# Run the application
dotnet run
```

### Accessing the Application

Once running, you'll see:

- **Aspire Dashboard**: <http://localhost:15000>
  - View traces, metrics, logs
  - Monitor service health
  - Access PostgreSQL admin (pgAdmin)

- **API Service**: <https://localhost:7001> (or port shown in console)
  - Swagger UI: <https://localhost:7001/openapi>
  - API endpoints: <https://localhost:7001/api/orders>

### Database Migrations

The database is automatically created on first run. If you need to manage migrations manually:

```powershell
# Add a new migration
dotnet ef migrations add <MigrationName> --project DDDPlayground.Infrastructure

# Update database
dotnet ef database update --project DDDPlayground.Infrastructure --startup-project DDDPlayground.ApiService
```

## 📁 Project Structure

```text
DDDPlayground/
├── .github/                          # GitHub Actions, issue templates
├── DDDPlayground.AppHost/            # Aspire orchestration host
│   └── Program.cs                    # Configures PostgreSQL, Redis, API service
├── DDDPlayground.ServiceDefaults/    # Aspire shared configuration
│   └── Extensions.cs                 # Service defaults (telemetry, health checks)
├── DDDPlayground.Domain/             # Pure domain logic (ZERO dependencies)
│   ├── Orders/
│   │   ├── Order.cs                  # Aggregate root with business logic
│   │   ├── OrderItem.cs              # Entity within aggregate
│   │   ├── OrderStatus.cs            # Enum
│   │   └── IOrderRepository.cs       # Repository interface
│   ├── ValueObjects/
│   │   ├── Money.cs                  # Value object with operators
│   │   ├── OrderId.cs                # Strongly-typed ID
│   │   ├── CustomerId.cs             # Strongly-typed ID
│   │   ├── ProductId.cs              # Strongly-typed ID
│   │   └── Email.cs                  # Validated email
│   ├── Services/
│   │   └── PricingService.cs         # Domain service
│   ├── Events/
│   │   └── OrderConfirmedEvent.cs    # Domain event
│   └── Exceptions/
│       └── DomainException.cs        # Base domain exception
├── DDDPlayground.Application/        # Use case orchestration
│   ├── Orders/
│   │   ├── CreateOrder/
│   │   │   ├── CreateOrderCommand.cs # Command DTO
│   │   │   └── CreateOrderHandler.cs # Command handler
│   │   ├── ConfirmOrder/
│   │   │   ├── ConfirmOrderCommand.cs
│   │   │   └── ConfirmOrderHandler.cs
│   │   ├── GetOrder/
│   │   │   ├── GetOrderQuery.cs      # Query DTO
│   │   │   └── GetOrderHandler.cs    # Query handler
│   │   └── OrderResponse.cs          # Response DTO with manual mapping
│   ├── Common/
│   │   └── IUnitOfWork.cs            # Transaction boundary
│   └── DependencyInjection.cs        # MediatR registration
├── DDDPlayground.Infrastructure/     # Persistence implementation
│   ├── Persistence/
│   │   ├── Models/                   # Separate persistence entities
│   │   │   ├── OrderEntity.cs        # EF Core entity (NOT domain Order)
│   │   │   └── OrderItemEntity.cs    # EF Core entity
│   │   ├── Mappers/                  # Manual domain ↔ persistence mapping
│   │   │   ├── OrderMapper.cs        # ToEntity() / ToDomain()
│   │   │   └── OrderItemMapper.cs
│   │   ├── Configurations/           # EF Core Fluent API
│   │   │   ├── OrderEntityConfiguration.cs
│   │   │   └── OrderItemEntityConfiguration.cs
│   │   ├── Repositories/
│   │   │   └── OrderRepository.cs    # IOrderRepository implementation
│   │   ├── Migrations/               # EF Core migrations
│   │   └── AppDbContext.cs           # DbContext with DbSet<OrderEntity>
│   └── DependencyInjection.cs        # Repository registration
├── DDDPlayground.ApiService/         # HTTP API layer
│   ├── Endpoints/
│   │   └── OrderEndpoints.cs         # Minimal API endpoints with MapGroup
│   ├── Models/
│   │   └── CreateOrderRequest.cs     # API request DTOs
│   ├── Program.cs                    # Aspire + Minimal API setup
│   ├── appsettings.json              # Configuration
│   └── DDDPlayground.ApiService.http # HTTP client test file
├── .gitignore                        # Git ignore rules
├── DDDPlayground.sln                 # Solution file
├── MVP.md                            # Detailed MVP specification
└── README.md                         # This file
```

## 🔌 API Endpoints

### Orders

All endpoints are prefixed with `/api/orders`.

#### Create Order

```http
POST /api/orders
Content-Type: application/json

{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "items": [
    {
      "productId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 2,
      "unitPrice": 99.99,
      "currency": "USD"
    }
  ]
}
```

##### Response: 201 Created

```json
{
  "id": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Draft",
  "subtotalAmount": 199.98,
  "subtotalCurrency": "USD",
  "discountAmount": 0.0,
  "discountCurrency": "USD",
  "totalAmount": 199.98,
  "totalCurrency": "USD",
  "createdAt": "2025-11-01T12:00:00Z",
  "confirmedAt": null,
  "items": [...]
}
```

#### Get Order

```http
GET /api/orders/{id}
```

**Response: 200 OK** (same structure as Create response)

#### Confirm Order

```http
POST /api/orders/{id}/confirm
```

**Response: 200 OK** (returns updated order with `status: "Confirmed"` and `confirmedAt` timestamp)

### Testing with HTTP Client

Use the included `DDDPlayground.ApiService.http` file with Visual Studio or VS Code REST Client extension.

## 📚 Learning Resources

### Key Files to Study

1. **Domain Model**: `DDDPlayground.Domain/Orders/Order.cs`
   - Rich domain model with encapsulation
   - Business rules enforcement
   - Factory methods and reconstitution

2. **Persistence Separation**: `DDDPlayground.Infrastructure/Persistence/`
   - `Models/OrderEntity.cs` - Separate EF entity
   - `Mappers/OrderMapper.cs` - Manual mapping
   - `Configurations/OrderEntityConfiguration.cs` - Fluent API

3. **CQRS Pattern**: `DDDPlayground.Application/Orders/`
   - Commands vs. Queries
   - Handler responsibilities
   - DTO mapping

4. **Minimal APIs**: `DDDPlayground.ApiService/Endpoints/OrderEndpoints.cs`
   - MapGroup organization
   - TypedResults for type safety
   - Static handlers for performance

### Patterns Explained

#### Rich Domain Model

```csharp
// ❌ Anemic (all properties public, no logic)
public class Order 
{ 
    public Guid Id { get; set; }
    public string Status { get; set; }
}

// ✅ Rich (encapsulated, with business logic)
public class Order 
{ 
    public OrderId Id { get; private set; }
    public OrderStatus Status { get; private set; }
    
    public void Confirm() 
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Only draft orders can be confirmed");
        Status = OrderStatus.Confirmed;
    }
}
```

#### Persistence Ignorance

```csharp
// ❌ Domain model with EF attributes
public class Order 
{
    [Key]
    public Guid Id { get; set; }
    
    [Column("order_status")]
    public string Status { get; set; }
}

// ✅ Separate domain and persistence models
// Domain: Order.cs (pure business logic)
public sealed class Order { /* ... */ }

// Persistence: OrderEntity.cs (EF Core entity)
public class OrderEntity { /* ... */ }

// Mapper: OrderMapper.cs (explicit transformation)
public static class OrderMapper 
{
    public static OrderEntity ToEntity(Order order) { /* ... */ }
    public static Order ToDomain(OrderEntity entity) { /* ... */ }
}
```

### External Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
