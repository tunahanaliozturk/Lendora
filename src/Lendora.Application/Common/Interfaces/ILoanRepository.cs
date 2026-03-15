using Lendora.Domain.Entities;

namespace Lendora.Application.Common.Interfaces;

/// <summary>
/// Repository contract for <see cref="Loan"/> aggregate roots,
/// extending the generic repository with domain-specific queries.
/// </summary>
public interface ILoanRepository : IRepository<Loan>
{
    /// <summary>
    /// Retrieves a loan including its repayment installments.
    /// </summary>
    /// <param name="id">The loan identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loan with installments if found; otherwise <c>null</c>.</returns>
    Task<Loan?> GetWithInstallmentsAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a loan including its repayment installments and payments.
    /// </summary>
    /// <param name="id">The loan identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loan with installments and payments if found; otherwise <c>null</c>.</returns>
    Task<Loan?> GetWithInstallmentsAndPaymentsAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all loans belonging to a specific customer.
    /// </summary>
    /// <param name="customerId">The customer identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of loans for the customer.</returns>
    Task<List<Loan>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all installments across all loans that are overdue as of the specified date.
    /// </summary>
    /// <param name="asOfDate">The reference date for determining overdue status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of overdue repayment installments.</returns>
    Task<List<RepaymentInstallment>> GetOverdueInstallmentsAsync(DateTime asOfDate, CancellationToken cancellationToken);
}
