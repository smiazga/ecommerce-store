namespace Ecommerce.Domain.Catalog.Events;

using Ecommerce.SharedKernel.Abstractions;

/// <summary>
/// Raised when a product is deactivated.
/// </summary>
public sealed class ProductDeactivatedDomainEvent : DomainEvent
{
    /// <summary>
    /// Product identifier.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ProductDeactivatedDomainEvent"/>.
    /// </summary>
    public ProductDeactivatedDomainEvent(Guid productId)
        : base()
    {
        ProductId = productId;
    }
}
