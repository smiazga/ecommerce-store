namespace Ecommerce.SharedKernel.Exceptions;

/// <summary>
/// Base exception for domain-related errors.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Initializes a new domain exception with the specified message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public DomainException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new domain exception with the specified message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
