using Lendora.Api.Contracts.Requests;
using Lendora.Application.Loans.Commands.RegisterPayment;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lendora.Api.Controllers;

/// <summary>
/// Handles loan payment registration.
/// </summary>
[ApiController]
[Route("api/v1/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly ISender _sender;

    public PaymentsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Registers a payment against a loan.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RegisterPaymentResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterPaymentCommand(
            request.LoanId,
            request.Amount,
            request.PaidAt,
            request.PaymentReference);

        var result = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(
            null,
            new { id = result.PaymentId },
            result);
    }
}
