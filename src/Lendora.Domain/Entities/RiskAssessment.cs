using Lendora.Domain.Common;

namespace Lendora.Domain.Entities;

/// <summary>
/// Represents the result of a risk evaluation performed on a loan application.
/// </summary>
public class RiskAssessment : BaseEntity
{
    /// <summary>
    /// The loan application that was evaluated.
    /// </summary>
    public Guid ApplicationId { get; private set; }

    /// <summary>
    /// Numeric risk score assigned by the evaluation engine.
    /// </summary>
    public int Score { get; private set; }

    /// <summary>
    /// The decision outcome (e.g., "Approve", "Reject").
    /// </summary>
    public string Decision { get; private set; } = default!;

    /// <summary>
    /// UTC timestamp indicating when the evaluation was performed.
    /// </summary>
    public DateTime EvaluatedAt { get; private set; }

    private RiskAssessment() { } // EF Core

    public RiskAssessment(Guid applicationId, int score, string decision, DateTime evaluatedAt)
    {
        if (string.IsNullOrWhiteSpace(decision))
            throw new ArgumentException("Decision cannot be empty.", nameof(decision));

        ApplicationId = applicationId;
        Score = score;
        Decision = decision;
        EvaluatedAt = evaluatedAt;
    }
}
