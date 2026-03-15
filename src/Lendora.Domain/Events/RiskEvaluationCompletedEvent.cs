using Lendora.Domain.Common;

namespace Lendora.Domain.Events;

/// <summary>
/// Raised when risk evaluation completes for a loan application.
/// </summary>
public sealed record RiskEvaluationCompletedEvent(
    Guid ApplicationId,
    int Score,
    string Decision
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
