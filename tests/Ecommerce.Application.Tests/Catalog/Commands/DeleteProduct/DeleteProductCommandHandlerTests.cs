using Ecommerce.Application.Catalog.Commands.DeleteProduct;
using Ecommerce.Application.Common.Persistence;
using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.Repositories;

using Moq;

namespace Ecommerce.Application.Tests.Catalog.Commands.DeleteProduct;

[TestClass]
public class DeleteProductCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_ProductExists_DeactivatesAndSaves()
    {
        // Arrange
        var product = CreateActiveProduct();

        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        Product? captured = null;
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => captured = p)
            .Returns(Task.CompletedTask);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<DeleteProductCommandHandler>>();

        var handler = new DeleteProductCommandHandler(repoMock.Object, uowMock.Object, loggerMock.Object);

        var command = new DeleteProductCommand(product.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(captured);
        Assert.IsFalse(captured!.IsActive);
        uowMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_ProductNotFound_ReturnsFailure()
    {
        // Arrange
        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var uowMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<DeleteProductCommandHandler>>();

        var handler = new DeleteProductCommandHandler(repoMock.Object, uowMock.Object, loggerMock.Object);

        var command = new DeleteProductCommand(Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("PRODUCT_NOT_FOUND", result.Error?.Code);
    }

    private static Product CreateActiveProduct()
    {
        var sku = new Ecommerce.Domain.ValueObjects.Sku("SKU-1");
        var money = new Ecommerce.Domain.ValueObjects.Money(10m, "USD");
        var result = Product.Create(sku, "Name", money, Guid.NewGuid(), "desc");
        if (!result.IsSuccess) throw new InvalidOperationException("Failed to create product for test");
        var product = result.Value;
        product.Activate();
        return product;
    }
}
