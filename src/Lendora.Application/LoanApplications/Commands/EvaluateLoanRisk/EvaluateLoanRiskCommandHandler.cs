using Lendora.Application.Common.Interfaces;
using Lendora.Domain.Entities;
using MediatR;

namespace Lendora.Application.LoanApplications.Commands.EvaluateLoanRisk;

/// <summary>
/// Handles <see cref="EvaluateLoanRiskCommand"/> by computing a risk score based on
/// the applicant's financial profile, creating a <see cref="RiskAssessment"/>, and
/// transitioning the application state accordingly.
/// </summary>
/// <remarks>
/// Scoring formula:
///   score = 850 - (debtToIncomeRatio * 200) - (amountToIncomeRatio * 100) - termPenalty
///   Clamped to [300, 850].
///
/// Decision thresholds:
///   score >= 650 => "Approve" (auto-continue)
///   score &lt; 500  => "Reject" (auto-reject)
///   500..649      => "Manual Review" (stays in RiskEvaluating)
/// </remarks>
public sealed class EvaluateLoanRiskCommandHandler(
    ILoanApplicationRepository loanApplicationRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<EvaluateLoanRiskCommand, RiskEvaluationResult>
{
    public async Task<RiskEvaluationResult> Handle(
        EvaluateLoanRiskCommand request,
        CancellationToken cancellationToken)
    {
        var application = await loanApplicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Loan application with ID '{request.ApplicationId}' was not found.");

        application.StartRiskEvaluation();

        var score = CalculateRiskScore(application);

        application.CompleteRiskEvaluation(score);

        var decision = DetermineDecision(score);

        var riskAssessment = new RiskAssessment(
            application.Id,
            score,
            decision,
            dateTimeProvider.UtcNow);

        if (score < 500)
        {
            application.Reject($"Automatic rejection: risk score {score} is below the minimum threshold of 500.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RiskEvaluationResult(score, decision, application.Status.ToString());
    }

    private static int CalculateRiskScore(LoanApplication application)
    {
        var monthlyIncome = application.AnnualIncome / 12m;

        var debtToIncomeRatio = monthlyIncome > 0
            ? application.ExistingMonthlyDebt / monthlyIncome
            : 1m;

        var amountToIncomeRatio = application.AnnualIncome > 0
            ? application.RequestedAmount / application.AnnualIncome
            : 10m;

        var termPenalty = application.RequestedTermMonths switch
        {
            <= 12 => 0,
            <= 36 => 10,
            <= 60 => 25,
            <= 120 => 50,
            <= 240 => 75,
            _ => 100
        };

        var rawScore = 850m - (debtToIncomeRatio * 200m) - (amountToIncomeRatio * 100m) - termPenalty;

        return (int)Math.Clamp(rawScore, 300m, 850m);
    }

    private static string DetermineDecision(int score) => score switch
    {
        >= 650 => "Approve",
        < 500 => "Reject",
        _ => "Manual Review"
    };
}
