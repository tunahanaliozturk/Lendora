using Lendora.Domain.Common;

namespace Lendora.Domain.Events;

/// <summary>
/// Raised when a loan application is rejected after risk evaluation.
/// </summary>
public sealed record LoanApplicationRejectedEvent(
    Guid ApplicationId,
    string Reason
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
