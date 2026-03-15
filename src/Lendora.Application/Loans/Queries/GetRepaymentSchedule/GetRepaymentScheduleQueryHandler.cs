using Lendora.Application.Common.Interfaces;
using MediatR;

namespace Lendora.Application.Loans.Queries.GetRepaymentSchedule;

/// <summary>
/// Handles <see cref="GetRepaymentScheduleQuery"/> by retrieving the loan's installment
/// schedule from cache or the repository. Caches the result for 1 hour.
/// </summary>
public sealed class GetRepaymentScheduleQueryHandler(
    ILoanRepository loanRepository,
    ICacheService cacheService) : IRequestHandler<GetRepaymentScheduleQuery, RepaymentScheduleDto>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    public async Task<RepaymentScheduleDto> Handle(
        GetRepaymentScheduleQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"loan:{request.LoanId}:schedule";

        var cached = await cacheService.GetAsync<RepaymentScheduleDto>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var loan = await loanRepository.GetWithInstallmentsAsync(request.LoanId, cancellationToken)
            ?? throw new KeyNotFoundException($"Loan with ID '{request.LoanId}' was not found.");

        var installments = loan.Installments
            .OrderBy(i => i.InstallmentNumber)
            .Select(i => new InstallmentDto(
                i.InstallmentNumber,
                i.DueDate,
                i.PrincipalAmount,
                i.InterestAmount,
                i.TotalAmount,
                i.PaidAmount,
                i.Status.ToString()))
            .ToList();

        var dto = new RepaymentScheduleDto(loan.Id, installments);

        await cacheService.SetAsync(cacheKey, dto, CacheTtl, cancellationToken);

        return dto;
    }
}
