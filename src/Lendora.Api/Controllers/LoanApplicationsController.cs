using Lendora.Api.Contracts.Requests;
using Lendora.Application.LoanApplications.Commands.ApproveLoanApplication;
using Lendora.Application.LoanApplications.Commands.EvaluateLoanRisk;
using Lendora.Application.LoanApplications.Commands.RejectLoanApplication;
using Lendora.Application.LoanApplications.Commands.SubmitLoanApplication;
using Lendora.Application.LoanApplications.Queries.GetLoanApplication;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lendora.Api.Controllers;

/// <summary>
/// Manages the lifecycle of loan applications: submission, risk evaluation, approval, and rejection.
/// </summary>
[ApiController]
[Route("api/v1/loan-applications")]
public sealed class LoanApplicationsController : ControllerBase
{
    private readonly ISender _sender;

    public LoanApplicationsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Submits a new loan application.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitLoanApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SubmitLoanApplicationCommand(
            request.CustomerId,
            request.RequestedAmount,
            request.RequestedTermMonths,
            request.AnnualIncome,
            request.ExistingMonthlyDebt);

        var applicationId = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(Get),
            new { id = applicationId },
            new { id = applicationId });
    }

    /// <summary>
    /// Triggers risk evaluation for a loan application.
    /// </summary>
    [HttpPost("{id:guid}/evaluate")]
    [ProducesResponseType(typeof(RiskEvaluationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Evaluate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new EvaluateLoanRiskCommand(id);
        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Approves a loan application, creating a loan with a repayment schedule.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApproveLoanApplicationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApproveLoanApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ApproveLoanApplicationCommand(
            id,
            request.ApprovedBy,
            request.InterestRate,
            request.ApprovedAmount,
            request.ApprovedTermMonths);

        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Rejects a loan application.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectLoanApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RejectLoanApplicationCommand(
            id,
            request.RejectedBy,
            request.Reason);

        await _sender.Send(command, cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Retrieves a loan application by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoanApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetLoanApplicationQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }
}
