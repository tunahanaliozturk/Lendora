using Lendora.Domain.Common;

namespace Lendora.Domain.Events;

/// <summary>
/// Raised when a payment is successfully registered against a loan.
/// </summary>
public sealed record PaymentRegisteredEvent(
    Guid LoanId,
    Guid PaymentId,
    decimal Amount
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
