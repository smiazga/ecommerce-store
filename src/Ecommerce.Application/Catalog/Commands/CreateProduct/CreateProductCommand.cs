namespace Ecommerce.Application.Catalog.Commands.CreateProduct;

using MediatR;
using Ecommerce.SharedKernel.Results;

/// <summary>
/// CQRS command to create a new product in the catalog.
/// </summary>
public sealed record CreateProductCommand(
    string Sku,
    string Name,
    decimal Price,
    Guid CategoryId,
    string? Description = null,
    string Currency = "USD") : IRequest<Result<Guid>>;
