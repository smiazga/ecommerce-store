namespace Ecommerce.Domain.ValueObjects;

using Ecommerce.SharedKernel.Abstractions;

/// <summary>
/// Represents a product SKU (stock keeping unit).
/// </summary>
public sealed class Sku : ValueObject
{
    /// <summary>
    /// The SKU value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="Sku"/>.
    /// </summary>
    /// <param name="value">The SKU value. Required.</param>
    public Sku(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("SKU is required.", nameof(value));
        }

        Value = value.Trim();
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}
