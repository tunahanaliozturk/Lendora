using MediatR;

namespace Lendora.Application.Loans.Commands.MarkInstallmentAsLate;

/// <summary>
/// Command to mark a specific installment as late and potentially mark the loan as delinquent.
/// </summary>
public sealed record MarkInstallmentAsLateCommand(
    Guid LoanId,
    Guid InstallmentId) : IRequest;
