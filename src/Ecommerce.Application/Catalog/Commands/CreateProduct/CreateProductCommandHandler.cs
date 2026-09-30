namespace Ecommerce.Application.Catalog.Commands.CreateProduct;

using MediatR;
using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.Repositories;
using Ecommerce.Domain.ValueObjects;
using Ecommerce.SharedKernel.Results;
using Ecommerce.Application.Common.Persistence;

/// <summary>
/// Handles the creation of a new product in the catalog.
/// Coordinates domain logic, validation, and persistence.
/// </summary>
public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="CreateProductCommandHandler"/>.
    /// </summary>
    /// <param name="repository">Product repository abstraction for persistence.</param>
    /// <param name="unitOfWork">Unit of Work for managing transaction boundaries.</param>
    public CreateProductCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(repository, nameof(repository));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the CreateProductCommand request.
    /// Orchestrates domain creation, validation, and persistence in a single transaction.
    /// </summary>
    /// <param name="request">The create product command.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>
    /// A Result containing the created product's ID on success,
    /// or a failure result with error details on failure.
    /// </returns>
    public async Task<Result<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        try
        {
            // Map command inputs to domain value objects
            var sku = new Sku(request.Sku);
            var price = new Money(request.Price, request.Currency);

            // Invoke domain creation factory - encapsulates business invariants
            var productResult = Product.Create(
                sku,
                request.Name,
                price,
                request.CategoryId,
                request.Description);

            // Handle domain validation failure
            if (!productResult.IsSuccess)
            {
                return Result<Guid>.Failure(
                    productResult.Error?.Code ?? "PRODUCT_CREATION_FAILED",
                    productResult.Error?.Message ?? "Failed to create product due to business rule violation.");
            }

            // Extract successfully created product
            var product = productResult.Value;
            ArgumentNullException.ThrowIfNull(product, nameof(product));

            // Check if product with same SKU already exists (business constraint)
            var existingProduct = await _repository.GetBySkuAsync(sku, cancellationToken);
            if (existingProduct is not null)
            {
                return Result<Guid>.Failure(
                    "SKU_ALREADY_EXISTS",
                    "A product with this SKU already exists in the catalog.");
            }

            // Persist product aggregate via repository abstraction
            // Handler determines transaction boundary and commits changes via Unit of Work
            await _repository.AddAsync(product, cancellationToken);

            // Commit all pending changes in a single transaction (per ADR-002)
            await _unitOfWork.SaveAsync(cancellationToken);

            // Return success with created product ID
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
                "Product creation operation was cancelled.");
        }
        catch (Exception ex)
        {
            // Log unexpected exceptions in production
            return Result<Guid>.Failure(
                "PRODUCT_CREATION_ERROR",
                "An unexpected error occurred while creating the product.");
        }
    }
}
