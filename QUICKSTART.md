# Quick Start Guide

This guide will help you get the DDD Playground up and running in minutes.

## Prerequisites Check

Before you begin, ensure you have:

- [ ] .NET 9 SDK installed: `dotnet --version` (should show 9.0 or later)
- [ ] Docker Desktop running (for PostgreSQL container)
- [ ] .NET Aspire workload: `dotnet workload list` (should show `aspire`)

## Installation Steps

### 1. Install .NET Aspire Workload (if not already installed)

```powershell
dotnet workload update
dotnet workload install aspire
```

### 2. Clone and Navigate

```powershell
git clone <your-repo-url>
cd DDDPlayground
```

### 3. Restore Dependencies

```powershell
dotnet restore
```

### 4. Run the Application

#### Using Visual Studio 2022

1. Open `DDDPlayground.sln`
2. Ensure `DDDPlayground.AppHost` is set as the startup project
3. Press `F5` or click the Run button

#### Using Command Line

```powershell
cd DDDPlayground.AppHost
dotnet run
```

### 5. Access the Services

After starting, you should see console output with URLs:

```
info: Aspire.Hosting.DistributedApplication[0]
      Aspire version: 9.5.2+...
      Now listening on: http://localhost:15000
      Distributed application started. Press Ctrl+C to shut down.
```

Open your browser to:

- **Aspire Dashboard**: http://localhost:15000
- **API Service**: Check dashboard for the actual API port (usually https://localhost:7xxx)

## First API Call

### Using the .http file (Visual Studio/VS Code)

1. Open `DDDPlayground.ApiService/DDDPlayground.ApiService.http`
2. Update the port in `@ApiService_HostAddress` if needed
3. Click "Send Request" above the "Create Order" request

### Using curl

```powershell
# Get the API URL from the Aspire dashboard, then:
curl -X POST https://localhost:7xxx/api/orders `
  -H "Content-Type: application/json" `
  -d '{
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "items": [{
      "productId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 2,
      "unitPrice": 99.99,
      "currency": "USD"
    }]
  }'
```

### Expected Response

```json
{
  "id": "generated-guid-here",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Draft",
  "subtotalAmount": 199.98,
  "subtotalCurrency": "USD",
  "discountAmount": 0.0,
  "discountCurrency": "USD",
  "totalAmount": 199.98,
  "totalCurrency": "USD",
  "createdAt": "2025-11-01T...",
  "confirmedAt": null,
  "items": [...]
}
```

## Exploring the Aspire Dashboard

The Aspire Dashboard provides:

### Resources Tab
- View all running services (API, PostgreSQL, Redis)
- Check health status
- See connection strings

### Traces Tab
- OpenTelemetry distributed traces
- See the full request flow through your application
- Identify performance bottlenecks

### Metrics Tab
- Real-time metrics (requests/sec, duration, errors)
- CPU and memory usage

### Logs Tab
- Structured logs from all services
- Filter by severity, service, or search text

### Console Tab
- View stdout/stderr from each service
- Useful for debugging

## Accessing PostgreSQL

### Using pgAdmin (via Aspire)

1. In the Aspire Dashboard, go to **Resources**
2. Find the `postgres` resource
3. Click the **pgAdmin** link
4. Login with default credentials (shown in dashboard)
5. Connect to database `dddplaygrounddb`

### Using Connection String

The connection string is shown in the Aspire Dashboard under the `dddplaygrounddb` resource.

## Troubleshooting

### "Workload 'aspire' not found"

```powershell
dotnet workload update
dotnet workload install aspire
```

### Docker not running

Ensure Docker Desktop is started. Aspire needs it to run PostgreSQL containers.

### Port already in use

Aspire will automatically select different ports if the defaults are taken. Check the console output for actual URLs.

### Database migration errors

The application will automatically create the database on first run. If you encounter issues:

```powershell
cd DDDPlayground.Infrastructure
dotnet ef database drop --force --startup-project ../DDDPlayground.ApiService
dotnet ef database update --startup-project ../DDDPlayground.ApiService
```

### "Cannot find MediatR handlers"

Ensure you've run `dotnet restore` at the solution level:

```powershell
dotnet restore DDDPlayground.sln
```

## Next Steps

1. **Explore the Code**
   - Start with `DDDPlayground.Domain/Orders/Order.cs` to see the rich domain model
   - Check `DDDPlayground.Infrastructure/Persistence/Mappers/OrderMapper.cs` for domain/persistence separation
   - Look at `DDDPlayground.ApiService/Endpoints/OrderEndpoints.cs` for Minimal API patterns

2. **Try the API**
   - Create an order
   - Get the order by ID
   - Confirm the order
   - Watch the traces in Aspire Dashboard

3. **Read the Documentation**
   - `README.md` - Comprehensive overview
   - `MVP.md` - Detailed specifications and patterns
   - Inline code comments - Learn as you read

4. **Experiment**
   - Add a new domain rule to `Order.Confirm()`
   - Create a new value object
   - Add a new endpoint

## Common Development Tasks

### Adding a Migration

```powershell
cd DDDPlayground.Infrastructure
dotnet ef migrations add <MigrationName> --startup-project ../DDDPlayground.ApiService
```

### Running Tests (when added)

```powershell
dotnet test
```

### Viewing Logs

Check the Aspire Dashboard **Logs** tab, or view structured logs in the console.

### Stopping the Application

Press `Ctrl+C` in the terminal where `dotnet run` is executing, or stop debugging in Visual Studio.

## Getting Help

- Check the [README.md](README.md) for detailed documentation
- Review the [MVP.md](MVP.md) for architectural decisions
- Read inline code comments for pattern explanations
- Check the [.NET Aspire documentation](https://learn.microsoft.com/dotnet/aspire/)

---

**Happy Learning! 🎓**
