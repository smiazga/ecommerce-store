namespace Ecommerce.Domain.Catalog.Events;

using Ecommerce.SharedKernel.Abstractions;
using Ecommerce.Domain.ValueObjects;

/// <summary>
/// Raised when a new product is created.
/// </summary>
public sealed class ProductCreatedDomainEvent : DomainEvent
{
    /// <summary>
    /// Product identifier.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// Product SKU.
    /// </summary>
    public Sku Sku { get; }

    /// <summary>
    /// Product name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Product price.
    /// </summary>
    public Money Price { get; }

    /// <summary>
    /// Category identifier.
    /// </summary>
    public Guid CategoryId { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ProductCreatedDomainEvent"/>.
    /// </summary>
    public ProductCreatedDomainEvent(Guid productId, Sku sku, string name, Money price, Guid categoryId)
        : base()
    {
        ProductId = productId;
        Sku = sku;
        Name = name;
        Price = price;
        CategoryId = categoryId;
    }
}
