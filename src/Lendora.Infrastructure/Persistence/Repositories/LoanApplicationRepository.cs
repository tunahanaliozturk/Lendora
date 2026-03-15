using Lendora.Application.Common.Interfaces;
using Lendora.Domain.Entities;
using Lendora.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Lendora.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for <see cref="LoanApplication"/> aggregate roots.
/// </summary>
public sealed class LoanApplicationRepository : ILoanApplicationRepository
{
    private readonly LendoraDbContext _context;

    public LoanApplicationRepository(LendoraDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<LoanApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.LoanApplications
            .FirstOrDefaultAsync(la => la.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(LoanApplication entity, CancellationToken cancellationToken)
    {
        await _context.LoanApplications.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(LoanApplication entity)
    {
        _context.LoanApplications.Update(entity);
    }

    /// <inheritdoc />
    public void Delete(LoanApplication entity)
    {
        _context.LoanApplications.Remove(entity);
    }

    /// <inheritdoc />
    public async Task<LoanApplication?> GetWithRiskAssessmentsAsync(
        Guid id, CancellationToken cancellationToken)
    {
        // LoanApplication does not have a navigation property for RiskAssessments,
        // so we load the application and its assessments separately, then let EF Core
        // fix up the relationship via the tracked RiskAssessment entities.
        var application = await _context.LoanApplications
            .FirstOrDefaultAsync(la => la.Id == id, cancellationToken);

        if (application is null)
            return null;

        // Load related risk assessments into the context so they are available
        // through the relationship configured in EF Core.
        await _context.RiskAssessments
            .Where(ra => ra.ApplicationId == id)
            .LoadAsync(cancellationToken);

        return application;
    }

    /// <inheritdoc />
    public async Task<bool> HasPendingApplicationAsync(
        Guid customerId, CancellationToken cancellationToken)
    {
        return await _context.LoanApplications.AnyAsync(
            la => la.CustomerId == customerId
                  && (la.Status == LoanApplicationStatus.Submitted
                      || la.Status == LoanApplicationStatus.RiskEvaluating),
            cancellationToken);
    }
}
