using MediatR;

namespace Lendora.Application.Loans.Commands.RegisterPayment;

/// <summary>
/// Command to register a payment against an active loan.
/// </summary>
public sealed record RegisterPaymentCommand(
    Guid LoanId,
    decimal Amount,
    DateTime PaidAt,
    string? PaymentReference) : IRequest<RegisterPaymentResult>;

/// <summary>
/// Result of a payment registration, containing the payment identifier and the remaining balance.
/// </summary>
public sealed record RegisterPaymentResult(Guid PaymentId, decimal RemainingBalance);
