# Ecommerce.Web Implementation Examples

## Usage Examples

### Consuming the Product Service in a Blazor Component

```razor
@page "/my-products-page"
@using Ecommerce.Web.Services.Products
@using Ecommerce.Web.ViewModels.Products
@inject IProductService ProductService

<h1>My Products</h1>

@if (products != null)
{
    @foreach (var product in products.Items)
    {
        <div>
            <h3>@product.Name</h3>
            <p>Price: @product.FormattedPrice</p>
        </div>
    }
}

@code {
    private PaginatedProductsViewModel? products;

    protected override async Task OnInitializedAsync()
    {
        products = await ProductService.GetProductsAsync(pageNumber: 1, pageSize: 10);
    }
}
```

### Getting a Single Product

```csharp
// In a Blazor component
var product = await ProductService.GetProductByIdAsync(productId);

if (product == null)
{
    // Handle product not found
}
else
{
    Console.WriteLine($"Product: {product.Name}, Price: {product.FormattedPrice}");
}
```

### Creating a Product

```csharp
// In a Blazor form component
var createRequest = new CreateProductViewModel
{
    Sku = "PROD-001",
    Name = "Best Product",
    Price = 99.99m,
    Currency = "USD",
    CategoryId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
    Description = "Amazing product"
};

var createdProduct = await ProductService.CreateProductAsync(createRequest);
Console.WriteLine($"Created product with ID: {createdProduct.Id}");
```

## Architecture Patterns

### Service Layer Pattern

The service layer abstracts HTTP details from components:

```csharp
// Components never call HttpClient directly
// They only use IProductService

@inject IProductService ProductService  // ✓ Correct

// NOT:
// @inject IProductApiClient ApiClient  // ✗ Wrong - bypasses service layer
// @inject HttpClient Http              // ✗ Wrong - direct HTTP access
```

### ViewModel Translation

Services translate API responses to UI-specific ViewModels:

```csharp
// API Response (from Ecommerce.Contracts)
CreateProductResponse apiResponse = new(
    Id: Guid.NewGuid(),
    Sku: "SKU123",
    Name: "Product Name",
    Price: 99.99m,
    Currency: "USD",
    // ... other properties
);

// Translated to ViewModel for UI
ProductDetailViewModel viewModel = new()
{
    Id = apiResponse.Id,
    Sku = apiResponse.Sku,
    Name = apiResponse.Name,
    FormattedPrice = $"{apiResponse.Currency} {apiResponse.Price:F2}",
    // UI uses FormattedPrice, API sends separate Currency and Price
};
```

### Dependency Injection Pattern

```csharp
// In ServiceCollectionExtensions.cs
public static IServiceCollection AddProductServices(
    this IServiceCollection services,
    string apiBaseUrl)
{
    // Typed HttpClient registration
    services
        .AddHttpClient<IProductApiClient, ProductApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

    // Service layer registration
    services.AddScoped<IProductService, ProductService>();

    return services;
}

// In Program.cs
var apiUrl = builder.Configuration["ApiSettings:ProductsApiUrl"]
    ?? "default-url";
builder.Services.AddProductServices(apiUrl);
```

### Error Handling Pattern

```razor
@page "/products"
@inject IProductService ProductService

@if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="alert alert-danger">
        <strong>Error:</strong> @errorMessage
    </div>
}

@if (isLoading)
{
    <div class="alert alert-info">Loading...</div>
}

@code {
    private string? errorMessage;
    private bool isLoading;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = null;
            // Call service...
            var products = await ProductService.GetProductsAsync();
        }
        catch (HttpRequestException ex)
        {
            errorMessage = "Failed to connect to API";
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

### Logging Pattern

All services include ILogger injection:

```csharp
public sealed class ProductService : IProductService
{
    private readonly IProductApiClient _apiClient;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductApiClient apiClient, ILogger<ProductService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

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

            _logger.LogInformation("Successfully retrieved product details: {ProductId}", id);
            return MapToDetailViewModel(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product details for ID: {ProductId}", id);
            throw;
        }
    }
}
```

## API Contract Examples

### Incoming Contracts (from Ecommerce.Api)

```csharp
// Response DTO - API returns this for GET /products/{id}
public sealed record CreateProductResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    Guid CategoryId,
    bool IsActive)
{
    public string PriceDisplay => $"{Currency} {Price:F2}";
}

// Request DTO - Send this for POST /products
public sealed record CreateProductRequest(
    string Sku,
    string Name,
    decimal Price,
    Guid CategoryId,
    string? Description = null,
    string Currency = "USD");
```

### Outgoing ViewModels (to Blazor Components)

```csharp
// For list display
public sealed class ProductSummaryViewModel
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string FormattedPrice => $"{Currency} {Price:F2}";
    public bool IsActive { get; set; }
    public Guid CategoryId { get; set; }
}

// For detail display
public sealed class ProductDetailViewModel
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }  // Extra for details
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string FormattedPrice => $"{Currency} {Price:F2}";
    public bool IsActive { get; set; }
    public Guid CategoryId { get; set; }
}

// Pagination wrapper
public sealed class PaginatedProductsViewModel
{
    public IReadOnlyList<ProductSummaryViewModel> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize > 0 ? (TotalCount + PageSize - 1) / PageSize : 0;
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
```

## Configuration Examples

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Ecommerce.Web": "Information"
    }
  },
  "ApiSettings": {
    "ProductsApiUrl": "https://localhost:7001/api/products"
  },
  "AllowedHosts": "*"
}
```

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Ecommerce.Web": "Debug"
    }
  },
  "ApiSettings": {
    "ProductsApiUrl": "https://localhost:7001/api/products"
  }
}
```

## Testing Examples

### Mocking the Service

```csharp
// Unit test example
[TestMethod]
public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
{
    // Arrange
    var mockApiClient = new Mock<IProductApiClient>();
    var mockLogger = new Mock<ILogger<ProductService>>();

    var apiResponse = new CreateProductResponse(
        Id: Guid.NewGuid(),
        Sku: "TEST-001",
        Name: "Test Product",
        Description: "Test description",
        Price: 99.99m,
        Currency: "USD",
        CategoryId: Guid.NewGuid(),
        IsActive: true);

    mockApiClient
        .Setup(x => x.GetProductByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(apiResponse);

    var service = new ProductService(mockApiClient.Object, mockLogger.Object);

    // Act
    var result = await service.GetProductByIdAsync(Guid.NewGuid());

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual("Test Product", result.Name);
}
```

## Performance Considerations

### Pagination

Always use pagination for list endpoints:

```csharp
// Good - Paged request
var products = await ProductService.GetProductsAsync(
    pageNumber: 1,
    pageSize: 20);

// Bad - No pagination (retrieves all)
var allProducts = await ProductService.GetProductsAsync();
```

### Caching (Future Enhancement)

```csharp
// Could be added with property-level checks
private static readonly Dictionary<Guid, ProductDetailViewModel> _cache = [];

public async Task<ProductDetailViewModel?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
{
    if (_cache.TryGetValue(id, out var cached))
    {
        _logger.LogInformation("Cache hit for product {ProductId}", id);
        return cached;
    }

    var response = await _apiClient.GetProductByIdAsync(id, cancellationToken);
    if (response != null)
    {
        var viewModel = MapToDetailViewModel(response);
        _cache[id] = viewModel;
        return viewModel;
    }
    return null;
}
```

---

**For more examples and patterns, see:**
- [Dependency Rules](../docs/architecture/dependency-rules.md)
- [Coding Standards](../docs/coding-standards.md)
- Existing Blazor components: `Components/Products/*.razor`
