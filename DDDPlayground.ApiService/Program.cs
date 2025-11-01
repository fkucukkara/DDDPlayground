using DDDPlayground.ApiService.Endpoints;
using DDDPlayground.Application;
using DDDPlayground.Infrastructure;
using DDDPlayground.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();

// Add Aspire PostgreSQL with EF Core
builder.AddNpgsqlDbContext<AppDbContext>("dddplaygrounddb");

// Add Application layer (MediatR, FluentValidation)
builder.Services.AddApplication();

// Add Infrastructure layer (Repositories, Unit of Work)
builder.Services.AddInfrastructure();

// Add services to the container
builder.Services.AddProblemDetails();

// Add OpenAPI/Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map API endpoints
app.MapOrderEndpoints();

app.MapDefaultEndpoints();

app.Run();
