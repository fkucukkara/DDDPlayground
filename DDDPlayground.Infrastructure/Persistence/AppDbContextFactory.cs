using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DDDPlayground.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating DbContext during migrations.
/// Required because Aspire configures connection strings at runtime.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        // Use a placeholder connection string for migrations
        // The actual connection string will be provided by Aspire at runtime
        optionsBuilder.UseNpgsql("Host=localhost;Database=dddplaygrounddb;Username=postgres;Password=postgres");
        
        return new AppDbContext(optionsBuilder.Options);
    }
}
