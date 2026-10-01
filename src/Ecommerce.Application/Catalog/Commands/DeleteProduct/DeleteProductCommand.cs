namespace Ecommerce.Application.Catalog.Commands.DeleteProduct;

using MediatR;
using Ecommerce.SharedKernel.Results;

/// <summary>
/// Command to delete (deactivate) a product.
/// </summary>
public sealed record DeleteProductCommand(Guid ProductId) : IRequest<Result<Guid>>;
