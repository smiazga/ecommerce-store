namespace Ecommerce.Application.Catalog.DTOs;

/// <summary>
/// Data transfer object representing product information.
/// Used for API responses to prevent direct domain entity exposure.
/// </summary>
public sealed record ProductDto(
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
