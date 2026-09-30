namespace Ecommerce.Application.Catalog.Commands.UpdateProduct;

using FluentValidation;

/// <summary>
/// Validator for <see cref="UpdateProductCommand"/>.
/// Enforces request-level constraints before orchestration.
/// </summary>
public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    /// <summary>
    /// Defines validation rules for UpdateProductCommand.
    /// </summary>
    public UpdateProductCommandValidator()
    {
        RuleFor(cmd => cmd.ProductId)
            .NotEqual(Guid.Empty)
            .WithMessage("Product ID is required.");

        RuleFor(cmd => cmd.Name)
            .MaximumLength(200)
            .WithMessage("Product name must not exceed 200 characters.")
            .When(cmd => !string.IsNullOrWhiteSpace(cmd.Name));

        RuleFor(cmd => cmd.Description)
            .MaximumLength(2000)
            .WithMessage("Description must not exceed 2000 characters.")
            .When(cmd => !string.IsNullOrWhiteSpace(cmd.Description));

        RuleFor(cmd => cmd.Price)
            .GreaterThanOrEqualTo(0m)
            .WithMessage("Price must be greater than or equal to zero.")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Price must have at most 2 decimal places.")
            .When(cmd => cmd.Price.HasValue);

        RuleFor(cmd => cmd.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Length(3)
            .WithMessage("Currency must be a valid ISO 4217 three-letter code.")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be uppercase ISO code (e.g., USD, EUR).")
            .When(cmd => !string.IsNullOrWhiteSpace(cmd.Currency));

        RuleFor(cmd => cmd.CategoryId)
            .NotEqual(Guid.Empty)
            .WithMessage("Category ID must be a valid identifier.")
            .When(cmd => cmd.CategoryId.HasValue);

        RuleFor(cmd => cmd)
            .Must(cmd => HasAtLeastOneUpdate(cmd))
            .WithMessage("At least one field must be provided for update.");
    }

    /// <summary>
    /// Ensures that at least one updatable field is provided.
    /// </summary>
    private static bool HasAtLeastOneUpdate(UpdateProductCommand cmd)
    {
        return !string.IsNullOrWhiteSpace(cmd.Name) ||
               !string.IsNullOrWhiteSpace(cmd.Description) ||
               cmd.Price.HasValue ||
               cmd.CategoryId.HasValue ||
               cmd.IsActive.HasValue;
    }
}
