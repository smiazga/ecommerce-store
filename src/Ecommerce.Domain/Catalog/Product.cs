namespace Ecommerce.Domain.Catalog;

using Ecommerce.Domain.Catalog.Events;
using Ecommerce.Domain.ValueObjects;
using Ecommerce.SharedKernel.Abstractions;
using Ecommerce.SharedKernel.Guards;
using Ecommerce.SharedKernel.Results;

/// <summary>
/// Aggregate root representing a product in the catalog.
/// </summary>
public sealed class Product : AggregateRoot
{
    private readonly List<ProductImage> _images = new();

    /// <summary>
    /// The product name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The product description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// The product SKU.
    /// </summary>
    public Sku Sku { get; private set; }

    /// <summary>
    /// The product price.
    /// </summary>
    public Money Price { get; private set; }

    /// <summary>
    /// Category identifier.
    /// </summary>
    public Guid CategoryId { get; private set; }

    /// <summary>
    /// Indicates whether the product is active/visible in catalog.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Optimistic concurrency token (optional).
    /// </summary>
    public byte[]? RowVersion { get; private set; }

    /// <summary>
    /// Collection of product images.
    /// </summary>
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private Product()
    {
        Name = string.Empty;
        Sku = default!;
        Price = new Money(0m, "USD");
        CategoryId = Guid.Empty;
    }

    /// <summary>
    /// Creates a new product if invariants are satisfied.
    /// </summary>
    /// <param name="sku">Product SKU.</param>
    /// <param name="name">Product name.</param>
    /// <param name="price">Product price.</param>
    /// <param name="categoryId">Category identifier.</param>
    /// <param name="description">Optional description.</param>
    public static Result<Product> Create(Sku sku, string name, Money price, Guid categoryId, string? description = null)
    {
        try
        {
            Guard.Null(sku, nameof(sku));
            Guard.NullOrEmpty(name, nameof(name));

            if (name.Length > 200)
            {
                return Result<Product>.Failure("NAME_TOO_LONG", "Name must be 200 characters or fewer.");
            }

            if (price.Amount < 0m)
            {
                return Result<Product>.Failure("INVALID_PRICE", "Price must be greater than or equal to zero.");
            }

            if (categoryId == Guid.Empty)
            {
                return Result<Product>.Failure("CATEGORY_REQUIRED", "Category is required.");
            }

            var product = new Product
            {
                Name = name.Trim(),
                Description = description?.Trim(),
                Sku = sku,
                Price = price,
                CategoryId = categoryId,
                IsActive = false
            };

            product.RaiseDomainEvent(new ProductCreatedDomainEvent(product.Id, product.Sku, product.Name, product.Price, product.CategoryId));

            return Result<Product>.Success(product);
        }
        catch (ArgumentNullException ex)
        {
            return Result<Product>.Failure("ARGUMENT_NULL", ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result<Product>.Failure("ARGUMENT_INVALID", ex.Message);
        }
    }

    /// <summary>
    /// Activates the product.
    /// </summary>
    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        RaiseDomainEvent(new ProductActivatedDomainEvent(Id));
    }

    /// <summary>
    /// Deactivates the product.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        RaiseDomainEvent(new ProductDeactivatedDomainEvent(Id));
    }

    /// <summary>
    /// Changes the product price.
    /// </summary>
    /// <param name="newPrice">The new price.</param>
    public Result ChangePrice(Money newPrice)
    {
        Guard.Null(newPrice, nameof(newPrice));

        if (newPrice.Amount < 0m)
        {
            return Result.Failure("INVALID_PRICE", "Price must be greater than or equal to zero.");
        }

        if (Price.Amount == newPrice.Amount && Price.Currency == newPrice.Currency)
        {
            return Result.Success();
        }

        var oldPrice = Price;
        Price = newPrice;
        RaiseDomainEvent(new ProductPriceChangedDomainEvent(Id, oldPrice, newPrice));

        return Result.Success();
    }

    /// <summary>
    /// Changes the product name.
    /// </summary>
    /// <param name="name">New name.</param>
    public Result ChangeName(string name)
    {
        Guard.NullOrEmpty(name, nameof(name));

        if (name.Length > 200)
        {
            return Result.Failure("NAME_TOO_LONG", "Name must be 200 characters or fewer.");
        }

        Name = name.Trim();
        return Result.Success();
    }

    /// <summary>
    /// Changes the product description.
    /// </summary>
    /// <param name="description">New description.</param>
    public void ChangeDescription(string? description)
    {
        Description = description?.Trim();
    }

    /// <summary>
    /// Adds a new image to the product.
    /// </summary>
    /// <param name="imageUrl">Image URL.</param>
    /// <param name="displayOrder">Display order (>=0).</param>
    public ProductImage AddImage(string imageUrl, int displayOrder = 0)
    {
        var image = new ProductImage(imageUrl, displayOrder);
        _images.Add(image);
        RaiseDomainEvent(new ProductImageAddedDomainEvent(Id, image.Id));
        return image;
    }

    /// <summary>
    /// Removes an image by id.
    /// </summary>
    /// <param name="imageId">Image identifier.</param>
    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
        {
            return;
        }

        _images.Remove(image);
    }

    /// <summary>
    /// Reorders an image inside the collection.
    /// </summary>
    /// <param name="imageId">Image identifier.</param>
    /// <param name="newOrder">New display order.</param>
    public void ReorderImage(Guid imageId, int newOrder)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
        {
            return;
        }

        image.UpdateDisplayOrder(newOrder);
        // Optionally normalize display orders across collection here
    }
}
