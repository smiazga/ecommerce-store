namespace Ecommerce.Application.Catalog.Queries.GetProductById;

using MediatR;
using Ecommerce.Application.Catalog.DTOs;

/// <summary>
/// Query to retrieve a product by its identifier.
/// </summary>
public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
