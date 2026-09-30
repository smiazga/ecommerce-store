namespace Ecommerce.Contracts.Catalog;

/// <summary>
/// API request contract for creating a new product.
/// </summary>
public sealed record CreateProductRequest(
    string Sku,
    string Name,
    decimal Price,
    Guid CategoryId,
    string? Description = null,
    string Currency = "USD");
