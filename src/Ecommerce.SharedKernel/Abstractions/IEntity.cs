namespace Ecommerce.SharedKernel.Abstractions;

/// <summary>
/// Contract for domain entities exposing identity.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    Guid Id { get; }
}
