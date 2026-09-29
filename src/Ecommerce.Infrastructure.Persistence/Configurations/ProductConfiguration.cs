namespace Ecommerce.Infrastructure.Persistence.Configurations;

using Ecommerce.Domain.Products;
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
            sku.Property(s => s.Value)
                .HasColumnName("Sku")
                .IsRequired()
                .HasMaxLength(100);
        });

        // Map Price as owned type with separate columns
        builder.OwnsOne(p => p.Price, price =>
        {
            price.Property(pn => pn.Amount).HasColumnName("Price_Amount");
            price.Property(pn => pn.Currency).HasColumnName("Price_Currency").HasMaxLength(3).IsRequired();
        });

        // Unique index on SKU
        builder.HasIndex("Sku").IsUnique();

        // Configure images navigation to use backing field
        builder.Navigation(nameof(Product.Images)).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Configure relationship to product images (separate entity configuration will define table)
        builder.HasMany(p => p.Images)
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
