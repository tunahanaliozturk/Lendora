using MediatR;

namespace Lendora.Application.LoanApplications.Queries.GetLoanApplication;

/// <summary>
/// Query to retrieve a loan application with its associated risk assessments.
/// </summary>
public sealed record GetLoanApplicationQuery(Guid ApplicationId) : IRequest<LoanApplicationDto>;

/// <summary>
/// Data transfer object representing a loan application and its risk evaluations.
/// </summary>
public sealed record LoanApplicationDto(
    Guid Id,
    Guid CustomerId,
    decimal RequestedAmount,
    int RequestedTermMonths,
    decimal AnnualIncome,
    decimal ExistingMonthlyDebt,
    string Status,
    int? RiskScore,
    decimal? InterestRate,
    string? ApprovedBy,
    decimal? ApprovedAmount,
    int? ApprovedTermMonths,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<RiskAssessmentDto> RiskAssessments);

/// <summary>
/// Data transfer object representing a single risk assessment result.
/// </summary>
public sealed record RiskAssessmentDto(
    Guid Id,
    int Score,
    string Decision,
    DateTime EvaluatedAt);
