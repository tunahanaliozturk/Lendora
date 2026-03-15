namespace Lendora.Api.Contracts.Requests;

/// <summary>
/// Request body for rejecting a loan application.
/// </summary>
public sealed record RejectLoanApplicationRequest(
    string RejectedBy,
    string Reason);
