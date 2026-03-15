using System.Collections.ObjectModel;

namespace Lendora.Domain.Common;

/// <summary>
/// Base class for aggregate roots. Extends <see cref="BaseEntity"/> with domain event support.
/// Domain events are collected during the aggregate's lifecycle and dispatched after persistence.
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Domain events raised by this aggregate, awaiting dispatch.
    /// </summary>
    public ReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to be dispatched after the aggregate is persisted.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all pending domain events. Called by the infrastructure after successful dispatch.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
