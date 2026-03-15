using MediatR;

namespace Lendora.Application.Loans.Queries.GetRepaymentSchedule;

/// <summary>
/// Query to retrieve the full repayment schedule for a loan.
/// </summary>
public sealed record GetRepaymentScheduleQuery(Guid LoanId) : IRequest<RepaymentScheduleDto>;

/// <summary>
/// Data transfer object representing a loan's complete repayment schedule.
/// </summary>
public sealed record RepaymentScheduleDto(
    Guid LoanId,
    List<InstallmentDto> Installments);

/// <summary>
/// Data transfer object for a single repayment installment.
/// </summary>
public sealed record InstallmentDto(
    int Number,
    DateTime DueDate,
    decimal PrincipalAmount,
    decimal InterestAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    string Status);
