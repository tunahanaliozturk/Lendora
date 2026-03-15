using Lendora.Application.Common.Interfaces;
using MediatR;

namespace Lendora.Application.Loans.Queries.GetCustomerLoans;

/// <summary>
/// Handles <see cref="GetCustomerLoansQuery"/> by retrieving all loans for a customer
/// and mapping them to summary DTOs.
/// </summary>
public sealed class GetCustomerLoansQueryHandler(
    ILoanRepository loanRepository) : IRequestHandler<GetCustomerLoansQuery, List<CustomerLoanSummaryDto>>
{
    public async Task<List<CustomerLoanSummaryDto>> Handle(
        GetCustomerLoansQuery request,
        CancellationToken cancellationToken)
    {
        var loans = await loanRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        return loans
            .Select(loan => new CustomerLoanSummaryDto(
                loan.Id,
                loan.ApprovedAmount,
                loan.Status.ToString(),
                loan.StartDate,
                loan.MonthlyPayment))
            .ToList();
    }
}
