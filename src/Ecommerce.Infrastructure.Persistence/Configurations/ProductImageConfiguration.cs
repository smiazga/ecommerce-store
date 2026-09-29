namespace Ecommerce.Infrastructure.Persistence.Configurations;

using Ecommerce.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// EF Core configuration for ProductImage entity.
/// </summary>
public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.ImageUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(pi => pi.DisplayOrder)
            .IsRequired();

        // configure FK column created by ProductConfiguration
        builder.Property<Guid>("ProductId");

        builder.HasIndex("ProductId");
    }
}
