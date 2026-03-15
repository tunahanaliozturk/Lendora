using FluentValidation;
using Lendora.Application.Loans.Commands.RegisterPayment;

namespace Lendora.Application.Validators;

/// <summary>
/// Validates <see cref="RegisterPaymentCommand"/> to ensure payment parameters are valid.
/// </summary>
public sealed class RegisterPaymentCommandValidator : AbstractValidator<RegisterPaymentCommand>
{
    public RegisterPaymentCommandValidator()
    {
        RuleFor(x => x.LoanId)
            .NotEmpty()
            .WithMessage("Loan ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Payment amount must be greater than zero.");
    }
}
