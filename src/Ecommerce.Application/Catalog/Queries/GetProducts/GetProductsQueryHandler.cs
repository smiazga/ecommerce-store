namespace Ecommerce.Application.Catalog.Queries.GetProducts;

using MediatR;
using Ecommerce.Application.Catalog.DTOs;
using Ecommerce.Domain.Repositories;

/// <summary>
/// Handler for GetProductsQuery.
/// </summary>
public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductRepository _repository;

    public GetProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        if (request.PageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(request.PageNumber));
        if (request.PageSize <= 0) throw new ArgumentOutOfRangeException(nameof(request.PageSize));

        var products = await _repository.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);

        return products.Select(p => p.ToDto());
    }
}
