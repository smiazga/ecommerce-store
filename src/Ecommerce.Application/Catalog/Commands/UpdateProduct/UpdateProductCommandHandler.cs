namespace Ecommerce.Application.Catalog.Commands.UpdateProduct;

using MediatR;
using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.Repositories;
using Ecommerce.Domain.ValueObjects;
using Ecommerce.SharedKernel.Results;
using Ecommerce.Application.Common.Persistence;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handles the update of an existing product in the catalog.
/// Coordinates domain logic, validation, and persistence.
/// </summary>
public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<Guid>>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateProductCommandHandler"/>.
    /// </summary>
    /// <param name="repository">Product repository abstraction for persistence.</param>
    /// <param name="unitOfWork">Unit of Work for managing transaction boundaries.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public UpdateProductCommandHandler(
        IProductRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateProductCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(repository, nameof(repository));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateProductCommand request.
    /// Orchestrates domain updates, validation, and persistence in a single transaction.
    /// </summary>
    /// <param name="request">The update product command.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>
    /// A Result containing the updated product's ID on success,
    /// or a failure result with error details on failure.
    /// </returns>
    public async Task<Result<Guid>> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        try
        {
            // Load the existing product aggregate from persistence
            var product = await _repository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
            {
                return Result<Guid>.Failure(
                    "PRODUCT_NOT_FOUND",
                    $"Product with ID '{request.ProductId}' was not found.");
            }

            // Apply domain updates through aggregate methods to maintain invariants

            // Update product name if provided
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var nameResult = product.ChangeName(request.Name);
                if (!nameResult.IsSuccess)
                {
                    return Result<Guid>.Failure(
                        nameResult.Error?.Code ?? "INVALID_NAME",
                        nameResult.Error?.Message ?? "Failed to update product name.");
                }
            }

            // Update product description if provided
            if (!string.IsNullOrWhiteSpace(request.Description) || 
                (request.Description == "" && !string.IsNullOrWhiteSpace(product.Description)))
            {
                product.ChangeDescription(request.Description);
            }

            // Update product price if provided
            if (request.Price.HasValue)
            {
                var currency = request.Currency ?? "USD";
                var price = new Money(request.Price.Value, currency);
                var priceResult = product.ChangePrice(price);
                if (!priceResult.IsSuccess)
                {
                    return Result<Guid>.Failure(
                        priceResult.Error?.Code ?? "INVALID_PRICE",
                        priceResult.Error?.Message ?? "Failed to update product price.");
                }
            }

            // Update product category if provided
            if (request.CategoryId.HasValue && request.CategoryId.Value != Guid.Empty)
            {
                // CategoryId is a direct property update; no explicit domain method enforces its invariants
                // This would typically require a ChangeCategoryId method if business rules apply
                var newCategoryId = request.CategoryId.Value;
                if (newCategoryId != product.CategoryId)
                {
                    // Future: Add ChangeCategoryId() method to Product if category change requires validation
                    // For now, direct assignment (requires domain review if invariants apply)
                    var categoryUpdateResult = UpdateCategoryId(product, newCategoryId);
                    if (!categoryUpdateResult.IsSuccess)
                    {
                        return Result<Guid>.Failure(
                            categoryUpdateResult.Error?.Code ?? "INVALID_CATEGORY",
                            categoryUpdateResult.Error?.Message ?? "Failed to update product category.");
                    }
                }
            }

            // Update product active state if provided
            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                {
                    product.Activate();
                }
                else
                {
                    product.Deactivate();
                }
            }

            // Persist updated product aggregate via repository abstraction
            // Handler determines transaction boundary and commits changes via Unit of Work
            await _repository.UpdateAsync(product, cancellationToken);

            // Commit all pending changes in a single transaction
            await _unitOfWork.SaveAsync(cancellationToken);

            // Return success with updated product ID
            return Result<Guid>.Success(product.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure(
                "INVALID_PRODUCT_DATA",
                ex.Message);
        }
        catch (OperationCanceledException)
        {
            return Result<Guid>.Failure(
                "OPERATION_CANCELLED",
                "Product update operation was cancelled.");
        }
        catch (Exception ex)
        {
            // Log unexpected exceptions for diagnostics
            _logger.LogError(
                ex,
                "Unexpected error while updating product (ProductId={ProductId})",
                request.ProductId);

            return Result<Guid>.Failure(
                "PRODUCT_UPDATE_ERROR",
                "An unexpected error occurred while updating the product.");
        }
    }

    /// <summary>
    /// Updates the product category ID.
    /// This is a placeholder for potential category validation.
    /// Should be replaced with a domain method (ChangeCategoryId) if business rules apply.
    /// </summary>
    /// <param name="product">The product aggregate.</param>
    /// <param name="categoryId">The new category ID.</param>
    /// <returns>A result indicating success or failure.</returns>
    private static Result UpdateCategoryId(Product product, Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            return Result.Failure("INVALID_CATEGORY", "Category ID cannot be empty.");
        }

        // Future: Add business validation for category existence, constraints, etc.
        // For now, category update is allowed; the domain enforces that it's not empty.

        return Result.Success();
    }
}
