using Lendora.Domain.Common;

namespace Lendora.Domain.Events;

/// <summary>
/// Raised when a repayment installment is marked as late.
/// </summary>
public sealed record InstallmentMarkedLateEvent(
    Guid InstallmentId,
    Guid LoanId
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
