namespace Ecommerce.Infrastructure.Persistence;

using Ecommerce.Domain.Catalog;
using Ecommerce.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// EF Core DbContext for Ecommerce infrastructure persistence.
/// </summary>
public sealed class EcommerceDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="EcommerceDbContext"/>.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Products set.
    /// </summary>
    public DbSet<Product> Products { get; set; } = null!;

    /// <summary>
    /// Product images set.
    /// </summary>
    public DbSet<ProductImage> ProductImages { get; set; } = null!;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductImageConfiguration());
    }
}
