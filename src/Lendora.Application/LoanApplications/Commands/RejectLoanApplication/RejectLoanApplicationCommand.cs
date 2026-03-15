using MediatR;

namespace Lendora.Application.LoanApplications.Commands.RejectLoanApplication;

/// <summary>
/// Command to reject a loan application with a specified reason.
/// </summary>
public sealed record RejectLoanApplicationCommand(
    Guid ApplicationId,
    string RejectedBy,
    string Reason) : IRequest;
