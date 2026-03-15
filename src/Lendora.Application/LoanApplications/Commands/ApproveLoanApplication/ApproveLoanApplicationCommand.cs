using MediatR;

namespace Lendora.Application.LoanApplications.Commands.ApproveLoanApplication;

/// <summary>
/// Command to approve a loan application and create the corresponding loan with
/// a fully amortized repayment schedule.
/// </summary>
public sealed record ApproveLoanApplicationCommand(
    Guid ApplicationId,
    string ApprovedBy,
    decimal InterestRate,
    decimal ApprovedAmount,
    int ApprovedTermMonths) : IRequest<ApproveLoanApplicationResult>;

/// <summary>
/// Result of approving a loan application, containing both the application and new loan identifiers.
/// </summary>
public sealed record ApproveLoanApplicationResult(Guid ApplicationId, Guid LoanId);
