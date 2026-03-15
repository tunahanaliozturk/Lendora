using Lendora.Application.Common.Interfaces;
using Lendora.Domain.Enums;
using MediatR;

namespace Lendora.Application.Loans.Commands.MarkInstallmentAsLate;

/// <summary>
/// Handles <see cref="MarkInstallmentAsLateCommand"/> by marking the specified installment
/// as late and transitioning the loan to delinquent status if any installments are late.
/// </summary>
public sealed class MarkInstallmentAsLateCommandHandler(
    ILoanRepository loanRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkInstallmentAsLateCommand>
{
    public async Task Handle(
        MarkInstallmentAsLateCommand request,
        CancellationToken cancellationToken)
    {
        var loan = await loanRepository.GetWithInstallmentsAsync(request.LoanId, cancellationToken)
            ?? throw new KeyNotFoundException($"Loan with ID '{request.LoanId}' was not found.");

        var installment = loan.Installments.FirstOrDefault(i => i.Id == request.InstallmentId)
            ?? throw new KeyNotFoundException($"Installment with ID '{request.InstallmentId}' was not found on loan '{request.LoanId}'.");

        installment.MarkAsLate();

        var hasLateInstallments = loan.Installments.Any(i => i.Status == InstallmentStatus.Late);

        if (hasLateInstallments && loan.Status == LoanStatus.Active)
        {
            loan.MarkAsDelinquent();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
