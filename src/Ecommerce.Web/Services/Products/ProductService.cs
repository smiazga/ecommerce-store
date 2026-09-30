namespace Ecommerce.Web.Services.Products;

using Ecommerce.Contracts.Catalog;
using Ecommerce.Web.Clients;
using Ecommerce.Web.ViewModels.Products;

/// <summary>
/// Implementation of the product business service.
/// Translates API responses to ViewModels and coordinates product operations.
/// </summary>
public sealed class ProductService : IProductService
{
    private readonly IProductApiClient _apiClient;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductApiClient apiClient, ILogger<ProductService> logger)
    {
        ArgumentNullException.ThrowIfNull(apiClient, nameof(apiClient));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a product by its identifier.
    /// </summary>
    public async Task<ProductDetailViewModel?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving product details for ID: {ProductId}", id);

            var response = await _apiClient.GetProductByIdAsync(id, cancellationToken);

            if (response is null)
            {
                _logger.LogWarning("Product not found: {ProductId}", id);
                return null;
            }

            var viewModel = MapToDetailViewModel(response);
            _logger.LogInformation("Successfully retrieved product details: {ProductId}", id);
            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product details for ID: {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a paged list of products.
    /// </summary>
    public async Task<PaginatedProductsViewModel> GetProductsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving products. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

            var responses = await _apiClient.GetProductsAsync(pageNumber, pageSize, cancellationToken);

            var items = responses
                .Select(MapToSummaryViewModel)
                .ToList();

            var viewModel = new PaginatedProductsViewModel
            {
                Items = items.AsReadOnly(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = items.Count,
                // Note: API doesn't return total count, so we assume retrieved count is from requested page
                // For production, modify API to return paging metadata
            };

            _logger.LogInformation("Successfully retrieved {ProductCount} products", items.Count);
            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products (page {PageNumber})", pageNumber);
            throw;
        }
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    public async Task<ProductDetailViewModel> CreateProductAsync(
        CreateProductViewModel request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        try
        {
            _logger.LogInformation("Creating product: {ProductName}", request.Name);

            var createRequest = new CreateProductRequest(
                request.Sku,
                request.Name,
                request.Price,
                request.CategoryId,
                request.Description,
                request.Currency);

            var response = await _apiClient.CreateProductAsync(createRequest, cancellationToken);

            if (response is null)
            {
                _logger.LogError("Product creation returned null response");
                throw new InvalidOperationException("Product creation unexpected response");
            }

            var viewModel = MapToDetailViewModel(response);
            _logger.LogInformation("Successfully created product: {ProductId}", response.Id);
            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", request.Name);
            throw;
        }
    }

    /// <summary>
    /// Maps API response to ProductSummaryViewModel.
    /// </summary>
    private static ProductSummaryViewModel MapToSummaryViewModel(CreateProductResponse response)
    {
        ArgumentNullException.ThrowIfNull(response, nameof(response));

        return new ProductSummaryViewModel
        {
            Id = response.Id,
            Sku = response.Sku,
            Name = response.Name,
            Price = response.Price,
            Currency = response.Currency,
            IsActive = response.IsActive,
            CategoryId = response.CategoryId
        };
    }

    /// <summary>
    /// Maps API response to ProductDetailViewModel.
    /// </summary>
    private static ProductDetailViewModel MapToDetailViewModel(CreateProductResponse response)
    {
        ArgumentNullException.ThrowIfNull(response, nameof(response));

        return new ProductDetailViewModel
        {
            Id = response.Id,
            Sku = response.Sku,
            Name = response.Name,
            Description = response.Description,
            Price = response.Price,
            Currency = response.Currency,
            IsActive = response.IsActive,
            CategoryId = response.CategoryId
        };
    }
}
