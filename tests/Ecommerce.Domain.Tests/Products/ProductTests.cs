namespace Ecommerce.Domain.Tests.Products;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.ValueObjects;
using Ecommerce.Domain.Catalog.Events;
using Ecommerce.SharedKernel.Results;

[TestClass]
public sealed class ProductTests
{
    [TestMethod]
    public void Create_ValidInputs_ReturnsProduct()
    {
        // Arrange
        var sku = new Sku("SKU-123");
        var price = new Money(19.99m, "USD");
        var categoryId = Guid.NewGuid();

        // Act
        var result = Product.Create(sku, "Test Product", price, categoryId, "Description");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var product = result.Unwrap();
        product.Sku.Value.Should().Be("SKU-123");
        product.Name.Should().Be("Test Product");
        product.Price.Amount.Should().Be(19.99m);
        product.CategoryId.Should().Be(categoryId);
        product.IsActive.Should().BeFalse();
    }

    [TestMethod]
    public void Create_InvalidName_ReturnsFailure()
    {
        // Arrange
        var sku = new Sku("SKU-1");
        var price = new Money(10m, "USD");
        var categoryId = Guid.NewGuid();

        // Act
        var result = Product.Create(sku, string.Empty, price, categoryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [TestMethod]
    public void Create_NullSku_ReturnsFailure()
    {
        // Arrange
        Sku? sku = null;
        var price = new Money(10m, "USD");
        var categoryId = Guid.NewGuid();

        // Act
        var result = Product.Create(sku!, "Name", price, categoryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error?.Code.Should().Be("ARGUMENT_NULL");
    }

    [TestMethod]
    public void Money_NegativeAmount_ThrowsArgumentOutOfRange()
    {
        // Act
        var act = () => new Money(-1m, "USD");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void Activate_WhenInactive_SetsIsActiveAndRaisesEvent()
    {
        // Arrange
        var sku = new Sku("SKU-4");
        var price = new Money(5m, "USD");
        var product = Product.Create(sku, "P", price, Guid.NewGuid()).Unwrap();

        // Act
        product.Activate();

        // Assert
        product.IsActive.Should().BeTrue();
        product.DomainEvents.Should().ContainSingle(e => e.GetType() == typeof(ProductActivatedDomainEvent));
    }

    [TestMethod]
    public void Deactivate_WhenActive_SetsIsActiveAndRaisesEvent()
    {
        // Arrange
        var sku = new Sku("SKU-5");
        var price = new Money(5m, "USD");
        var product = Product.Create(sku, "P", price, Guid.NewGuid()).Unwrap();
        product.Activate();
        product.ClearDomainEvents();

        // Act
        product.Deactivate();

        // Assert
        product.IsActive.Should().BeFalse();
        product.DomainEvents.Should().ContainSingle(e => e.GetType() == typeof(ProductDeactivatedDomainEvent));
    }

    [TestMethod]
    public void ChangePrice_ValidPrice_UpdatesPriceAndRaisesEvent()
    {
        // Arrange
        var sku = new Sku("SKU-6");
        var price = new Money(10m, "USD");
        var product = Product.Create(sku, "P", price, Guid.NewGuid()).Unwrap();

        // Act
        var result = product.ChangePrice(new Money(15m, "USD"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Price.Amount.Should().Be(15m);
        product.DomainEvents.Should().ContainSingle(e => e.GetType() == typeof(ProductPriceChangedDomainEvent));
    }
}
