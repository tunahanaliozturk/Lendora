using FluentValidation;
using Lendora.Application.LoanApplications.Commands.SubmitLoanApplication;

namespace Lendora.Application.Validators;

/// <summary>
/// Validates <see cref="SubmitLoanApplicationCommand"/> to ensure all input parameters
/// meet business constraints before the handler is invoked.
/// </summary>
public sealed class SubmitLoanApplicationCommandValidator : AbstractValidator<SubmitLoanApplicationCommand>
{
    public SubmitLoanApplicationCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required.");

        RuleFor(x => x.RequestedAmount)
            .GreaterThan(0)
            .WithMessage("Requested amount must be greater than zero.")
            .LessThanOrEqualTo(1_000_000)
            .WithMessage("Requested amount cannot exceed 1,000,000.");

        RuleFor(x => x.RequestedTermMonths)
            .InclusiveBetween(6, 360)
            .WithMessage("Requested term must be between 6 and 360 months.");

        RuleFor(x => x.AnnualIncome)
            .GreaterThan(0)
            .WithMessage("Annual income must be greater than zero.");

        RuleFor(x => x.ExistingMonthlyDebt)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Existing monthly debt cannot be negative.");
    }
}
