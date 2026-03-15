using Lendora.Domain.Common;
using Lendora.Domain.Enums;
using Lendora.Domain.Exceptions;

namespace Lendora.Domain.Entities;

/// <summary>
/// Represents a single scheduled repayment installment within a loan's amortization schedule.
/// </summary>
public class RepaymentInstallment : BaseEntity
{
    /// <summary>
    /// The loan this installment belongs to.
    /// </summary>
    public Guid LoanId { get; private set; }

    /// <summary>
    /// Sequential number of this installment within the loan schedule (1-based).
    /// </summary>
    public int InstallmentNumber { get; private set; }

    /// <summary>
    /// The date by which this installment is due.
    /// </summary>
    public DateTime DueDate { get; private set; }

    /// <summary>
    /// The principal portion of this installment.
    /// </summary>
    public decimal PrincipalAmount { get; private set; }

    /// <summary>
    /// The interest portion of this installment.
    /// </summary>
    public decimal InterestAmount { get; private set; }

    /// <summary>
    /// Total amount due for this installment (principal + interest).
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Amount paid so far toward this installment.
    /// </summary>
    public decimal PaidAmount { get; private set; }

    /// <summary>
    /// Current payment status of this installment.
    /// </summary>
    public InstallmentStatus Status { get; private set; }

    private RepaymentInstallment() { } // EF Core

    public RepaymentInstallment(
        Guid loanId,
        int installmentNumber,
        DateTime dueDate,
        decimal principalAmount,
        decimal interestAmount)
    {
        if (installmentNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(installmentNumber), "Installment number must be at least 1.");
        if (principalAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(principalAmount), "Principal amount cannot be negative.");
        if (interestAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(interestAmount), "Interest amount cannot be negative.");

        LoanId = loanId;
        InstallmentNumber = installmentNumber;
        DueDate = dueDate;
        PrincipalAmount = principalAmount;
        InterestAmount = interestAmount;
        TotalAmount = principalAmount + interestAmount;
        PaidAmount = 0m;
        Status = InstallmentStatus.Scheduled;
    }

    /// <summary>
    /// Applies a payment amount to this installment. Returns the amount actually applied
    /// (which may be less than the input if the installment is overpaid).
    /// </summary>
    /// <param name="amount">The amount to apply. Must be positive.</param>
    /// <returns>The portion of the amount that was applied to this installment.</returns>
    public decimal ApplyPayment(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be positive.");

        if (Status == InstallmentStatus.Paid)
            return 0m;

        var remaining = TotalAmount - PaidAmount;
        var applied = Math.Min(amount, remaining);

        PaidAmount += applied;
        UpdatedAt = DateTime.UtcNow;

        Status = PaidAmount >= TotalAmount
            ? InstallmentStatus.Paid
            : InstallmentStatus.PartiallyPaid;

        return applied;
    }

    /// <summary>
    /// Marks this installment as late if it has not been fully paid.
    /// </summary>
    public void MarkAsLate()
    {
        if (Status is InstallmentStatus.Scheduled or InstallmentStatus.PartiallyPaid)
        {
            Status = InstallmentStatus.Late;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
