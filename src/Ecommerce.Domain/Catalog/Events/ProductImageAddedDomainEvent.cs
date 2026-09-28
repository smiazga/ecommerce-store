namespace Ecommerce.Domain.Catalog.Events;

using Ecommerce.SharedKernel.Abstractions;

/// <summary>
/// Raised when an image is added to a product.
/// </summary>
public sealed class ProductImageAddedDomainEvent : DomainEvent
{
    /// <summary>
    /// Product identifier.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// Image identifier.
    /// </summary>
    public Guid ImageId { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ProductImageAddedDomainEvent"/>.
    /// </summary>
    public ProductImageAddedDomainEvent(Guid productId, Guid imageId)
        : base()
    {
        ProductId = productId;
        ImageId = imageId;
    }
}
