using MediatR;

namespace Lendora.Application.LoanApplications.Commands.EvaluateLoanRisk;

/// <summary>
/// Command to trigger risk evaluation for a submitted loan application.
/// </summary>
public sealed record EvaluateLoanRiskCommand(Guid ApplicationId) : IRequest<RiskEvaluationResult>;

/// <summary>
/// Result of a risk evaluation, containing the computed score, decision, and resulting status.
/// </summary>
public sealed record RiskEvaluationResult(int Score, string Decision, string Status);
