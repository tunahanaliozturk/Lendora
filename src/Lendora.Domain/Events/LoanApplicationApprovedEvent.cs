using Lendora.Domain.Common;

namespace Lendora.Domain.Events;

/// <summary>
/// Raised when a loan application is approved after risk evaluation.
/// </summary>
public sealed record LoanApplicationApprovedEvent(
    Guid ApplicationId,
    decimal ApprovedAmount,
    decimal InterestRate
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
