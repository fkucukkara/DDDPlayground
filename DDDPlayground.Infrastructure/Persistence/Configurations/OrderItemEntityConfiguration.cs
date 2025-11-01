using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DDDPlayground.Infrastructure.Persistence.Models;

namespace DDDPlayground.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for OrderItemEntity.
/// </summary>
public class OrderItemEntityConfiguration : IEntityTypeConfiguration<OrderItemEntity>
{
    public void Configure(EntityTypeBuilder<OrderItemEntity> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductId)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .IsRequired();

        // Money value object flattened to columns
        builder.Property(i => i.UnitPriceAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(i => i.UnitPriceCurrency)
            .IsRequired()
            .HasMaxLength(3);

        // Index for product lookup
        builder.HasIndex(i => i.ProductId);
    }
}
