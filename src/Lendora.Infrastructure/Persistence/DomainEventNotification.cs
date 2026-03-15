using Lendora.Domain.Common;
using MediatR;

namespace Lendora.Infrastructure.Persistence;

/// <summary>
/// Wraps a domain event as a MediatR notification, bridging the gap between
/// the domain's <see cref="IDomainEvent"/> and MediatR's <see cref="INotification"/>.
/// This allows domain event handlers to be registered as MediatR notification handlers
/// without coupling the domain layer to MediatR.
/// </summary>
/// <typeparam name="TDomainEvent">The domain event type being wrapped.</typeparam>
public sealed class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : IDomainEvent
{
    /// <summary>
    /// The underlying domain event.
    /// </summary>
    public TDomainEvent DomainEvent { get; }

    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent ?? throw new ArgumentNullException(nameof(domainEvent));
    }
}
