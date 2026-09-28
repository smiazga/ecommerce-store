namespace Ecommerce.SharedKernel.Guards;

/// <summary>
/// Guard helper for argument validation.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Ensures the specified string is not null or empty.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <exception cref="ArgumentException">Thrown when value is null or empty.</exception>
    public static void NullOrEmpty(string? value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
    }

    /// <summary>
    /// Ensures the specified value is not null.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static void Null<T>(T? value, string parameterName) where T : class
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
    }

    /// <summary>
    /// Ensures the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition to validate.</param>
    /// <param name="message">The error message.</param>
    /// <exception cref="ArgumentException">Thrown when condition is false.</exception>
    public static void Against(bool condition, string message)
    {
        if (condition)
        {
            throw new ArgumentException(message);
        }
    }

    /// <summary>
    /// Ensures the specified value is not negative.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is negative.</exception>
    public static void NegativeOrZero(decimal value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                $"{parameterName} must be greater than zero.");
        }
    }

    /// <summary>
    /// Ensures the specified value is not negative.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is negative.</exception>
    public static void NegativeOrZero(int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                $"{parameterName} must be greater than zero.");
        }
    }

    /// <summary>
    /// Ensures the specified value is within the specified range.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is out of range.</exception>
    public static void OutOfRange(decimal value, decimal min, decimal max, string parameterName)
    {
        if (value < min || value > max)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                $"{parameterName} must be between {min} and {max}.");
        }
    }
}
