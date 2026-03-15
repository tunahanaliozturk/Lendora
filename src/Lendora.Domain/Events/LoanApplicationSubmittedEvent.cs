using Lendora.Domain.Common;

namespace Lendora.Domain.Events;

/// <summary>
/// Raised when a new loan application is submitted by a customer.
/// </summary>
public sealed record LoanApplicationSubmittedEvent(
    Guid ApplicationId,
    Guid CustomerId
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
