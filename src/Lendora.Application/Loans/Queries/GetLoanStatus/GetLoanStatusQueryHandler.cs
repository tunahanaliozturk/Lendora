using Lendora.Application.Common.Interfaces;
using Lendora.Domain.Enums;
using MediatR;

namespace Lendora.Application.Loans.Queries.GetLoanStatus;

/// <summary>
/// Handles <see cref="GetLoanStatusQuery"/> by retrieving the loan status from cache
/// or computing it from the loan's installments. Caches the result for 5 minutes.
/// </summary>
public sealed class GetLoanStatusQueryHandler(
    ILoanRepository loanRepository,
    ICacheService cacheService) : IRequestHandler<GetLoanStatusQuery, LoanStatusDto>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public async Task<LoanStatusDto> Handle(
        GetLoanStatusQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"loan:{request.LoanId}:status";

        var cached = await cacheService.GetAsync<LoanStatusDto>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var loan = await loanRepository.GetWithInstallmentsAsync(request.LoanId, cancellationToken)
            ?? throw new KeyNotFoundException($"Loan with ID '{request.LoanId}' was not found.");

        var remainingBalance = loan.Installments
            .Where(i => i.Status != InstallmentStatus.Paid)
            .Sum(i => i.TotalAmount - i.PaidAmount);

        var dto = new LoanStatusDto(
            loan.Id,
            loan.Status.ToString(),
            loan.ApprovedAmount,
            loan.InterestRate,
            loan.TermMonths,
            loan.MonthlyPayment,
            loan.StartDate,
            remainingBalance);

        await cacheService.SetAsync(cacheKey, dto, CacheTtl, cancellationToken);

        return dto;
    }
}
