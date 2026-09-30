namespace Ecommerce.Application.Tests.Catalog.Commands.CreateProduct;

using Ecommerce.Application.Catalog.Commands.CreateProduct;
using Ecommerce.Application.Common.Persistence;
using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.Repositories;
using Ecommerce.Domain.ValueObjects;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

/// <summary>
/// Unit tests for CreateProductCommandHandler using MSTest, Moq, and FluentAssertions.
/// Tests command orchestration, domain logic coordination, and error handling.
/// </summary>
[TestClass]
public sealed class CreateProductCommandHandlerTests
{
    private Mock<IProductRepository> _repositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private CreateProductCommandHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateProductCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Successful Product Creation

    [TestMethod]
    public async Task Handle_ValidCommand_ReturnsSuccessWithProductId()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "PROD-001",
            Name: "Laptop",
            Price: 999.99m,
            CategoryId: Guid.NewGuid(),
            Description: "High-performance laptop",
            Currency: "USD");

        _repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        result.Error.Should().BeNull();

        _repositoryMock.Verify(
            r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Handle_MinimalValidCommand_ReturnsSuccessWithProductId()
    {
        // Arrange - only required fields
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand(
            Sku: "PROD-002",
            Name: "Mouse",
            Price: 29.99m,
            CategoryId: categoryId);

        _repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [TestMethod]
    public async Task Handle_ValidCommandWithoutDescription_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "PROD-003",
            Name: "Keyboard",
            Price: 79.99m,
            CategoryId: Guid.NewGuid(),
            Description: null);

        _repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [TestMethod]
    public async Task Handle_ValidCommandWithDefaultCurrency_ReturnsSuccess()
    {
        // Arrange - uses default USD currency
        var command = new CreateProductCommand(
            Sku: "PROD-004",
            Name: "Monitor",
            Price: 299.99m,
            CategoryId: Guid.NewGuid());

        _repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [TestMethod]
    public async Task Handle_ValidCommandWithZeroPrice_ReturnsSuccess()
    {
        // Arrange - price of zero is valid
        var command = new CreateProductCommand(
            Sku: "PROMO-001",
            Name: "Free Sample",
            Price: 0m,
            CategoryId: Guid.NewGuid());

        _repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Duplicate SKU Handling

    [TestMethod]
    public async Task Handle_DuplicateSku_ReturnsFailureWithSkuAlreadyExistsError()
    {
        // Arrange
        var sku = new Sku("DUPLICATE-SKU");
        var existingProduct = Product.Create(
            sku,
            "Existing Product",
            new Money(50m, "USD"),
            Guid.NewGuid()).Value;

        var command = new CreateProductCommand(
            Sku: "DUPLICATE-SKU",
            Name: "New Product",
            Price: 99.99m,
            CategoryId: Guid.NewGuid());

        _repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("SKU_ALREADY_EXISTS");
        result.Error.Message.Should().Contain("SKU", "product");

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            u => u.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_DuplicateSkuCheckFails_DoesNotPersistProduct()
    {
        // Arrange
        var sku = new Sku("DUPLICATE-001");
        var existingProduct = Product.Create(
            sku,
            "Existing",
            new Money(10m, "USD"),
            Guid.NewGuid()).Value;

        var command = new CreateProductCommand(
            Sku: "DUPLICATE-001",
            Name: "New",
            Price: 20m,
            CategoryId: Guid.NewGuid());

        _repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Verify AddAsync is never called when SKU duplicate detected
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Product should not be persisted when duplicate SKU is found");

        _unitOfWorkMock.Verify(
            u => u.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Never,
            "UnitOfWork.SaveAsync should not be called when duplicate SKU is found");
    }

    #endregion

    #region Invalid Value Objects (Domain Validation)

    [TestMethod]
    public async Task Handle_InvalidSkuFormat_ReturnsFailureWithErrorCode()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "", // Empty SKU is invalid (Sku throws ArgumentException)
            Name: "Product",
            Price: 100m,
            CategoryId: Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("INVALID_PRODUCT_DATA");
    }

    [TestMethod]
    public async Task Handle_NegativePrice_ReturnsFailureWithInvalidPriceError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "PROD-NEG",
            Name: "Product",
            Price: -10m, // Negative price is invalid (Money throws ArgumentOutOfRangeException)
            CategoryId: Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("INVALID_PRODUCT_DATA");
    }

    [TestMethod]
    public async Task Handle_InvalidCurrency_ReturnsFailureWithErrorCode()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "PROD-005",
            Name: "Product",
            Price: 50m,
            CategoryId: Guid.NewGuid(),
            Currency: ""); // Empty currency is invalid

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("INVALID_PRODUCT_DATA");
    }

    [TestMethod]
    public async Task Handle_EmptyName_ReturnsFailureWithErrorCode()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "PROD-006",
            Name: "", // Empty name is invalid (Domain.Product.Create fails)
            Price: 50m,
            CategoryId: Guid.NewGuid());

        _repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [TestMethod]
    public async Task Handle_NameExceeds200Characters_ReturnsFailureWithNameTooLongError()
    {
        // Arrange
        var longName = new string('A', 201); // Exceeds 200 char limit
        var command = new CreateProductCommand(
            Sku: "PROD-007",
            Name: longName,
            Price: 50m,
            CategoryId: Guid.NewGuid());

        _repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("NAME_TOO_LONG");
    }

    [TestMethod]
    public async Task Handle_EmptyCategoryId_ReturnsFailureWithCategoryRequiredError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "PROD-008",
            Name: "Product",
            Price: 50m,
            CategoryId: Guid.Empty); // Invalid empty GUID

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("CATEGORY_REQUIRED");
    }

    #endregion

    #region Repository Failures

    [TestMethod]
    public async Task Handle_GetBySkuAsyncThrows_ReturnsFailureWithGenericErrorCode()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "PROD-009",
            Name: "Product",
            Price: 50m,
            CategoryId: Guid.NewGuid());

        _repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("PRODUCT_CREATION_ERROR");
    }

    [TestMethod]
    public async Task Handle_AddAsyncThrows_ReturnsFailureAndDoesNotPropagate()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "PROD-010",
            Name: "Product",
            Price: 50m,
            CategoryId: Guid.NewGuid());

        _repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("PRODUCT_CREATION_ERROR");
    }

    #endregion

    #region Cancellation Token Handling

    [TestMethod]
    public async Task Handle_CancellationTokenCancelled_ReturnsFailureWithOperationCancelledError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "PROD-011",
            Name: "Product",
            Price: 50m,
            CategoryId: Guid.NewGuid());

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException("Operation was cancelled"));

        // Act
        var result = await _handler.Handle(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("OPERATION_CANCELLED");
    }

    [TestMethod]
    public async Task Handle_CancellationSignalledDuringPersist_ReturnsFailureWithOperationCancelledError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "PROD-012",
            Name: "Product",
            Price: 50m,
            CategoryId: Guid.NewGuid());

        var cancellationTokenSource = new CancellationTokenSource();

        _repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException("Operation was cancelled"));

        // Act
        var result = await _handler.Handle(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("OPERATION_CANCELLED");
    }

    #endregion

    #region Null Guard Handling

    [TestMethod]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new CreateProductCommandHandler(null!, new Mock<IUnitOfWork>().Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void Constructor_NullUnitOfWork_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new CreateProductCommandHandler(_repositoryMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Repository Interaction Verification

    [TestMethod]
    public async Task Handle_ValidCommand_CallsRepositoryMethodsInCorrectOrder()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "PROD-013",
            Name: "Product",
            Price: 50m,
            CategoryId: Guid.NewGuid());

        var callOrder = new List<string>();

        _repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("GetBySku"))
            .ReturnsAsync((Product?)null);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("Add"))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("Save"))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().Equal("GetBySku", "Add", "Save");
    }

    [TestMethod]
    public async Task Handle_ValidCommand_PassesCancellationTokenToRepository()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "PROD-014",
            Name: "Product",
            Price: 50m,
            CategoryId: Guid.NewGuid());

        var cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;

        _repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, token);

        // Assert
        _repositoryMock.Verify(
            r => r.GetBySkuAsync(It.IsAny<Sku>(), token),
            Times.Once);

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Product>(), token),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveAsync(token),
            Times.Once);
    }

    #endregion
}
