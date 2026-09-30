namespace Ecommerce.Application.Tests.Catalog.Queries.GetProducts;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ecommerce.Application.Catalog.Queries.GetProducts;
using Ecommerce.Domain.Repositories;
using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.ValueObjects;
using Ecommerce.Application.Catalog.DTOs;

[TestClass]
public sealed class GetProductsQueryHandlerTests
{
    private Mock<IProductRepository> _repositoryMock = null!;
    private GetProductsQueryHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductsQueryHandler(_repositoryMock.Object);
    }

    [TestMethod]
    public async Task Handle_ProductsExist_ReturnsMappedProductDtoCollection()
    {
        // Arrange
        var sku1 = new Sku("P-1");
        var sku2 = new Sku("P-2");
        var product1 = Product.Create(sku1, "Product 1", new Money(10m, "USD"), Guid.NewGuid(), "Desc1").Value;
        var product2 = Product.Create(sku2, "Product 2", new Money(20.5m, "EUR"), Guid.NewGuid(), null).Value;
        var products = new List<Product> { product1, product2 };

        _repositoryMock
            .Setup(r => r.GetPagedAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.AsEnumerable());

        // Act
        var result = await _handler.Handle(new GetProductsQuery(1, 20), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var dtoList = result.ToList();
        dtoList[0].Id.Should().Be(product1.Id);
        dtoList[0].Sku.Should().Be(product1.Sku.Value);
        dtoList[0].Price.Should().Be(product1.Price.Amount);

        dtoList[1].Id.Should().Be(product2.Id);
        dtoList[1].Sku.Should().Be(product2.Sku.Value);
        dtoList[1].Price.Should().Be(product2.Price.Amount);
    }

    [TestMethod]
    public async Task Handle_NoProducts_ReturnsEmptyCollection()
    {
        // Arrange
        var empty = Enumerable.Empty<Product>().ToList();
        _repositoryMock
            .Setup(r => r.GetPagedAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(empty);

        // Act
        var result = await _handler.Handle(new GetProductsQuery(1, 20), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Handle_VerifyDtoMapping_PriceDisplayFormatIsCorrect()
    {
        // Arrange
        var sku = new Sku("P-X");
        var product = Product.Create(sku, "Product X", new Money(123.4m, "USD"), Guid.NewGuid(), null).Value;
        _repositoryMock
            .Setup(r => r.GetPagedAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        // Act
        var result = await _handler.Handle(new GetProductsQuery(1, 20), CancellationToken.None);

        // Assert
        var dto = result.Single();
        dto.PriceDisplay.Should().Be($"{dto.Currency} {dto.Price:F2}");
    }
}
