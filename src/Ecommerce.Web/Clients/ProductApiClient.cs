namespace Ecommerce.Web.Clients;

using System.Text.Json;

using Ecommerce.Contracts.Catalog;

/// <summary>
/// Implementation of the typed HTTP client for Product API.
/// Handles HTTP communication with product endpoints.
/// </summary>
public sealed class ProductApiClient : IProductApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductApiClient> _logger;

    public ProductApiClient(HttpClient httpClient, ILogger<ProductApiClient> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a product by its identifier.
    /// </summary>
    public async Task<CreateProductResponse?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching product with ID: {ProductId}", id);

            // Use relative URI segments when BaseAddress already contains the API path
            var response = await _httpClient.GetAsync($"/api/products/{id}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Product not found: {ProductId}", id);
                    return null;
                }

                _logger.LogError("Failed to fetch product {ProductId}. Status: {StatusCode}", id, response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var product = JsonSerializer.Deserialize<CreateProductResponse>(content, GetJsonOptions());

            _logger.LogInformation("Successfully fetched product: {ProductId}", id);
            return product;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching product {ProductId}", id);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error while fetching product {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a paged list of products.
    /// </summary>
    public async Task<IReadOnlyList<CreateProductResponse>> GetProductsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure valid pagination parameters
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            _logger.LogInformation("Fetching products. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

            // Build a relative query string; do not start with a leading slash which would target the host root
            var requestUri = $"?pageNumber={pageNumber}&pageSize={pageSize}";
            var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch products. Status: {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var products = JsonSerializer.Deserialize<List<CreateProductResponse>>(content, GetJsonOptions());

            _logger.LogInformation("Successfully fetched {ProductCount} products", products?.Count ?? 0);
            return products?.AsReadOnly() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching products (page {PageNumber})", pageNumber);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error while fetching products");
            throw;
        }
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    public async Task<CreateProductResponse> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        try
        {
            _logger.LogInformation("Creating product: {ProductName}", request.Name);

            var jsonContent = JsonSerializer.Serialize(request, GetJsonOptions());
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            // Post to the collection resource; use empty string to post to the base address (e.g. /api/products)
            var response = await _httpClient.PostAsync(string.Empty, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create product. Status: {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var createdProduct = JsonSerializer.Deserialize<CreateProductResponse>(responseContent, GetJsonOptions());

            if (createdProduct is null)
            {
                _logger.LogError("Product response deserialization returned null");
                throw new InvalidOperationException("Failed to deserialize product response");
            }

            _logger.LogInformation("Successfully created product: {ProductId}", createdProduct.Id);
            return createdProduct;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while creating product {ProductName}", request.Name);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON error while creating product {ProductName}", request.Name);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    public async Task<CreateProductResponse> UpdateProductAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        try
        {
            _logger.LogInformation("Updating product: {ProductId}", id);

            var jsonContent = JsonSerializer.Serialize(request, GetJsonOptions());
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            // PUT to the specific product resource
            var response = await _httpClient.PutAsync($"/api/products/{id}", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to update product {ProductId}. Status: {StatusCode}", id, response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var updatedProduct = JsonSerializer.Deserialize<CreateProductResponse>(responseContent, GetJsonOptions());

            if (updatedProduct is null)
            {
                _logger.LogError("Update product response deserialization returned null");
                throw new InvalidOperationException("Failed to deserialize update product response");
            }

            _logger.LogInformation("Successfully updated product: {ProductId}", updatedProduct.Id);
            return updatedProduct;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while updating product {ProductId}", id);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON error while updating product {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Deletes (deactivates) an existing product.
    /// </summary>
    public async Task<CreateProductResponse> DeleteProductAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting product: {ProductId}", id);

            // DELETE to the specific product resource
            var response = await _httpClient.DeleteAsync($"/api/products/{id}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to delete product {ProductId}. Status: {StatusCode}", id, response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var deletedProduct = JsonSerializer.Deserialize<CreateProductResponse>(responseContent, GetJsonOptions());

            if (deletedProduct is null)
            {
                _logger.LogError("Delete product response deserialization returned null");
                throw new InvalidOperationException("Failed to deserialize delete product response");
            }

            _logger.LogInformation("Successfully deleted product: {ProductId}", deletedProduct.Id);
            return deletedProduct;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while deleting product {ProductId}", id);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON error while deleting product {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets JSON serializer options for consistent deserialization.
    /// </summary>
    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
}
