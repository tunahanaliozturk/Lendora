using Lendora.Application.Common.Interfaces;
using Lendora.Domain.Entities;
using Lendora.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Lendora.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for <see cref="Loan"/> aggregate roots.
/// </summary>
public sealed class LoanRepository : ILoanRepository
{
    private readonly LendoraDbContext _context;

    public LoanRepository(LendoraDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Loans
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Loan entity, CancellationToken cancellationToken)
    {
        await _context.Loans.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(Loan entity)
    {
        _context.Loans.Update(entity);
    }

    /// <inheritdoc />
    public void Delete(Loan entity)
    {
        _context.Loans.Remove(entity);
    }

    /// <inheritdoc />
    public async Task<Loan?> GetWithInstallmentsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Loans
            .Include(l => l.Installments)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Loan?> GetWithInstallmentsAndPaymentsAsync(
        Guid id, CancellationToken cancellationToken)
    {
        return await _context.Loans
            .Include(l => l.Installments)
            .Include(l => l.Payments)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<Loan>> GetByCustomerIdAsync(
        Guid customerId, CancellationToken cancellationToken)
    {
        return await _context.Loans
            .Where(l => l.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<RepaymentInstallment>> GetOverdueInstallmentsAsync(
        DateTime asOfDate, CancellationToken cancellationToken)
    {
        return await _context.RepaymentInstallments
            .Where(ri => (ri.Status == InstallmentStatus.Scheduled
                          || ri.Status == InstallmentStatus.PartiallyPaid)
                         && ri.DueDate < asOfDate)
            .ToListAsync(cancellationToken);
    }
}
