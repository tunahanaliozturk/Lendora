namespace Lendora.Domain.Enums;

/// <summary>
/// Represents the payment status of a single repayment installment.
/// </summary>
public enum InstallmentStatus
{
    Scheduled,
    Paid,
    PartiallyPaid,
    Late
}
