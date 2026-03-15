using Lendora.Application.Common.Interfaces;
using MediatR;

namespace Lendora.Application.LoanApplications.Commands.RejectLoanApplication;

/// <summary>
/// Handles <see cref="RejectLoanApplicationCommand"/> by transitioning the application
/// to a rejected state with the provided reason.
/// </summary>
public sealed class RejectLoanApplicationCommandHandler(
    ILoanApplicationRepository loanApplicationRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RejectLoanApplicationCommand>
{
    public async Task Handle(
        RejectLoanApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var application = await loanApplicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Loan application with ID '{request.ApplicationId}' was not found.");

        application.Reject(request.Reason);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
