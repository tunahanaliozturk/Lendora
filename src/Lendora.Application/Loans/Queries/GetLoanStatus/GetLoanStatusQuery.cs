using MediatR;

namespace Lendora.Application.Loans.Queries.GetLoanStatus;

/// <summary>
/// Query to retrieve the current status summary of a loan.
/// </summary>
public sealed record GetLoanStatusQuery(Guid LoanId) : IRequest<LoanStatusDto>;

/// <summary>
/// Data transfer object representing the current status of a loan.
/// </summary>
public sealed record LoanStatusDto(
    Guid Id,
    string Status,
    decimal ApprovedAmount,
    decimal InterestRate,
    int TermMonths,
    decimal MonthlyPayment,
    DateTime StartDate,
    decimal RemainingBalance);
