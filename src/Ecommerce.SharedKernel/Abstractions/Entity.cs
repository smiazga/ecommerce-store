namespace Ecommerce.SharedKernel.Abstractions;

/// <summary>
/// Base class for domain entities with identity.
/// </summary>
public abstract class Entity : IEntity
{
    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    public Guid Id { get; protected init; }

    /// <summary>
    /// Initializes a new entity with a generated identifier.
    /// </summary>
    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Initializes a new entity with a specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for this entity.</param>
    protected Entity(Guid id)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, Guid.Empty, nameof(id));
        Id = id;
    }

    /// <summary>
    /// Determines whether two entities are equal based on their identifiers.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity entity)
        {
            return false;
        }

        return entity.Id == Id;
    }

    /// <summary>
    /// Gets the hash code based on the entity identifier.
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Determines whether two entity instances are equal.
    /// </summary>
    public static bool operator ==(Entity? left, Entity? right)
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
    /// Determines whether two entity instances are not equal.
    /// </summary>
    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}
