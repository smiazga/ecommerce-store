namespace Ecommerce.Web.ViewModels.Products;

/// <summary>
/// View model for displaying a product summary in a list.
/// UI-specific properties for Blazor components.
/// </summary>
public sealed class ProductSummaryViewModel
{
    /// <summary>
    /// The product identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The product SKU.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// The product name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The price amount.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// The currency code (USD, EUR, etc.).
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Formatted price display (e.g., "USD 19.99").
    /// </summary>
    public string FormattedPrice => $"{Currency} {Price:F2}";

    /// <summary>
    /// Whether the product is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The category identifier.
    /// </summary>
    public Guid CategoryId { get; set; }
}
