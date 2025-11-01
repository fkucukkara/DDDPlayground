using Microsoft.Extensions.DependencyInjection;

namespace DDDPlayground.Application;

/// <summary>
/// Application layer dependency injection configuration.
/// Registers MediatR and FluentValidation services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Application layer services.
    /// Call this from Program.cs in the API project.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
