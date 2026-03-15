namespace Lendora.Api.Contracts.Requests;

/// <summary>
/// Request body for approving a loan application.
/// </summary>
public sealed record ApproveLoanApplicationRequest(
    string ApprovedBy,
    decimal InterestRate,
    decimal ApprovedAmount,
    int ApprovedTermMonths);
