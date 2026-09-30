namespace Ecommerce.Domain.Repositories;

using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.ValueObjects;

/// <summary>
/// Repository contract for product aggregates.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Finds a product by identifier.
    /// </summary>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a product by SKU.
    /// </summary>
    Task<Product?> GetBySkuAsync(Sku sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new product to the repository.
    /// </summary>
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product in the repository.
    /// </summary>
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paged list of products.
    /// </summary>
    Task<IEnumerable<Product>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
