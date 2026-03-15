namespace Lendora.Api.Contracts.Requests;

/// <summary>
/// Request body for submitting a new loan application.
/// </summary>
public sealed record SubmitLoanApplicationRequest(
    Guid CustomerId,
    decimal RequestedAmount,
    int RequestedTermMonths,
    decimal AnnualIncome,
    decimal ExistingMonthlyDebt);
