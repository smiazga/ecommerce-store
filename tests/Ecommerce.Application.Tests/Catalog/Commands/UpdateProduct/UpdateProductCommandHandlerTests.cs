namespace Ecommerce.Application.Tests.Catalog.Commands.UpdateProduct;

using Ecommerce.Application.Catalog.Commands.UpdateProduct;
using Ecommerce.Application.Common.Persistence;
using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.Repositories;
using Ecommerce.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

/// <summary>
/// Unit tests for UpdateProductCommandHandler using MSTest, Moq, and FluentAssertions.
/// Tests command orchestration, domain logic coordination, and error handling.
/// </summary>
[TestClass]
public sealed class UpdateProductCommandHandlerTests
{
    private Mock<IProductRepository> _repositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<Microsoft.Extensions.Logging.ILogger<UpdateProductCommandHandler>> _loggerMock = null!;
    private UpdateProductCommandHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<UpdateProductCommandHandler>>();
        _handler = new UpdateProductCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
    }

    #region Successful Updates

    [TestMethod]
    public async Task Handle_ProductExistsAndNameUpdated_ReturnsSuccessWithProductId()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-001"),
            "Original Name",
            new Money(99.99m, "USD"),
            categoryId).Value;

        var command = new UpdateProductCommand(
            ProductId: productId,
            Name: "Updated Name");

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existingProduct.Id);
        result.Error.Should().BeNull();

        _repositoryMock.Verify(
            r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        existingProduct.Name.Should().Be("Updated Name");
    }

    [TestMethod]
    public async Task Handle_ProductExistsAndPriceUpdated_ReturnsSuccessWithProductId()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-002"),
            "Test Product",
            new Money(50.00m, "USD"),
            categoryId).Value;

        var command = new UpdateProductCommand(
            ProductId: productId,
            Price: 75.50m,
            Currency: "USD");

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existingProduct.Id);
        existingProduct.Price.Amount.Should().Be(75.50m);
        existingProduct.Price.Currency.Should().Be("USD");
    }

    [TestMethod]
    public async Task Handle_ProductExistsAndDescriptionUpdated_ReturnsSuccessWithProductId()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-003"),
            "Product Name",
            new Money(100m, "USD"),
            categoryId,
            "Original Description").Value;

        var command = new UpdateProductCommand(
            ProductId: productId,
            Description: "Updated Description");

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingProduct.Description.Should().Be("Updated Description");
    }

    [TestMethod]
    public async Task Handle_ProductExistsAndActivatedFromInactive_ReturnsSuccessWithProductId()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-004"),
            "Product Name",
            new Money(100m, "USD"),
            categoryId).Value;

        // Product starts inactive
        existingProduct.IsActive.Should().BeFalse();

        var command = new UpdateProductCommand(
            ProductId: productId,
            IsActive: true);

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingProduct.IsActive.Should().BeTrue();
    }

    [TestMethod]
    public async Task Handle_ProductExistsAndDeactivatedFromActive_ReturnsSuccessWithProductId()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-005"),
            "Product Name",
            new Money(100m, "USD"),
            categoryId).Value;

        // Activate the product first
        existingProduct.Activate();
        existingProduct.IsActive.Should().BeTrue();

        var command = new UpdateProductCommand(
            ProductId: productId,
            IsActive: false);

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingProduct.IsActive.Should().BeFalse();
    }

    [TestMethod]
    public async Task Handle_ProductExistsAndMultipleFieldsUpdated_ReturnsSuccessWithProductId()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-006"),
            "Product Name",
            new Money(100m, "USD"),
            categoryId,
            "Original Description").Value;

        var command = new UpdateProductCommand(
            ProductId: productId,
            Name: "Updated Name",
            Description: "Updated Description",
            Price: 150.00m,
            Currency: "EUR",
            CategoryId: categoryId, // Use same category ID since handler doesn't support changing it yet
            IsActive: true);

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingProduct.Name.Should().Be("Updated Name");
        existingProduct.Description.Should().Be("Updated Description");
        existingProduct.Price.Amount.Should().Be(150.00m);
        existingProduct.Price.Currency.Should().Be("EUR");
        existingProduct.CategoryId.Should().Be(categoryId);
        existingProduct.IsActive.Should().BeTrue();
    }

    #endregion

    #region Product Not Found

    [TestMethod]
    public async Task Handle_ProductNotFound_ReturnsFailureWithProductNotFoundError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand(
            ProductId: productId,
            Name: "Updated Name");

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("PRODUCT_NOT_FOUND");
        result.Error.Message.Should().Contain(productId.ToString());

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            u => u.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Domain Validation Failures

    [TestMethod]
    public async Task Handle_InvalidNameExceedsMaxLength_ReturnsFailureWithInvalidNameError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-007"),
            "Product Name",
            new Money(100m, "USD"),
            categoryId).Value;

        // Create a name that exceeds 200 characters
        var invalidName = new string('A', 201);

        var command = new UpdateProductCommand(
            ProductId: productId,
            Name: invalidName);

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("NAME_TOO_LONG");

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            u => u.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_InvalidPriceNegative_ReturnsFailureWithInvalidProductDataError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-008"),
            "Product Name",
            new Money(100m, "USD"),
            categoryId).Value;

        var command = new UpdateProductCommand(
            ProductId: productId,
            Price: -10.00m);

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        // Negative price causes ArgumentException in Money constructor, caught as INVALID_PRODUCT_DATA
        result.Error!.Code.Should().Be("INVALID_PRODUCT_DATA");

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            u => u.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_EmptyGuidCategoryId_IgnoresAndSucceedsUpdate()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-009"),
            "Product Name",
            new Money(100m, "USD"),
            categoryId).Value;

        // Guid.Empty is silently ignored by the handler (see handler logic: request.CategoryId.Value != Guid.Empty)
        var command = new UpdateProductCommand(
            ProductId: productId,
            CategoryId: Guid.Empty);

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Empty CategoryId is silently ignored and doesn't prevent update
        result.IsSuccess.Should().BeTrue();
        existingProduct.CategoryId.Should().Be(categoryId); // CategoryId unchanged
    }

    #endregion

    #region Aggregate Methods Invocation

    [TestMethod]
    public async Task Handle_NameProvided_ChangeNameMethodIsInvoked()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-010"),
            "Original Name",
            new Money(100m, "USD"),
            categoryId).Value;

        var newName = "New Product Name";
        var command = new UpdateProductCommand(
            ProductId: productId,
            Name: newName);

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingProduct.Name.Should().Be(newName);
    }

    [TestMethod]
    public async Task Handle_PriceProvided_ChangePriceMethodIsInvoked()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-011"),
            "Product Name",
            new Money(100m, "USD"),
            categoryId).Value;

        var newPrice = 199.99m;
        var command = new UpdateProductCommand(
            ProductId: productId,
            Price: newPrice,
            Currency: "USD");

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingProduct.Price.Amount.Should().Be(newPrice);
        existingProduct.Price.Currency.Should().Be("USD");
    }

    [TestMethod]
    public async Task Handle_DescriptionProvided_ChangeDescriptionMethodIsInvoked()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-012"),
            "Product Name",
            new Money(100m, "USD"),
            categoryId,
            "Original Description").Value;

        var newDescription = "New Description";
        var command = new UpdateProductCommand(
            ProductId: productId,
            Description: newDescription);

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingProduct.Description.Should().Be(newDescription);
    }

    [TestMethod]
    public async Task Handle_ActivateTrue_ActivateMethodIsInvoked()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-013"),
            "Product Name",
            new Money(100m, "USD"),
            categoryId).Value;

        existingProduct.IsActive.Should().BeFalse();

        var command = new UpdateProductCommand(
            ProductId: productId,
            IsActive: true);

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingProduct.IsActive.Should().BeTrue();
    }

    [TestMethod]
    public async Task Handle_DeactivateFalse_DeactivateMethodIsInvoked()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-014"),
            "Product Name",
            new Money(100m, "USD"),
            categoryId).Value;

        existingProduct.Activate();
        existingProduct.IsActive.Should().BeTrue();

        var command = new UpdateProductCommand(
            ProductId: productId,
            IsActive: false);

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingProduct.IsActive.Should().BeFalse();
    }

    #endregion

    #region Repository and Unit of Work Lifecycle

    [TestMethod]
    public async Task Handle_SuccessfulUpdate_CallsRepositoryUpdateAsyncBeforeSaveAsync()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-015"),
            "Product Name",
            new Money(100m, "USD"),
            categoryId).Value;

        var command = new UpdateProductCommand(
            ProductId: productId,
            Name: "Updated Name");

        var callOrder = new List<string>();

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("Update"))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("Save"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        callOrder.Should().HaveCount(2);
        callOrder[0].Should().Be("Update");
        callOrder[1].Should().Be("Save");
    }

    [TestMethod]
    public async Task Handle_SuccessfulUpdate_UpdateAsyncIsCalledOnce()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-016"),
            "Product Name",
            new Money(100m, "USD"),
            categoryId).Value;

        var command = new UpdateProductCommand(
            ProductId: productId,
            Name: "Updated Name");

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Handle_SuccessfulUpdate_SaveAsyncIsCalledOnce()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROD-017"),
            "Product Name",
            new Money(100m, "USD"),
            categoryId).Value;

        var command = new UpdateProductCommand(
            ProductId: productId,
            Name: "Updated Name");

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(
            u => u.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Error Handling

    [TestMethod]
    public async Task Handle_OperationCancelled_ReturnsFailureWithOperationCancelledError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand(
            ProductId: productId,
            Name: "Updated Name");

        var cancellationToken = new CancellationToken(canceled: true);

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("OPERATION_CANCELLED");
    }

    [TestMethod]
    public async Task Handle_ArgumentExceptionThrown_ReturnsFailureWithInvalidProductDataError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand(
            ProductId: productId,
            Name: "Updated Name");

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid argument"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("INVALID_PRODUCT_DATA");
    }

    [TestMethod]
    public async Task Handle_UnexpectedExceptionThrown_ReturnsFailureWithProductUpdateErrorAndLogs()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand(
            ProductId: productId,
            Name: "Updated Name");

        var exception = new InvalidOperationException("Unexpected error");

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("PRODUCT_UPDATE_ERROR");

        _loggerMock.Verify(
            l => l.Log(
                It.IsAny<Microsoft.Extensions.Logging.LogLevel>(),
                It.IsAny<Microsoft.Extensions.Logging.EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Handle_NullCommand_ThrowsArgumentNullException()
    {
        // Act
        var action = async () => await _handler.Handle(null!, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Validation Edge Cases

    [TestMethod]
    public async Task Handle_PriceZero_SuccessfullyUpdates()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create(
            new Sku("PROMO-001"),
            "Promotional Product",
            new Money(100m, "USD"),
            categoryId).Value;

        var command = new UpdateProductCommand(
            ProductId: productId,
            Price: 0m);

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingProduct.Price.Amount.Should().Be(0m);
    }

    [TestMethod]
    public async Task Handle_WhitespaceOnlyNameNotProvided_DoesNotUpdateName()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var originalName = "Original Name";
        var existingProduct = Product.Create(
            new Sku("PROD-018"),
            originalName,
            new Money(100m, "USD"),
            categoryId).Value;

        // Whitespace-only name is treated as not provided
        var command = new UpdateProductCommand(
            ProductId: productId,
            Name: "   ");

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingProduct.Name.Should().Be(originalName);
    }

    [TestMethod]
    public async Task Handle_SamePriceAsOriginal_UpdatesSuccessfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var price = 100m;
        var existingProduct = Product.Create(
            new Sku("PROD-019"),
            "Product Name",
            new Money(price, "USD"),
            categoryId).Value;

        var command = new UpdateProductCommand(
            ProductId: productId,
            Price: price,
            Currency: "USD");

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
