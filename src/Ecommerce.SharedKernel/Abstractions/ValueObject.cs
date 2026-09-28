namespace Ecommerce.SharedKernel.Abstractions;

/// <summary>
/// Base class for value objects.
/// Value objects are immutable objects with no identity that are compared by value.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the components that define the equality of this value object.
    /// Subclasses must override this to return their significant properties.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>
    /// Determines whether two value objects are equal based on their components.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject valueObject)
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(
            valueObject.GetEqualityComponents());
    }

    /// <summary>
    /// Gets the hash code based on the value object components.
    /// </summary>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(
                1,
                (current, obj) =>
                {
                    unchecked
                    {
                        return current * 23 + (obj?.GetHashCode() ?? 0);
                    }
                });
    }

    /// <summary>
    /// Determines whether this value object is equal to another.
    /// </summary>
    public bool Equals(ValueObject? other)
    {
        return Equals((object?)other);
    }

    /// <summary>
    /// Determines whether two value object instances are equal.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two value object instances are not equal.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
