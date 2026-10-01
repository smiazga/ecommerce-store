namespace Ecommerce.Application.Catalog.Commands.DeleteProduct;

using Ecommerce.Application.Common.Persistence;
using Ecommerce.Domain.Repositories;
using Ecommerce.SharedKernel.Results;

using MediatR;

using Microsoft.Extensions.Logging;

/// <summary>
/// Handles deletion (deactivation) of a product aggregate.
/// Uses aggregate behavior to deactivate and raises domain events.
/// </summary>
public sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<Guid>>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        IProductRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteProductCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(repository, nameof(repository));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        try
        {
            var product = await _repository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
            {
                return Result<Guid>.Failure(
                    "PRODUCT_NOT_FOUND",
                    $"Product with ID '{request.ProductId}' was not found.");
            }

            // Use aggregate behavior to deactivate (soft-delete)
            product.Deactivate();

            // Persist changes through repository and unit of work
            await _repository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return Result<Guid>.Success(product.Id);
        }
        catch (OperationCanceledException)
        {
            return Result<Guid>.Failure("OPERATION_CANCELLED", "Product delete operation was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting product (ProductId={ProductId})", request.ProductId);
            return Result<Guid>.Failure("PRODUCT_DELETE_ERROR", "An unexpected error occurred while deleting the product.");
        }
    }
}
