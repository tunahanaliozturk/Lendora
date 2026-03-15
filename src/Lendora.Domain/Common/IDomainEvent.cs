namespace Lendora.Domain.Common;

/// <summary>
/// Marker interface for domain events raised by aggregate roots.
/// Events are dispatched via the infrastructure layer after persistence.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// UTC timestamp indicating when the event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}
