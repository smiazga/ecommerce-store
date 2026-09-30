namespace Ecommerce.Contracts.Catalog;

/// <summary>
/// API response contract for product creation.
/// Returned on successful product creation (201 Created).
/// </summary>
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
    /// <summary>
    /// Gets the full price display (Currency Amount).
    /// </summary>
    public string PriceDisplay => $"{Currency} {Price:F2}";
}
