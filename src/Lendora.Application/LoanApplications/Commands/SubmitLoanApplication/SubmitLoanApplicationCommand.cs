using MediatR;

namespace Lendora.Application.LoanApplications.Commands.SubmitLoanApplication;

/// <summary>
/// Command to submit a new loan application for a customer.
/// </summary>
public sealed record SubmitLoanApplicationCommand(
    Guid CustomerId,
    decimal RequestedAmount,
    int RequestedTermMonths,
    decimal AnnualIncome,
    decimal ExistingMonthlyDebt) : IRequest<Guid>;
