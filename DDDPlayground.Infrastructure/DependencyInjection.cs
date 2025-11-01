using DDDPlayground.Application.Common;
using DDDPlayground.Domain.Orders;
using DDDPlayground.Infrastructure.Persistence;
using DDDPlayground.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DDDPlayground.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection configuration.
/// Demonstrates clean registration of persistence concerns.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Infrastructure layer services.
    /// Call this from Program.cs in the API project.
    /// Note: DbContext registration is handled by Aspire in Program.cs
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Unit of Work pattern
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

        // Repository pattern
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
