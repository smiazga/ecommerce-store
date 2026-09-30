namespace Ecommerce.Application.Tests.Catalog.Commands.CreateProduct;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ecommerce.Application.Catalog.Commands.CreateProduct;

/// <summary>
/// Unit tests for CreateProductCommandValidator using FluentValidation.
/// Tests request-level validation constraints.
/// </summary>
[TestClass]
public sealed class CreateProductCommandValidatorTests
{
    private CreateProductCommandValidator _validator = null!;

    [TestInitialize]
    public void Setup()
    {
        _validator = new CreateProductCommandValidator();
    }

    #region Valid Commands

    [TestMethod]
    public void Validate_ValidCommand_ReturnsNoErrors()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "PROD-001",
            Name: "Laptop",
            Price: 999.99m,
            CategoryId: Guid.NewGuid(),
            Description: "A powerful laptop",
            Currency: "USD");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public void Validate_MinimalValidCommand_ReturnsNoErrors()
    {
        // Arrange - only required fields
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void Validate_ZeroPrice_ReturnsNoErrors()
    {
        // Arrange - zero price is valid
        var command = new CreateProductCommand(
            Sku: "FREE-001",
            Name: "Free Product",
            Price: 0m,
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void Validate_MaxLengthSku_ReturnsNoErrors()
    {
        // Arrange
        var maxLengthSku = new string('A', 100);
        var command = new CreateProductCommand(
            Sku: maxLengthSku,
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void Validate_MaxLengthName_ReturnsNoErrors()
    {
        // Arrange
        var maxLengthName = new string('A', 200);
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: maxLengthName,
            Price: 10m,
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void Validate_NullDescription_ReturnsNoErrors()
    {
        // Arrange - null description is valid (optional field)
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid(),
            Description: null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void Validate_EmptyStringDescription_ReturnsNoErrors()
    {
        // Arrange - empty description is not validated (only non-null/whitespace strings)
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid(),
            Description: "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void Validate_WhitespaceDescription_ReturnsNoErrors()
    {
        // Arrange - whitespace description is not validated
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid(),
            Description: "   ");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region SKU Validation

    [TestMethod]
    public void Validate_EmptySku_ReturnsSkuRequiredError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "Sku" &&
            e.ErrorMessage.Contains("required", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Validate_WhitespaceSku_ReturnsSkuRequiredError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "   ",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Sku" &&
            e.ErrorMessage.Contains("required", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Validate_SkuExceeds100Characters_ReturnsSkuLengthError()
    {
        // Arrange
        var tooLongSku = new string('A', 101);
        var command = new CreateProductCommand(
            Sku: tooLongSku,
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Sku" &&
            e.ErrorMessage.Contains("100", StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Name Validation

    [TestMethod]
    public void Validate_EmptyName_ReturnsNameRequiredError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "",
            Price: 10m,
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "Name" &&
            e.ErrorMessage.Contains("required", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Validate_WhitespaceName_ReturnsNameRequiredError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "   ",
            Price: 10m,
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Name" &&
            e.ErrorMessage.Contains("required", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Validate_NameExceeds200Characters_ReturnsNameLengthError()
    {
        // Arrange
        var tooLongName = new string('A', 201);
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: tooLongName,
            Price: 10m,
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Name" &&
            e.ErrorMessage.Contains("200", StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Price Validation

    [TestMethod]
    public void Validate_NegativePrice_ReturnsPriceGreaterThanOrEqualToZeroError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: -0.01m,
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Price" &&
            e.ErrorMessage.Contains("greater than or equal to", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Validate_PriceWithMoreThan2DecimalPlaces_ReturnsPrecisionError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10.999m, // 3 decimal places
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Price" &&
            e.ErrorMessage.Contains("decimal", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Validate_PriceWith2DecimalPlaces_ReturnsNoErrors()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10.99m, // Valid: exactly 2 decimal places
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void Validate_PriceWith1DecimalPlace_ReturnsNoErrors()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10.5m, // Valid: 1 decimal place
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void Validate_PriceWholeNumber_ReturnsNoErrors()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 100m, // Valid: whole number
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Currency Validation

    [TestMethod]
    public void Validate_EmptyCurrency_ReturnsCurrencyRequiredError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid(),
            Currency: "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "Currency" &&
            e.ErrorMessage.Contains("required", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Validate_CurrencyLessThan3Characters_ReturnsCurrencyError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid(),
            Currency: "US");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Currency",
            "Should have currency validation error");
    }

    [TestMethod]
    public void Validate_CurrencyMoreThan3Characters_ReturnsCurrencyError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid(),
            Currency: "USDA");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Currency",
            "Should have currency validation error");
    }

    [TestMethod]
    public void Validate_ValidCurrency_ReturnsNoErrors()
    {
        // Arrange
        var validCurrencies = new[] { "USD", "EUR", "GBP", "JPY", "CAD" };

        foreach (var currency in validCurrencies)
        {
            var command = new CreateProductCommand(
                Sku: "SKU",
                Name: "Product",
                Price: 10m,
                CategoryId: Guid.NewGuid(),
                Currency: currency);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue($"Currency {currency} should be valid");
        }
    }

    [TestMethod]
    public void Validate_LowercaseCurrency_ReturnsCurrencyFormatError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid(),
            Currency: "usd"); // lowercase should fail ISO format check

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Currency" &&
            e.ErrorMessage.Contains("uppercase", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Validate_MixedCaseCurrency_ReturnsCurrencyFormatError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid(),
            Currency: "Usd");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Currency" &&
            e.ErrorMessage.Contains("uppercase", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Validate_CurrencyWithSpecialCharacters_ReturnsCurrencyFormatError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid(),
            Currency: "US$");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Currency" &&
            e.ErrorMessage.Contains("ISO", StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region CategoryId Validation

    [TestMethod]
    public void Validate_EmptyCategoryId_ReturnsCategoryIdRequiredError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "CategoryId" &&
            e.ErrorMessage.Contains("required", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Validate_ValidCategoryId_ReturnsNoErrors()
    {
        // Arrange
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Description Validation

    [TestMethod]
    public void Validate_DescriptionExceeds2000Characters_ReturnsDescriptionLengthError()
    {
        // Arrange
        var tooLongDescription = new string('A', 2001);
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid(),
            Description: tooLongDescription);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Description" &&
            e.ErrorMessage.Contains("2000", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Validate_MaxLengthDescription_ReturnsNoErrors()
    {
        // Arrange
        var maxLengthDescription = new string('A', 2000);
        var command = new CreateProductCommand(
            Sku: "SKU",
            Name: "Product",
            Price: 10m,
            CategoryId: Guid.NewGuid(),
            Description: maxLengthDescription);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Multiple Validation Errors

    [TestMethod]
    public void Validate_MultipleErrorsInCommand_ReturnsAllErrors()
    {
        // Arrange - empty SKU, empty name, negative price, empty category
        var command = new CreateProductCommand(
            Sku: "",
            Name: "",
            Price: -10m,
            CategoryId: Guid.Empty,
            Currency: "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Select(e => e.PropertyName).Should()
            .Contain("Sku", "Name", "Price", "CategoryId", "Currency");
    }

    #endregion
}
