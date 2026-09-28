namespace Ecommerce.Domain.Catalog.Events;

using Ecommerce.SharedKernel.Abstractions;

/// <summary>
/// Raised when a product is activated.
/// </summary>
public sealed class ProductActivatedDomainEvent : DomainEvent
{
    /// <summary>
    /// Product identifier.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ProductActivatedDomainEvent"/>.
    /// </summary>
    public ProductActivatedDomainEvent(Guid productId)
        : base()
    {
        ProductId = productId;
    }
}
