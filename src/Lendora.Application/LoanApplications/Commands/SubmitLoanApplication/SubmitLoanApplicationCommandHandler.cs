using Lendora.Application.Common.Interfaces;
using Lendora.Domain.Entities;
using MediatR;

namespace Lendora.Application.LoanApplications.Commands.SubmitLoanApplication;

/// <summary>
/// Handles <see cref="SubmitLoanApplicationCommand"/> by creating a new loan application
/// via the domain factory method and persisting it.
/// </summary>
public sealed class SubmitLoanApplicationCommandHandler(
    ILoanApplicationRepository loanApplicationRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SubmitLoanApplicationCommand, Guid>
{
    public async Task<Guid> Handle(
        SubmitLoanApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var application = LoanApplication.Create(
            request.CustomerId,
            request.RequestedAmount,
            request.RequestedTermMonths,
            request.AnnualIncome,
            request.ExistingMonthlyDebt);

        await loanApplicationRepository.AddAsync(application, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return application.Id;
    }
}
