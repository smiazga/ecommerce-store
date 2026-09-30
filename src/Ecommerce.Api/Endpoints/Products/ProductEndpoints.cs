namespace Ecommerce.Api.Endpoints.Products;

using MediatR;
using Ecommerce.Application.Catalog.Commands.CreateProduct;
using Ecommerce.Application.Catalog.DTOs;
using Ecommerce.Application.Catalog.Queries.GetProductById;
using Ecommerce.Application.Catalog.Queries.GetProducts;
using Ecommerce.Contracts.Catalog;
using Ecommerce.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Registers product-related endpoints.
/// </summary>
public static class ProductEndpoints
{
    /// <summary>
    /// Maps product endpoints to the application.
    /// </summary>
    /// <param name="app">The WebApplication to map endpoints to.</param>
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapPost("/", CreateProductAsync)
            .WithName("CreateProduct")
            .WithDescription("Creates a new product in the catalog")
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetProductByIdAsync)
            .WithName("GetProductById")
            .WithDescription("Retrieves a product by its identifier")
            .Produces<ProductDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi();

        group.MapGet("/", GetProductsAsync)
            .WithName("GetProducts")
            .WithDescription("Retrieves a paged list of products")
            .Produces<IEnumerable<ProductDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi();
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The product creation request.</param>
    /// <param name="sender">MediatR ISender for command handling.</param>
    /// <param name="repository">Product repository for fetching created product.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>
    /// 201 Created: Product successfully created with response containing product details.
    /// 400 Bad Request: Validation failed or business rule violation.
    /// 500 Internal Server Error: Unexpected server error.
    /// </returns>
    private static async Task<IResult> CreateProductAsync(
        CreateProductRequest request,
        ISender sender,
        IProductRepository repository,
        CancellationToken cancellationToken)
    {
        // Guard clause - validate request is not null (should not happen in production, but defensive programming)
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        // Create command from request DTO
        var command = new CreateProductCommand(
            request.Sku,
            request.Name,
            request.Price,
            request.CategoryId,
            request.Description,
            request.Currency);

        // Send command through MediatR pipeline
        // Pipeline includes: Validation -> Handler
        var result = await sender.Send(command, cancellationToken);

        // Handle command failure
        if (!result.IsSuccess)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Type = "https://api.example.com/errors/validation-failed",
                Title = "Product Creation Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = result.Error?.Message ?? "Failed to create product",
                Instance = "POST /api/products",
                Extensions = new Dictionary<string, object?>
                {
                    { "errorCode", result.Error?.Code ?? "UNKNOWN" }
                }
            });
        }

        // Extract product ID from successful result
        var productId = result.Value;

        // Fetch created product from repository for response
        var product = await repository.GetByIdAsync(productId, cancellationToken);

        // Map domain entity to response DTO
        var response = ProductToResponse(product);

        // Return 201 Created with Location header and response body
        return Results.Created($"/api/products/{productId}", response);
    }

    /// <summary>
    /// Retrieves a product by id through MediatR query.
    /// </summary>
    private static async Task<IResult> GetProductByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var dto = await sender.Send(new GetProductByIdQuery(id), cancellationToken);
        if (dto is null)
        {
            return Results.NotFound(new ProblemDetails
            {
                Type = "https://api.example.com/errors/not-found",
                Title = "Product Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Product with id '{id}' was not found.",
                Instance = $"GET /api/products/{id}"
            });
        }

        return Results.Ok(dto);
    }

    /// <summary>
    /// Retrieves a paged list of products through MediatR query.
    /// </summary>
    private static async Task<IResult> GetProductsAsync(
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        ISender sender,
        CancellationToken cancellationToken)
    {
        // Provide sensible defaults if not supplied
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 20;

        var products = await sender.Send(new GetProductsQuery(pageNumber, pageSize), cancellationToken);

        return Results.Ok(products);
    }

    /// <summary>
    /// Maps a Product domain entity to a CreateProductResponse DTO.
    /// </summary>
    /// <param name="product">The product domain entity.</param>
    /// <returns>API response DTO.</returns>
    private static CreateProductResponse ProductToResponse(Domain.Catalog.Product product)
    {
        ArgumentNullException.ThrowIfNull(product, nameof(product));

        return new CreateProductResponse(
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
