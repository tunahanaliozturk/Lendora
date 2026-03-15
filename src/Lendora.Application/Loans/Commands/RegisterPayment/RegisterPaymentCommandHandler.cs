using Lendora.Application.Common.Interfaces;
using Lendora.Domain.Enums;
using MediatR;

namespace Lendora.Application.Loans.Commands.RegisterPayment;

/// <summary>
/// Handles <see cref="RegisterPaymentCommand"/> by applying the payment to the loan's
/// outstanding installments and invalidating related cache entries.
/// </summary>
public sealed class RegisterPaymentCommandHandler(
    ILoanRepository loanRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<RegisterPaymentCommand, RegisterPaymentResult>
{
    public async Task<RegisterPaymentResult> Handle(
        RegisterPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var loan = await loanRepository.GetWithInstallmentsAndPaymentsAsync(request.LoanId, cancellationToken)
            ?? throw new KeyNotFoundException($"Loan with ID '{request.LoanId}' was not found.");

        loan.RegisterPayment(request.Amount, request.PaidAt, request.PaymentReference);

        // Invalidate cached loan data since payment state has changed
        await cacheService.RemoveAsync($"loan:{loan.Id}:status", cancellationToken);
        await cacheService.RemoveAsync($"loan:{loan.Id}:schedule", cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var remainingBalance = loan.Installments
            .Where(i => i.Status != InstallmentStatus.Paid)
            .Sum(i => i.TotalAmount - i.PaidAmount);

        var latestPayment = loan.Payments
            .OrderByDescending(p => p.CreatedAt)
            .First();

        return new RegisterPaymentResult(latestPayment.Id, remainingBalance);
    }
}
