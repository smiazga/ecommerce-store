namespace Ecommerce.SharedKernel.Exceptions;

/// <summary>
/// Exception for domain validation failures.
/// </summary>
public sealed class DomainValidationException : DomainException
{
    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public IReadOnlyCollection<string> Errors { get; }

    /// <summary>
    /// Initializes a new domain validation exception with the specified errors.
    /// </summary>
    /// <param name="errors">The validation error messages.</param>
    public DomainValidationException(IEnumerable<string> errors)
        : base(FormatErrors(errors))
    {
        ArgumentNullException.ThrowIfNull(errors, nameof(errors));
        Errors = errors.ToList().AsReadOnly();
    }

    /// <summary>
    /// Initializes a new domain validation exception with a single error.
    /// </summary>
    /// <param name="error">The validation error message.</param>
    public DomainValidationException(string error)
        : base(error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error, nameof(error));
        Errors = [error];
    }

    private static string FormatErrors(IEnumerable<string> errors)
    {
        var errorList = errors.ToList();
        return $"Validation failed: {string.Join("; ", errorList)}";
    }
}
