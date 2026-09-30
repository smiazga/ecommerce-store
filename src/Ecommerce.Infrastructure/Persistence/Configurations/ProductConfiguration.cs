namespace Ecommerce.Infrastructure.Persistence.Configurations;

using Ecommerce.Domain.Catalog;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// EF Core configuration for the Product aggregate.
/// </summary>
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.IsActive)
            .IsRequired();

        // Concurrency token
        builder.Property(p => p.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Map SKU as owned type stored in a single column
        builder.OwnsOne(p => p.Sku, sku =>
        {
            // store SKU value in a dedicated column named SkuValue to avoid
            // a naming collision between the owned navigation and a shadow
            // property when building the EF model at design-time.
            sku.Property(s => s.Value)
                .HasColumnName("SkuValue")
                .IsRequired()
                .HasMaxLength(100);
        });

        // Map Price as owned type with separate columns
        builder.OwnsOne(p => p.Price, price =>
        {
            price.Property(pn => pn.Amount).HasColumnName("Price_Amount");
            price.Property(pn => pn.Currency).HasColumnName("Price_Currency").HasMaxLength(3).IsRequired();
        });

        // Unique index on SKU is intentionally not configured here to avoid
        // complications with owned property mapping during design-time model
        // creation. If you need an index on the SKU column, add it via a
        // dedicated migration or configure it on the owned type.

        // Configure images navigation to use backing field
        builder.Navigation(nameof(Product.Images)).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Configure relationship to product images (separate entity configuration will define table)
        builder.HasMany(p => p.Images)
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
