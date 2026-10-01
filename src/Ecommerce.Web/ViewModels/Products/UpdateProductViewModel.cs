namespace Ecommerce.Web.ViewModels.Products;

/// <summary>
/// View model for updating an existing product.
/// Supports partial updates - all fields are optional.
/// Used for the product edit form in Blazor.
/// </summary>
public sealed class UpdateProductViewModel
{
    /// <summary>
    /// The product name (optional for update).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The product description (optional for update).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The price amount (optional for update).
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// The currency code (optional for update).
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// The category identifier (optional for update).
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Whether the product is active (optional for update).
    /// </summary>
    public bool? IsActive { get; set; }
}
