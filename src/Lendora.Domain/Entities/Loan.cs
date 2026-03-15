using Lendora.Domain.Common;
using Lendora.Domain.Enums;
using Lendora.Domain.Events;
using Lendora.Domain.Exceptions;
using Stateless;

namespace Lendora.Domain.Entities;

/// <summary>
/// Aggregate root representing an active loan disbursed from an approved application.
/// Manages its repayment installments and payment lifecycle via a state machine.
/// </summary>
public class Loan : AggregateRoot
{
    internal enum LoanTrigger
    {
        MarkDelinquent,
        Close
    }

    private readonly StateMachine<LoanStatus, LoanTrigger> _stateMachine;
    private readonly List<RepaymentInstallment> _installments = [];
    private readonly List<Payment> _payments = [];

    /// <summary>
    /// The approved application this loan was created from.
    /// </summary>
    public Guid ApplicationId { get; private set; }

    /// <summary>
    /// The customer who owns this loan.
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// The approved loan principal amount.
    /// </summary>
    public decimal ApprovedAmount { get; private set; }

    /// <summary>
    /// The annual interest rate applied to this loan.
    /// </summary>
    public decimal InterestRate { get; private set; }

    /// <summary>
    /// The loan term in months.
    /// </summary>
    public int TermMonths { get; private set; }

    /// <summary>
    /// The date the loan was activated.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Current lifecycle status of the loan.
    /// </summary>
    public LoanStatus Status { get; private set; }

    /// <summary>
    /// The calculated fixed monthly payment amount.
    /// </summary>
    public decimal MonthlyPayment { get; private set; }

    /// <summary>
    /// The scheduled repayment installments for this loan.
    /// </summary>
    public IReadOnlyCollection<RepaymentInstallment> Installments => _installments.AsReadOnly();

    /// <summary>
    /// Payments made against this loan.
    /// </summary>
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private Loan()
    {
        // EF Core constructor
        _stateMachine = CreateStateMachine();
    }

    private Loan(
        Guid applicationId,
        Guid customerId,
        decimal approvedAmount,
        decimal interestRate,
        int termMonths,
        DateTime startDate,
        decimal monthlyPayment)
    {
        ApplicationId = applicationId;
        CustomerId = customerId;
        ApprovedAmount = approvedAmount;
        InterestRate = interestRate;
        TermMonths = termMonths;
        StartDate = startDate;
        MonthlyPayment = monthlyPayment;
        Status = LoanStatus.Active;

        _stateMachine = CreateStateMachine();
    }

    private StateMachine<LoanStatus, LoanTrigger> CreateStateMachine()
    {
        var machine = new StateMachine<LoanStatus, LoanTrigger>(
            () => Status,
            s => Status = s);

        machine.Configure(LoanStatus.Active)
            .Permit(LoanTrigger.MarkDelinquent, LoanStatus.Delinquent)
            .Permit(LoanTrigger.Close, LoanStatus.Closed);

        machine.Configure(LoanStatus.Delinquent)
            .Permit(LoanTrigger.Close, LoanStatus.Closed);

        return machine;
    }

    /// <summary>
    /// Factory method to create a new loan from an approved application.
    /// Computes the fixed monthly payment using standard amortization formula.
    /// </summary>
    /// <param name="applicationId">The approved application ID.</param>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="approvedAmount">The principal amount.</param>
    /// <param name="annualInterestRate">The annual interest rate (e.g., 0.12 for 12%).</param>
    /// <param name="termMonths">The loan term in months.</param>
    /// <param name="startDate">The date the loan is activated.</param>
    /// <returns>A new <see cref="Loan"/> in Active status.</returns>
    public static Loan Create(
        Guid applicationId,
        Guid customerId,
        decimal approvedAmount,
        decimal annualInterestRate,
        int termMonths,
        DateTime startDate)
    {
        if (applicationId == Guid.Empty)
            throw new ArgumentException("Application ID cannot be empty.", nameof(applicationId));
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));
        if (approvedAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(approvedAmount), "Approved amount must be positive.");
        if (annualInterestRate < 0)
            throw new ArgumentOutOfRangeException(nameof(annualInterestRate), "Interest rate cannot be negative.");
        if (termMonths <= 0)
            throw new ArgumentOutOfRangeException(nameof(termMonths), "Term must be at least 1 month.");

        var monthlyPayment = CalculateMonthlyPayment(approvedAmount, annualInterestRate, termMonths);

        var loan = new Loan(
            applicationId,
            customerId,
            approvedAmount,
            annualInterestRate,
            termMonths,
            startDate,
            monthlyPayment);

        loan.AddDomainEvent(new LoanActivatedEvent(loan.Id, applicationId));

        return loan;
    }

    /// <summary>
    /// Adds a repayment installment to this loan's schedule.
    /// </summary>
    /// <param name="installment">The installment to add.</param>
    public void AddInstallment(RepaymentInstallment installment)
    {
        ArgumentNullException.ThrowIfNull(installment);

        if (installment.LoanId != Id)
            throw new DomainException("Installment does not belong to this loan.");

        _installments.Add(installment);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Registers a payment against this loan. The payment is applied to outstanding installments
    /// in order of installment number. If all installments become paid, the loan is closed.
    /// </summary>
    /// <param name="amount">The payment amount.</param>
    /// <param name="paidAt">The UTC timestamp of the payment.</param>
    /// <param name="paymentReference">Optional external payment reference.</param>
    public void RegisterPayment(decimal amount, DateTime paidAt, string? paymentReference = null)
    {
        if (Status == LoanStatus.Closed)
            throw new DomainException("Cannot register payment on a closed loan.");

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be positive.");

        var payment = new Payment(Id, amount, paidAt, paymentReference);
        _payments.Add(payment);

        // Apply payment to outstanding installments in order
        var remaining = amount;
        var orderedInstallments = _installments
            .Where(i => i.Status != InstallmentStatus.Paid)
            .OrderBy(i => i.InstallmentNumber);

        foreach (var installment in orderedInstallments)
        {
            if (remaining <= 0)
                break;

            var applied = installment.ApplyPayment(remaining);
            remaining -= applied;
        }

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentRegisteredEvent(Id, payment.Id, amount));

        // If all installments are now paid, close the loan
        if (_installments.Count > 0 && _installments.All(i => i.Status == InstallmentStatus.Paid))
        {
            FireTrigger(LoanTrigger.Close);
            AddDomainEvent(new LoanClosedEvent(Id));
        }
    }

    /// <summary>
    /// Marks the loan as delinquent due to missed or late payments.
    /// </summary>
    public void MarkAsDelinquent()
    {
        FireTrigger(LoanTrigger.MarkDelinquent);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Closes the loan. Typically invoked when all installments are paid or via manual override.
    /// </summary>
    public void Close()
    {
        FireTrigger(LoanTrigger.Close);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LoanClosedEvent(Id));
    }

    private void FireTrigger(LoanTrigger trigger)
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

    /// <summary>
    /// Calculates the fixed monthly payment using the standard amortization formula:
    /// M = P * [r(1+r)^n] / [(1+r)^n - 1]
    /// where P = principal, r = monthly rate, n = number of payments.
    /// For zero-interest loans, simply divides principal by number of months.
    /// </summary>
    private static decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int termMonths)
    {
        if (annualRate == 0)
            return Math.Round(principal / termMonths, 2);

        var monthlyRate = (double)annualRate / 12;
        var power = Math.Pow(1 + monthlyRate, termMonths);
        var payment = (double)principal * (monthlyRate * power) / (power - 1);

        return Math.Round((decimal)payment, 2);
    }
}
