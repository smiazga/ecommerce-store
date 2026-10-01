namespace Ecommerce.Web.Clients;

using Ecommerce.Contracts.Catalog;

/// <summary>
/// Typed HTTP client for Product API operations.
/// Handles all direct API communication for product-related endpoints.
/// </summary>
public interface IProductApiClient
{
    /// <summary>
    /// Retrieves a product by its identifier.
    /// </summary>
    /// <param name="id">The product identifier (Guid).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Product response if found; null if not found.</returns>
    Task<CreateProductResponse?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paged list of products.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Collection of product responses.</returns>
    Task<IReadOnlyList<CreateProductResponse>> GetProductsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The product creation request.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Response containing the created product details.</returns>
    Task<CreateProductResponse> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="request">The product update request (all fields optional).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Response containing the updated product details.</returns>
    Task<CreateProductResponse> UpdateProductAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes (deactivates) an existing product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Response containing the deleted product details.</returns>
    Task<CreateProductResponse> DeleteProductAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
