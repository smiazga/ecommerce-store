namespace Ecommerce.Infrastructure.Persistence.Repositories;

using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// EF Core implementation of <see cref="IProductRepository"/>.
/// </summary>
public sealed class ProductRepository : IProductRepository
{
    private readonly EcommerceDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductRepository"/> class.
    /// </summary>
    public ProductRepository(EcommerceDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(p => p.Images)
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Product?> GetBySkuAsync(Ecommerce.Domain.ValueObjects.Sku sku, CancellationToken cancellationToken = default)
    {
        if (sku is null) throw new ArgumentNullException(nameof(sku));

        return await _dbContext.Products
            .Include(p => p.Images)
            .SingleOrDefaultAsync(p => p.Sku.Value == sku.Value, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (product is null) throw new ArgumentNullException(nameof(product));

        await _dbContext.Products.AddAsync(product, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (product is null) throw new ArgumentNullException(nameof(product));

        _dbContext.Products.Update(product);
        return Task.CompletedTask;
    }
}
