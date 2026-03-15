using Lendora.Domain.Common;

namespace Lendora.Domain.Entities;

/// <summary>
/// Represents a payment made against a loan.
/// </summary>
public class Payment : BaseEntity
{
    /// <summary>
    /// The loan this payment is applied to.
    /// </summary>
    public Guid LoanId { get; private set; }

    /// <summary>
    /// The payment amount.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// UTC timestamp indicating when the payment was made.
    /// </summary>
    public DateTime PaidAt { get; private set; }

    /// <summary>
    /// Optional external reference identifier for the payment (e.g., transaction ID).
    /// </summary>
    public string? PaymentReference { get; private set; }

    private Payment() { } // EF Core

    public Payment(Guid loanId, decimal amount, DateTime paidAt, string? paymentReference = null)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be positive.");

        LoanId = loanId;
        Amount = amount;
        PaidAt = paidAt;
        PaymentReference = paymentReference;
    }
}
