namespace Lendora.Domain.Enums;

/// <summary>
/// Represents the lifecycle states of a loan application.
/// </summary>
public enum LoanApplicationStatus
{
    Submitted,
    RiskEvaluating,
    Approved,
    Rejected
}
