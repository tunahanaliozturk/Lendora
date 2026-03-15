using Lendora.Domain.Common;

namespace Lendora.Application.Common.Interfaces;

/// <summary>
/// Generic repository contract for aggregate roots.
/// Provides basic CRUD operations following the repository pattern.
/// </summary>
/// <typeparam name="T">The aggregate root type.</typeparam>
public interface IRepository<T> where T : AggregateRoot
{
    /// <summary>
    /// Retrieves an aggregate root by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The aggregate root if found; otherwise <c>null</c>.</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new aggregate root to the repository.
    /// </summary>
    /// <param name="entity">The aggregate root to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(T entity, CancellationToken cancellationToken);

    /// <summary>
    /// Marks an existing aggregate root as modified.
    /// </summary>
    /// <param name="entity">The aggregate root to update.</param>
    void Update(T entity);

    /// <summary>
    /// Marks an aggregate root for deletion.
    /// </summary>
    /// <param name="entity">The aggregate root to delete.</param>
    void Delete(T entity);
}
