namespace Ecommerce.Web.ViewModels.Products;

/// <summary>
/// View model for creating a new product.
/// Input model for Blazor form.
/// </summary>
public sealed class CreateProductViewModel
{
    /// <summary>
    /// The product SKU.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// The product name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The product description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The price amount.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// The currency code (default USD).
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// The category identifier.
    /// </summary>
    public Guid CategoryId { get; set; }
}
