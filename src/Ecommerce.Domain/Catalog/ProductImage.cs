namespace Ecommerce.Domain.Catalog;

using Ecommerce.SharedKernel.Abstractions;

/// <summary>
/// Represents an image associated with a product.
/// Owned by the Product aggregate.
/// </summary>
public sealed class ProductImage : Entity
{
    /// <summary>
    /// The image URL.
    /// </summary>
    public string ImageUrl { get; private set; }

    /// <summary>
    /// The display order for the image. Zero-based.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Parameterless constructor for EF Core.
    /// </summary>
    private ProductImage()
    {
        ImageUrl = string.Empty;
        DisplayOrder = 0;
    }

    /// <summary>
    /// Creates a new product image.
    /// </summary>
    /// <param name="imageUrl">The image URL.</param>
    /// <param name="displayOrder">Display order (>= 0).</param>
    public ProductImage(string imageUrl, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new ArgumentException("ImageUrl is required.", nameof(imageUrl));
        }

        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(displayOrder), "DisplayOrder must be >= 0.");
        }

        ImageUrl = imageUrl.Trim();
        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Updates the display order.
    /// </summary>
    /// <param name="newOrder">New display order (>=0).</param>
    public void UpdateDisplayOrder(int newOrder)
    {
        if (newOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newOrder), "DisplayOrder must be >= 0.");
        }

        DisplayOrder = newOrder;
    }
}
