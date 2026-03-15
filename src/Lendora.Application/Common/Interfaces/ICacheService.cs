namespace Lendora.Application.Common.Interfaces;

/// <summary>
/// Abstraction over a distributed cache for storing and retrieving serialized objects.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieves a cached value by key.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the cached value into.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached value if found; otherwise <c>null</c>.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Stores a value in the cache.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiry">Optional time-to-live. If <c>null</c>, uses the provider default.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a cached entry by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken);
}
