namespace Ecommerce.Application.Catalog.Queries.GetProducts;

using MediatR;
using Ecommerce.Application.Catalog.DTOs;

/// <summary>
/// Query to retrieve a paged list of products.
/// </summary>
public sealed record GetProductsQuery(int PageNumber = 1, int PageSize = 20) : IRequest<IEnumerable<ProductDto>>;
