namespace Ecommerce.SharedKernel.Abstractions;

/// <summary>
/// Base class for domain events.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>
    /// Gets the date and time when this event occurred.
    /// </summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Initializes a new domain event with current timestamp.
    /// </summary>
    protected DomainEvent()
    {
        OccurredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new domain event with a specified timestamp.
    /// </summary>
    /// <param name="occurredAt">The date and time when this event occurred.</param>
    protected DomainEvent(DateTime occurredAt)
    {
        OccurredAt = occurredAt;
    }
}
