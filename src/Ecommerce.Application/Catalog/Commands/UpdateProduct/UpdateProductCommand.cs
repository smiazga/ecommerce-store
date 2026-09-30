namespace Ecommerce.Application.Catalog.Commands.UpdateProduct;

using MediatR;
using Ecommerce.SharedKernel.Results;

/// <summary>
/// CQRS command to update an existing product in the catalog.
/// </summary>
public sealed record UpdateProductCommand(
    Guid ProductId,
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    string? Currency = null,
    Guid? CategoryId = null,
    bool? IsActive = null) : IRequest<Result<Guid>>;
