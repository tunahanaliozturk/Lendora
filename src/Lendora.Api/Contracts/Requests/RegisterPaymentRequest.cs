namespace Lendora.Api.Contracts.Requests;

/// <summary>
/// Request body for registering a payment against a loan.
/// </summary>
public sealed record RegisterPaymentRequest(
    Guid LoanId,
    decimal Amount,
    DateTime PaidAt,
    string? PaymentReference);
