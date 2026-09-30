namespace Ecommerce.Contracts.Catalog;

/// <summary>
/// API request contract for updating an existing product.
/// All fields are optional to support partial updates.
/// </summary>
public sealed record UpdateProductRequest(
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    string? Currency = null,
    Guid? CategoryId = null,
    bool? IsActive = null);
