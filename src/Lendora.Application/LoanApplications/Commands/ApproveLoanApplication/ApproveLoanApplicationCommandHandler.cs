using Lendora.Application.Common.Interfaces;
using Lendora.Domain.Entities;
using MediatR;

namespace Lendora.Application.LoanApplications.Commands.ApproveLoanApplication;

/// <summary>
/// Handles <see cref="ApproveLoanApplicationCommand"/> by approving the application,
/// creating a loan, and generating a fully amortized repayment schedule.
/// </summary>
public sealed class ApproveLoanApplicationCommandHandler(
    ILoanApplicationRepository loanApplicationRepository,
    ILoanRepository loanRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<ApproveLoanApplicationCommand, ApproveLoanApplicationResult>
{
    public async Task<ApproveLoanApplicationResult> Handle(
        ApproveLoanApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var application = await loanApplicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Loan application with ID '{request.ApplicationId}' was not found.");

        application.Approve(
            request.ApprovedBy,
            request.InterestRate,
            request.ApprovedAmount,
            request.ApprovedTermMonths);

        var startDate = dateTimeProvider.UtcNow;

        var loan = Loan.Create(
            application.Id,
            application.CustomerId,
            request.ApprovedAmount,
            request.InterestRate,
            request.ApprovedTermMonths,
            startDate);

        GenerateAmortizationSchedule(loan, request.ApprovedAmount, request.InterestRate, request.ApprovedTermMonths, startDate);

        await loanRepository.AddAsync(loan, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApproveLoanApplicationResult(application.Id, loan.Id);
    }

    /// <summary>
    /// Generates a fully amortized repayment schedule using the standard amortization formula.
    /// Each installment has a fixed total payment with decreasing interest and increasing principal
    /// portions over the life of the loan.
    /// </summary>
    private static void GenerateAmortizationSchedule(
        Loan loan,
        decimal principal,
        decimal annualInterestRate,
        int termMonths,
        DateTime startDate)
    {
        if (annualInterestRate == 0)
        {
            var monthlyPrincipal = Math.Round(principal / termMonths, 2);

            for (var i = 1; i <= termMonths; i++)
            {
                // Adjust the last installment to absorb any rounding remainder
                var principalAmount = i == termMonths
                    ? principal - (monthlyPrincipal * (termMonths - 1))
                    : monthlyPrincipal;

                var installment = new RepaymentInstallment(
                    loan.Id,
                    i,
                    startDate.AddMonths(i),
                    principalAmount,
                    0m);

                loan.AddInstallment(installment);
            }

            return;
        }

        var monthlyRate = annualInterestRate / 12m;
        var power = (decimal)Math.Pow((double)(1m + monthlyRate), termMonths);
        var monthlyPayment = Math.Round(principal * (monthlyRate * power) / (power - 1m), 2);

        var remainingBalance = principal;

        for (var i = 1; i <= termMonths; i++)
        {
            var interestAmount = Math.Round(remainingBalance * monthlyRate, 2);
            var principalPortion = monthlyPayment - interestAmount;

            // Adjust the last installment to clear any rounding remainder
            if (i == termMonths)
            {
                principalPortion = remainingBalance;
                interestAmount = monthlyPayment - principalPortion;

                // If rounding causes negative interest on the last payment, zero it out
                if (interestAmount < 0)
                    interestAmount = 0m;
            }

            var installment = new RepaymentInstallment(
                loan.Id,
                i,
                startDate.AddMonths(i),
                principalPortion,
                interestAmount);

            loan.AddInstallment(installment);

            remainingBalance -= principalPortion;
        }
    }
}
