namespace Ecommerce.Application.Catalog.DTOs;

using Ecommerce.Domain.Catalog;

/// <summary>
/// Extension methods for mapping domain entities to DTOs.
/// </summary>
public static class ProductMappingExtensions
{
    /// <summary>
    /// Maps a Product aggregate to a ProductDto.
    /// </summary>
    /// <param name="product">The product aggregate to map.</param>
    /// <returns>A ProductDto containing the product information.</returns>
    public static ProductDto ToDto(this Product product)
    {
        ArgumentNullException.ThrowIfNull(product, nameof(product));

        return new ProductDto(
            Id: product.Id,
            Sku: product.Sku.Value,
            Name: product.Name,
            Description: product.Description,
            Price: product.Price.Amount,
            Currency: product.Price.Currency,
            CategoryId: product.CategoryId,
            IsActive: product.IsActive);
    }
}
