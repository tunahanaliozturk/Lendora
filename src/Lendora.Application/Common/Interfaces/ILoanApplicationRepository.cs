using Lendora.Domain.Entities;

namespace Lendora.Application.Common.Interfaces;

/// <summary>
/// Repository contract for <see cref="LoanApplication"/> aggregate roots,
/// extending the generic repository with domain-specific queries.
/// </summary>
public interface ILoanApplicationRepository : IRepository<LoanApplication>
{
    /// <summary>
    /// Retrieves a loan application including its associated risk assessments.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loan application with risk assessments if found; otherwise <c>null</c>.</returns>
    Task<LoanApplication?> GetWithRiskAssessmentsAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether the specified customer has a pending (non-terminal) loan application.
    /// </summary>
    /// <param name="customerId">The customer identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if a pending application exists; otherwise <c>false</c>.</returns>
    Task<bool> HasPendingApplicationAsync(Guid customerId, CancellationToken cancellationToken);
}
