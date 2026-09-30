namespace Ecommerce.Application.Tests.Catalog.Queries.GetProductById;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ecommerce.Application.Catalog.Queries.GetProductById;
using Ecommerce.Domain.Repositories;
using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.ValueObjects;
using Ecommerce.Application.Catalog.DTOs;

[TestClass]
public sealed class GetProductByIdQueryHandlerTests
{
    private Mock<IProductRepository> _repositoryMock = null!;
    private GetProductByIdQueryHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductByIdQueryHandler(_repositoryMock.Object);
    }

    [TestMethod]
    public async Task Handle_ProductExists_ReturnsProductDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sku = new Sku("PROD-100");
        var product = Product.Create(sku, "Test Product", new Money(19.95m, "USD"), Guid.NewGuid(), "Desc").Value;
        // ensure product uses our expected id
        var productWithId = product; // product.Id is set by domain factory

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productWithId);

        // Act
        var result = await _handler.Handle(new GetProductByIdQuery(id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productWithId.Id);
        result.Sku.Should().Be(productWithId.Sku.Value);
        result.Name.Should().Be(productWithId.Name);
        result.Description.Should().Be(productWithId.Description);
        result.Price.Should().Be(productWithId.Price.Amount);
        result.Currency.Should().Be(productWithId.Price.Currency);
        result.CategoryId.Should().Be(productWithId.CategoryId);
        result.IsActive.Should().Be(productWithId.IsActive);
    }

    [TestMethod]
    public async Task Handle_ProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(new GetProductByIdQuery(id), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var act = () => _handler.Handle(new GetProductByIdQuery(id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
