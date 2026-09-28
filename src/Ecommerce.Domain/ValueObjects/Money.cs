namespace Ecommerce.Domain.ValueObjects;

using System.Globalization;
using Ecommerce.SharedKernel.Abstractions;

/// <summary>
/// Represents a monetary amount and currency.
/// </summary>
public sealed class Money : ValueObject
{
    /// <summary>
    /// The monetary amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// The ISO currency code (e.g. "USD").
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="Money"/>.
    /// </summary>
    /// <param name="amount">Monetary amount. Must be >= 0.</param>
    /// <param name="currency">Three-letter ISO currency code.</param>
    public Money(decimal amount, string currency)
    {
        if (amount < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than or equal to zero.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        // normalize currency to upper invariant
        Currency = currency.ToUpperInvariant();
        Amount = decimal.Round(amount, 2, MidpointRounding.ToEven);
    }

    /// <summary>
    /// Returns a new <see cref="Money"/> with the amount replaced.
    /// </summary>
    /// <param name="amount">The new amount.</param>
    /// <returns>New money instance.</returns>
    public Money WithAmount(decimal amount) => new Money(amount, Currency);

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    /// <inheritdoc />
    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0} {1:0.00}", Currency, Amount);
}
