namespace Ecommerce.SharedKernel.Abstractions;

/// <summary>
/// Base class for aggregate roots.
/// Aggregates are clusters of entities and value objects treated as a single unit.
/// Aggregate roots are the entry point for modifying the aggregate.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the collection of domain events raised by this aggregate.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Initializes a new aggregate root with a generated identifier.
    /// </summary>
    protected AggregateRoot() : base()
    {
    }

    /// <summary>
    /// Initializes a new aggregate root with a specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for this aggregate root.</param>
    protected AggregateRoot(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Raises a domain event on this aggregate.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent, nameof(domainEvent));
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events on this aggregate.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
