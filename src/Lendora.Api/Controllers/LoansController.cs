using Lendora.Application.Loans.Queries.GetCustomerLoans;
using Lendora.Application.Loans.Queries.GetLoanStatus;
using Lendora.Application.Loans.Queries.GetRepaymentSchedule;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lendora.Api.Controllers;

/// <summary>
/// Provides read access to active loans, their status, and repayment schedules.
/// </summary>
[ApiController]
[Route("api/v1/loans")]
public sealed class LoansController : ControllerBase
{
    private readonly ISender _sender;

    public LoansController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves the current status of a loan.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoanStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetLoanStatusQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves the repayment schedule for a loan.
    /// </summary>
    [HttpGet("{id:guid}/schedule")]
    [ProducesResponseType(typeof(RepaymentScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchedule(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetRepaymentScheduleQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves all loans for a given customer.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(List<CustomerLoanSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerLoans(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerLoansQuery(customerId);
        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }
}
