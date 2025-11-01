using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DDDPlayground.Infrastructure.Persistence.Models;

namespace DDDPlayground.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for OrderEntity.
/// Demonstrates explicit configuration instead of conventions or attributes.
/// </summary>
public class OrderEntityConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired()
            .HasMaxLength(50);

        // Money value objects flattened to columns
        builder.Property(o => o.SubtotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.SubtotalCurrency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(o => o.DiscountAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.DiscountCurrency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(o => o.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.TotalCurrency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.ConfirmedAt)
            .IsRequired(false);

        // One-to-many relationship with OrderItems
        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for customer lookup
        builder.HasIndex(o => o.CustomerId);
    }
}
