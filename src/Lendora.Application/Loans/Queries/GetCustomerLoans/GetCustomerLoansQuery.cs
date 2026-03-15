using MediatR;

namespace Lendora.Application.Loans.Queries.GetCustomerLoans;

/// <summary>
/// Query to retrieve all loans belonging to a specific customer.
/// </summary>
public sealed record GetCustomerLoansQuery(Guid CustomerId) : IRequest<List<CustomerLoanSummaryDto>>;

/// <summary>
/// Summary data transfer object for a customer's loan.
/// </summary>
public sealed record CustomerLoanSummaryDto(
    Guid Id,
    decimal ApprovedAmount,
    string Status,
    DateTime StartDate,
    decimal MonthlyPayment);
