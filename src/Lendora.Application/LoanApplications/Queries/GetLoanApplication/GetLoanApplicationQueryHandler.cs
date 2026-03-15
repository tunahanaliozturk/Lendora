using Lendora.Application.Common.Interfaces;
using MediatR;

namespace Lendora.Application.LoanApplications.Queries.GetLoanApplication;

/// <summary>
/// Handles <see cref="GetLoanApplicationQuery"/> by retrieving the application
/// with its risk assessments and mapping to a DTO.
/// </summary>
/// <remarks>
/// The <see cref="ILoanApplicationRepository.GetWithRiskAssessmentsAsync"/> method is
/// expected to eagerly load associated risk assessments. Since <c>LoanApplication</c>
/// does not expose a navigation property for risk assessments (they are a separate entity
/// linked via <c>ApplicationId</c>), the infrastructure repository implementation is
/// responsible for projecting the full result set. This handler maps the application
/// properties directly; risk assessment mapping will be completed when the infrastructure
/// layer exposes the joined data (e.g., via EF Core shadow navigation or a custom projection).
/// </remarks>
public sealed class GetLoanApplicationQueryHandler(
    ILoanApplicationRepository loanApplicationRepository)
    : IRequestHandler<GetLoanApplicationQuery, LoanApplicationDto>
{
    public async Task<LoanApplicationDto> Handle(
        GetLoanApplicationQuery request,
        CancellationToken cancellationToken)
    {
        var application = await loanApplicationRepository.GetWithRiskAssessmentsAsync(
            request.ApplicationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Loan application with ID '{request.ApplicationId}' was not found.");

        return new LoanApplicationDto(
            application.Id,
            application.CustomerId,
            application.RequestedAmount,
            application.RequestedTermMonths,
            application.AnnualIncome,
            application.ExistingMonthlyDebt,
            application.Status.ToString(),
            application.RiskScore,
            application.InterestRate,
            application.ApprovedBy,
            application.ApprovedAmount,
            application.ApprovedTermMonths,
            application.RejectionReason,
            application.CreatedAt,
            application.UpdatedAt,
            RiskAssessments: []);
    }
}
