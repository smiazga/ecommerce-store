namespace Ecommerce.SharedKernel.Results;

/// <summary>
/// Represents an error with a code and message.
/// </summary>
public sealed record Error(string Code, string Message);

/// <summary>
/// Represents the result of an operation that may succeed or fail.
/// </summary>
public sealed record Result(bool IsSuccess, Error? Error = null)
{
    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error, nameof(error));
        return new(false, error);
    }

    /// <summary>
    /// Creates a failed result with the specified error code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public static Result Failure(string code, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code, nameof(code));
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));
        return Failure(new Error(code, message));
    }
}

/// <summary>
/// Represents the result of an operation that returns a value on success or a failure.
/// </summary>
/// <typeparam name="T">The type of value returned on success.</typeparam>
public sealed record Result<T>(bool IsSuccess, T? Value = default, Error? Error = null)
{
    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The successful result value.</param>
    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        return new(true, value);
    }

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    public static Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error, nameof(error));
        return new(false, default, error);
    }

    /// <summary>
    /// Creates a failed result with the specified error code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public static Result<T> Failure(string code, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code, nameof(code));
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));
        return Failure(new Error(code, message));
    }
}

/// <summary>
/// Extension methods for the Result pattern.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Projects a successful result's value to a new value.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="onSuccess">A selector function to apply to the value on success.</param>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));
        ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));

        if (!result.IsSuccess)
        {
            return Result<TOut>.Failure(result.Error!);
        }

        try
        {
            var mappedValue = onSuccess(result.Value!);
            return Result<TOut>.Success(mappedValue);
        }
        catch (Exception ex)
        {
            return Result<TOut>.Failure(
                "MAPPING_ERROR",
                ex.Message);
        }
    }

    /// <summary>
    /// Chains multiple operations that may fail.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="onSuccess">A function that returns a new result on success.</param>
    public static Result<T> Bind<T>(
        this Result<T> result,
        Func<T, Result<T>> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));
        ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));

        if (!result.IsSuccess)
        {
            return result;
        }

        try
        {
            return onSuccess(result.Value!);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(
                "BIND_ERROR",
                ex.Message);
        }
    }

    /// <summary>
    /// Executes a side effect on success.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="onSuccess">An action to execute on success.</param>
    public static Result<T> Tap<T>(
        this Result<T> result,
        Action<T> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));
        ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));

        if (result.IsSuccess)
        {
            onSuccess(result.Value!);
        }

        return result;
    }

    /// <summary>
    /// Executes a side effect on failure.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="onFailure">An action to execute on failure.</param>
    public static Result<T> TapError<T>(
        this Result<T> result,
        Action<Error> onFailure)
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));
        ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));

        if (!result.IsSuccess)
        {
            onFailure(result.Error!);
        }

        return result;
    }

    /// <summary>
    /// Unwraps a successful result or throws an exception.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <returns>The value on success.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public static T Unwrap<T>(this Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Cannot unwrap failed result: {result.Error?.Code} - {result.Error?.Message}");
        }

        return result.Value!;
    }

    /// <summary>
    /// Gets the value if successful, otherwise returns a default value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="defaultValue">The default value to return on failure.</param>
    public static T GetValueOrDefault<T>(
        this Result<T> result,
        T defaultValue)
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));
        ArgumentNullException.ThrowIfNull(defaultValue, nameof(defaultValue));

        return result.IsSuccess ? result.Value! : defaultValue;
    }
}
