namespace Ecommerce.Web.Services.Products;

using Ecommerce.Web.ViewModels.Products;

/// <summary>
/// Business service interface for product operations.
/// Abstracts HTTP details and translates API responses to ViewModels for Blazor components.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves a product by its identifier.
    /// Translates API response to ProductDetailViewModel.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>ProductDetailViewModel if found; null if not found.</returns>
    Task<ProductDetailViewModel?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paged list of products.
    /// Translates API responses to ProductSummaryViewModels.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated collection of product summaries.</returns>
    Task<PaginatedProductsViewModel> GetProductsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">Product creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>ProductDetailViewModel of the created product.</returns>
    Task<ProductDetailViewModel> CreateProductAsync(
        CreateProductViewModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="request">Product update request with optional fields.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>ProductDetailViewModel of the updated product.</returns>
    Task<ProductDetailViewModel> UpdateProductAsync(
        Guid id,
        UpdateProductViewModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes (deactivates) an existing product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
}
