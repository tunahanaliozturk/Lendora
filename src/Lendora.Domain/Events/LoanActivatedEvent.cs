using Lendora.Domain.Common;

namespace Lendora.Domain.Events;

/// <summary>
/// Raised when a loan is created and activated from an approved application.
/// </summary>
public sealed record LoanActivatedEvent(
    Guid LoanId,
    Guid ApplicationId
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
