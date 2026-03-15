namespace Lendora.Application.Common.Interfaces;

/// <summary>
/// Abstracts the persistence boundary, ensuring all changes within a business operation
/// are committed atomically.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes to the underlying store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the store.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
