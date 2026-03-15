using FluentValidation;
using Lendora.Application.LoanApplications.Commands.ApproveLoanApplication;

namespace Lendora.Application.Validators;

/// <summary>
/// Validates <see cref="ApproveLoanApplicationCommand"/> to ensure approval parameters
/// are within acceptable business ranges.
/// </summary>
public sealed class ApproveLoanApplicationCommandValidator : AbstractValidator<ApproveLoanApplicationCommand>
{
    public ApproveLoanApplicationCommandValidator()
    {
        RuleFor(x => x.ApprovedBy)
            .NotEmpty()
            .WithMessage("Approver identifier is required.");

        RuleFor(x => x.InterestRate)
            .GreaterThan(0)
            .WithMessage("Interest rate must be greater than zero.")
            .LessThanOrEqualTo(30)
            .WithMessage("Interest rate cannot exceed 30%.");

        RuleFor(x => x.ApprovedAmount)
            .GreaterThan(0)
            .WithMessage("Approved amount must be greater than zero.");

        RuleFor(x => x.ApprovedTermMonths)
            .InclusiveBetween(6, 360)
            .WithMessage("Approved term must be between 6 and 360 months.");
    }
}
