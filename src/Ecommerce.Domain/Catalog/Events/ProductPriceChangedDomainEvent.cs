namespace Ecommerce.Domain.Catalog.Events;

using Ecommerce.Domain.ValueObjects;
using Ecommerce.SharedKernel.Abstractions;

/// <summary>
/// Raised when a product price is changed.
/// </summary>
public sealed class ProductPriceChangedDomainEvent : DomainEvent
{
    /// <summary>
    /// Product identifier.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// Previous price.
    /// </summary>
    public Money OldPrice { get; }

    /// <summary>
    /// New price.
    /// </summary>
    public Money NewPrice { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ProductPriceChangedDomainEvent"/>.
    /// </summary>
    public ProductPriceChangedDomainEvent(Guid productId, Money oldPrice, Money newPrice)
        : base()
    {
        ProductId = productId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }
}
