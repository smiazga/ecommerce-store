namespace Ecommerce.Application.Catalog.Queries.GetProductById;

using MediatR;
using Ecommerce.Application.Catalog.DTOs;
using Ecommerce.Domain.Repositories;

/// <summary>
/// Handler for GetProductByIdQuery.
/// </summary>
public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _repository;

    public GetProductByIdQueryHandler(IProductRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null) return null;

        return product.ToDto();
    }
}
