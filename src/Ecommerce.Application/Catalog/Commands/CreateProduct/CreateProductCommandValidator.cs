namespace Ecommerce.Application.Catalog.Commands.CreateProduct;

using FluentValidation;

/// <summary>
/// Validator for <see cref="CreateProductCommand"/>.
/// Enforces request-level constraints before orchestration.
/// </summary>
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    /// <summary>
    /// Defines validation rules for CreateProductCommand.
    /// </summary>
    public CreateProductCommandValidator()
    {
        RuleFor(cmd => cmd.Sku)
            .NotEmpty()
            .WithMessage("SKU is required.")
            .MaximumLength(100)
            .WithMessage("SKU must not exceed 100 characters.");

        RuleFor(cmd => cmd.Name)
            .NotEmpty()
            .WithMessage("Product name is required.")
            .MaximumLength(200)
            .WithMessage("Product name must not exceed 200 characters.");

        RuleFor(cmd => cmd.Price)
            .GreaterThanOrEqualTo(0m)
            .WithMessage("Price must be greater than or equal to zero.")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Price must have at most 2 decimal places.");

        RuleFor(cmd => cmd.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Length(3)
            .WithMessage("Currency must be a valid ISO 4217 three-letter code.")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be uppercase ISO code (e.g., USD, EUR).");

        RuleFor(cmd => cmd.CategoryId)
            .NotEqual(Guid.Empty)
            .WithMessage("Category ID is required.");

        RuleFor(cmd => cmd.Description)
            .MaximumLength(2000)
            .WithMessage("Description must not exceed 2000 characters.")
            .When(cmd => !string.IsNullOrWhiteSpace(cmd.Description));
    }
}
