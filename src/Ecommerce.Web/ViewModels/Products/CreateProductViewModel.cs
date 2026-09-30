namespace Ecommerce.Web.ViewModels.Products;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// View model for creating a new product.
/// Input model for Blazor form.
/// </summary>
public sealed class CreateProductViewModel
{
    /// <summary>
    /// The product SKU.
    /// </summary>
    [Required(ErrorMessage = "SKU is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "SKU must be between 1 and 50 characters")]
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// The product name.
    /// </summary>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The product description.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// The price amount.
    /// </summary>
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    /// <summary>
    /// The currency code (default USD).
    /// </summary>
    [Required(ErrorMessage = "Currency is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters")]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// The category identifier.
    /// </summary>
    [Required(ErrorMessage = "Category is required")]
    public Guid CategoryId { get; set; }
}
