namespace Ecommerce.Application.Catalog.Commands.DeleteProduct;

using FluentValidation;

/// <summary>
/// Validates DeleteProductCommand.
/// </summary>
public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product id is required.");
    }
}
