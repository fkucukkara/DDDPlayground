using Microsoft.EntityFrameworkCore;
using DDDPlayground.Application.Common;
using DDDPlayground.Infrastructure.Persistence.Models;

namespace DDDPlayground.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for DDD Playground.
/// Contains only persistence entities (OrderEntity), not domain models (Order).
/// Implements IUnitOfWork for transaction boundaries.
/// </summary>
public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// IUnitOfWork implementation - commits all changes in a single transaction.
    /// </summary>
    public async Task<int> SaveChangesAsync()
    {
        return await base.SaveChangesAsync();
    }
}
