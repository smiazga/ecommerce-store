namespace Ecommerce.SharedKernel.Abstractions;

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the date and time when this event occurred.
    /// </summary>
    DateTime OccurredAt { get; }
}
