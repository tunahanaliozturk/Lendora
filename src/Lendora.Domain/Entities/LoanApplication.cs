using Lendora.Domain.Common;
using Lendora.Domain.Enums;
using Lendora.Domain.Events;
using Lendora.Domain.Exceptions;
using Stateless;

namespace Lendora.Domain.Entities;

/// <summary>
/// Aggregate root representing a customer's loan application.
/// Uses a state machine to enforce valid lifecycle transitions.
/// </summary>
public class LoanApplication : AggregateRoot
{
    internal enum LoanApplicationTrigger
    {
        Evaluate,
        Approve,
        Reject
    }

    private readonly StateMachine<LoanApplicationStatus, LoanApplicationTrigger> _stateMachine;

    /// <summary>
    /// The customer who submitted this application.
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// The loan amount requested by the customer.
    /// </summary>
    public decimal RequestedAmount { get; private set; }

    /// <summary>
    /// The loan term in months requested by the customer.
    /// </summary>
    public int RequestedTermMonths { get; private set; }

    /// <summary>
    /// The customer's stated annual income.
    /// </summary>
    public decimal AnnualIncome { get; private set; }

    /// <summary>
    /// The customer's existing monthly debt obligations.
    /// </summary>
    public decimal ExistingMonthlyDebt { get; private set; }

    /// <summary>
    /// The interest rate assigned upon approval.
    /// </summary>
    public decimal? InterestRate { get; private set; }

    /// <summary>
    /// The risk score determined by the evaluation engine.
    /// </summary>
    public int? RiskScore { get; private set; }

    /// <summary>
    /// Current status of the application.
    /// </summary>
    public LoanApplicationStatus Status { get; private set; }

    /// <summary>
    /// Reason provided when the application is rejected.
    /// </summary>
    public string? RejectionReason { get; private set; }

    /// <summary>
    /// Identifier of the approver (user or system) who approved the application.
    /// </summary>
    public string? ApprovedBy { get; private set; }

    /// <summary>
    /// The amount approved, which may differ from the requested amount.
    /// </summary>
    public decimal? ApprovedAmount { get; private set; }

    /// <summary>
    /// The approved term in months, which may differ from the requested term.
    /// </summary>
    public int? ApprovedTermMonths { get; private set; }

    private LoanApplication()
    {
        // EF Core constructor - state machine must still be initialized
        _stateMachine = CreateStateMachine();
    }

    private LoanApplication(
        Guid customerId,
        decimal requestedAmount,
        int requestedTermMonths,
        decimal annualIncome,
        decimal existingMonthlyDebt)
    {
        CustomerId = customerId;
        RequestedAmount = requestedAmount;
        RequestedTermMonths = requestedTermMonths;
        AnnualIncome = annualIncome;
        ExistingMonthlyDebt = existingMonthlyDebt;
        Status = LoanApplicationStatus.Submitted;

        _stateMachine = CreateStateMachine();
    }

    private StateMachine<LoanApplicationStatus, LoanApplicationTrigger> CreateStateMachine()
    {
        var machine = new StateMachine<LoanApplicationStatus, LoanApplicationTrigger>(
            () => Status,
            s => Status = s);

        machine.Configure(LoanApplicationStatus.Submitted)
            .Permit(LoanApplicationTrigger.Evaluate, LoanApplicationStatus.RiskEvaluating);

        machine.Configure(LoanApplicationStatus.RiskEvaluating)
            .Permit(LoanApplicationTrigger.Approve, LoanApplicationStatus.Approved)
            .Permit(LoanApplicationTrigger.Reject, LoanApplicationStatus.Rejected);

        return machine;
    }

    /// <summary>
    /// Factory method to create a new loan application.
    /// </summary>
    /// <param name="customerId">The customer submitting the application.</param>
    /// <param name="requestedAmount">The desired loan amount.</param>
    /// <param name="requestedTermMonths">The desired loan term in months.</param>
    /// <param name="annualIncome">The customer's annual income.</param>
    /// <param name="existingMonthlyDebt">The customer's existing monthly debt.</param>
    /// <returns>A new <see cref="LoanApplication"/> in Submitted status.</returns>
    public static LoanApplication Create(
        Guid customerId,
        decimal requestedAmount,
        int requestedTermMonths,
        decimal annualIncome,
        decimal existingMonthlyDebt)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));
        if (requestedAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(requestedAmount), "Requested amount must be positive.");
        if (requestedTermMonths <= 0)
            throw new ArgumentOutOfRangeException(nameof(requestedTermMonths), "Term must be at least 1 month.");
        if (annualIncome < 0)
            throw new ArgumentOutOfRangeException(nameof(annualIncome), "Annual income cannot be negative.");
        if (existingMonthlyDebt < 0)
            throw new ArgumentOutOfRangeException(nameof(existingMonthlyDebt), "Existing monthly debt cannot be negative.");

        var application = new LoanApplication(customerId, requestedAmount, requestedTermMonths, annualIncome, existingMonthlyDebt);

        application.AddDomainEvent(new LoanApplicationSubmittedEvent(application.Id, customerId));

        return application;
    }

    /// <summary>
    /// Transitions the application to RiskEvaluating status.
    /// </summary>
    public void StartRiskEvaluation()
    {
        FireTrigger(LoanApplicationTrigger.Evaluate);
    }

    /// <summary>
    /// Records the risk score from the evaluation engine.
    /// Must be called while in the RiskEvaluating state.
    /// </summary>
    /// <param name="score">The calculated risk score.</param>
    public void CompleteRiskEvaluation(int score)
    {
        if (Status != LoanApplicationStatus.RiskEvaluating)
            throw new DomainException($"Cannot complete risk evaluation when application is in '{Status}' status.");

        RiskScore = score;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Approves the loan application with the specified terms.
    /// </summary>
    /// <param name="approvedBy">Identifier of the approver.</param>
    /// <param name="interestRate">The assigned annual interest rate.</param>
    /// <param name="approvedAmount">The approved loan amount.</param>
    /// <param name="approvedTermMonths">The approved term in months.</param>
    public void Approve(string approvedBy, decimal interestRate, decimal approvedAmount, int approvedTermMonths)
    {
        if (string.IsNullOrWhiteSpace(approvedBy))
            throw new ArgumentException("Approver identifier is required.", nameof(approvedBy));
        if (interestRate < 0)
            throw new ArgumentOutOfRangeException(nameof(interestRate), "Interest rate cannot be negative.");
        if (approvedAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(approvedAmount), "Approved amount must be positive.");
        if (approvedTermMonths <= 0)
            throw new ArgumentOutOfRangeException(nameof(approvedTermMonths), "Approved term must be at least 1 month.");

        FireTrigger(LoanApplicationTrigger.Approve);

        ApprovedBy = approvedBy;
        InterestRate = interestRate;
        ApprovedAmount = approvedAmount;
        ApprovedTermMonths = approvedTermMonths;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LoanApplicationApprovedEvent(Id, approvedAmount, interestRate));
    }

    /// <summary>
    /// Rejects the loan application with the specified reason.
    /// </summary>
    /// <param name="reason">The reason for rejection.</param>
    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required.", nameof(reason));

        FireTrigger(LoanApplicationTrigger.Reject);

        RejectionReason = reason;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LoanApplicationRejectedEvent(Id, reason));
    }

    private void FireTrigger(LoanApplicationTrigger trigger)
    {
        try
        {
            _stateMachine.Fire(trigger);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidStateTransitionException(Status.ToString(), trigger.ToString(), ex);
        }
    }
}
